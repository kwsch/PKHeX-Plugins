using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PKHeX.Core.Injection;

public enum RWMethod
{
    Heap,
    Main,
    Absolute,
}

public sealed class SysBotMini : ICommunicatorNX, IPokeBlocks
{
    public string IP = "192.168.1.65";
    public int Port = 6000;
    public InjectorCommunicationType Protocol = InjectorCommunicationType.SocketNetwork;

    public Socket Connection = new(SocketType.Stream, ProtocolType.Tcp);

    public bool Connected;

    private readonly Lock _sync = new();

    InjectorCommunicationType ICommunicatorNX.Protocol
    {
        get => Protocol;
        set => Protocol = value;
    }
    bool ICommunicator.Connected
    {
        get => Connected;
        set => Connected = value;
    }
    int ICommunicator.Port
    {
        get => Port;
        set => Port = value;
    }
    string ICommunicator.IP
    {
        get => IP;
        set => IP = value;
    }

    public void Connect()
    {
        lock (_sync)
        {
            Connection = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Connection.Connect(IP, Port);
            Connected = true;
        }
    }

    public void Disconnect()
    {
        lock (_sync)
        {
            Connection.Disconnect(false);
            Connected = false;
        }
    }

    private int ReadInternal(Span<byte> buffer)
    {
        int i = 0;
        int br = 0;
        while (true)
        {
            var slice = buffer.Slice(i++, 1);
            int read = Connection.Receive(slice, SocketFlags.None);
            if (read == 0 || slice[0] == '\n')
                return br;
            br += read;
        }
    }

    private int SendInternal(ReadOnlySpan<byte> buffer) => Connection.Send(buffer);

    public int Read(Span<byte> buffer)
    {
        lock (_sync)
            return ReadInternal(buffer);
    }

    public void ReadBytes(ulong offset, int length, RWMethod method, Span<byte> dest)
    {
        lock (_sync)
        {
            var cmd = method switch
            {
                RWMethod.Heap => SwitchCommand.Peek(offset, length),
                RWMethod.Main => SwitchCommand.PeekMain(offset, length),
                RWMethod.Absolute => SwitchCommand.PeekAbsolute(offset, length),
                _ => SwitchCommand.Peek(offset, length),
            };

            SendInternal(cmd);

            // give it time to push data back
            Thread.Sleep((length / 256) + 100);
            ReadResponse(dest[..length]);
        }
    }

    public byte[] ReadBytes(ulong offset, int length, RWMethod method)
    {
        var result = new byte[length];
        ReadBytes(offset, length, method, result);
        return result;
    }

    public byte[] ReadAbsoluteMulti(Dictionary<ulong, int> offsets)
    {
        lock (_sync)
        {
            var cmd = SwitchCommand.PeekAbsoluteMulti(offsets);
            SendInternal(cmd);

            // give it time to push data back
            var length = offsets.Values.Sum();
            Thread.Sleep((length / 256) + 100);
            var result = new byte[length];
            ReadResponse(result);
            return result;
        }
    }

    private void ReadResponse(Span<byte> result)
    {
        var length = result.Length;
        var size = (length * 2) + 1;
        var rent = ArrayPool<byte>.Shared.Rent(size);
        var buffer = rent.AsSpan(0, size);
        _ = ReadInternal(buffer);
        Decoder.ConvertHexByteStringToBytes(buffer, result);
        buffer.Clear();
        ArrayPool<byte>.Shared.Return(rent);
    }

    public void WriteBytes(ReadOnlySpan<byte> data, ulong offset, RWMethod method)
    {
        lock (_sync)
        {
            var cmd = method switch
            {
                RWMethod.Heap => SwitchCommand.Poke(offset, data),
                RWMethod.Main => SwitchCommand.PokeMain(offset, data),
                RWMethod.Absolute => SwitchCommand.PokeAbsolute(offset, data),
                _ => SwitchCommand.Poke(offset, data),
            };

            SendInternal(cmd);

            // give it time to push data back
            Thread.Sleep((data.Length / 256) + 100);
        }
    }

    public void ReadLargeBytes(ulong offset, int length, RWMethod method, Span<byte> dest)
    {
        const int maxlength = 344 * 30;
        while (length > 0)
        {
            var readlength = Math.Min(maxlength, length);
            length -= readlength;
            ReadBytes(offset, readlength, method, dest);
            offset += (ulong)readlength;
            dest = dest[readlength..];
        }
    }

    public byte[] ReadLargeBytes(ulong offset, int length, RWMethod method)
    {
        var result = new byte[length];
        ReadLargeBytes(offset, length, method, result);
        return result;
    }

    public void WriteLargeBytes(ReadOnlySpan<byte> data, ulong offset, RWMethod method)
    {
        const int maxlength = 344 * 30;
        if (data.Length <= maxlength)
        {
            WriteBytes(data, offset, method);
            return;
        }

        int byteCount = data.Length;
        for (int i = 0; i < byteCount; i += maxlength)
        {
            var ba = SliceSafe(data, i, maxlength);
            WriteBytes(ba, offset, method);
            offset += maxlength;
        }
    }

    public static ReadOnlySpan<byte> SliceSafe(ReadOnlySpan<byte> src, int offset, int length)
    {
        var delta = src.Length - offset;
        if (delta < length)
            length = delta;
        return src.Slice(offset, length);
    }

    public ulong GetHeapBase()
    {
        var cmd = SwitchCommand.GetHeapBase();
        SendInternal(cmd);

        Span<byte> result = stackalloc byte[8];
        ReadResponse(result);
        return System.Buffers.Binary.BinaryPrimitives.ReadUInt64BigEndian(result);
    }

    public string GetTitleID()
    {
        var cmd = SwitchCommand.GetTitleID();
        SendInternal(cmd);

        Span<byte> buffer = stackalloc byte[17];
        _ = ReadInternal(buffer);
        return Encoding.ASCII.GetString(buffer).Trim();
    }

    public string GetBotbaseVersion()
    {
        var cmd = SwitchCommand.GetBotbaseVersion();
        SendInternal(cmd);

        var data = FlexRead();
        return Encoding.ASCII.GetString(data).Trim('\0');
    }

    public string GetGameInfo(string info)
    {
        var cmd = SwitchCommand.GetGameInfo(info);
        SendInternal(cmd);

        var data = FlexRead();
        return Encoding.UTF8.GetString(data).Trim('\0', '\n');
    }

    public bool IsProgramRunning(ulong pid)
    {
        var cmd = SwitchCommand.IsProgramRunning(pid);
        SendInternal(cmd);

        Span<byte> buffer = stackalloc byte[17];
        _ = ReadInternal(buffer);
        return ulong.TryParse(Encoding.ASCII.GetString(buffer).Trim(), out var value) && value == 1;
    }

    public byte[] ReadBytes(ulong offset, int length) => ReadLargeBytes(offset, length, RWMethod.Heap);

    public void WriteBytes(ReadOnlySpan<byte> data, ulong offset) => WriteLargeBytes(data, offset, RWMethod.Heap);

    public byte[] ReadBytesMain(ulong offset, int length) => ReadLargeBytes(offset, length, RWMethod.Main);

    public void WriteBytesMain(ReadOnlySpan<byte> data, ulong offset) => WriteLargeBytes(data, offset, RWMethod.Main);

    public byte[] ReadBytesAbsolute(ulong offset, int length) => ReadLargeBytes(offset, length, RWMethod.Absolute);

    public void WriteBytesAbsolute(ReadOnlySpan<byte> data, ulong offset) => WriteLargeBytes(data, offset, RWMethod.Absolute);

    public byte[] ReadBytesAbsoluteMulti(Dictionary<ulong, int> offsets) => ReadAbsoluteMulti(offsets);

    private byte[] FlexRead()
    {
        lock (_sync)
        {
            List<byte> flexBuffer = [];
            int available = Connection.Available;
            Connection.ReceiveTimeout = 1_000;

            do
            {
                byte[] buffer = new byte[available];
                Connection.Receive(buffer, available, SocketFlags.None);
                flexBuffer.AddRange(buffer);

                Thread.Sleep((0x1C0 / 256) + 64);
                available = Connection.Available;
            } while (flexBuffer.Count == 0 || flexBuffer[^1] != (byte)'\n');

            Connection.ReceiveTimeout = 0;
            return [.. flexBuffer];
        }
    }
}