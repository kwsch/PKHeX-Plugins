using System;
using System.Linq;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Core.Injection;

public static class InjectionUtil
{
    public const ulong INVALID_PTR = 0;

    public static ulong GetPointerAddress(this ICommunicatorNX sb, string ptr, bool heapRelative = true)
    {
        if (string.IsNullOrWhiteSpace(ptr) || ptr.AsSpan().IndexOfAny('-', '/', '*') != -1)
            return INVALID_PTR;

        while (ptr.Contains("]]"))
            ptr = ptr.Replace("]]", "]+0]");

        uint finadd = 0;
        if (!ptr.EndsWith(']'))
        {
            finadd = Util.GetHexValue(ptr.Split('+').Last());
            ptr = ptr[..ptr.LastIndexOf('+')];
        }
        var jumps = ptr.Replace("main", "").Replace("[", "").Replace("]", "").Split("+", StringSplitOptions.RemoveEmptyEntries);
        if (jumps.Length == 0)
            return INVALID_PTR;

        var initaddress = Util.GetHexValue(jumps[0].Trim());
        ulong address = ReadUInt64LittleEndian(sb.ReadBytesMain(initaddress, 0x8));
        foreach (var j in jumps)
        {
            var val = Util.GetHexValue(j.Trim());
            if (val == initaddress)
                continue;

            address = ReadUInt64LittleEndian(sb.ReadBytesAbsolute(address + val, 0x8));
        }
        address += finadd;
        if (heapRelative)
        {
            ulong heap = sb.GetHeapBase();
            address -= heap;
        }
        return address;
    }

    public static string ExtendPointer(this string pointer, params uint[] jumps)
    {
        foreach (var jump in jumps)
            pointer = $"[{pointer}]+{jump:X}";

        return pointer;
    }

    public static ulong SearchSaveKey(this PokeSysBotMini psb, string saveblocks, uint key)
    {
        if (psb.com is not ICommunicatorNX nx)
            return 0;

        var ptr = psb.GetCachedPointer(nx, saveblocks, false);
        var dt = nx.ReadBytesAbsolute(ptr + 8, 16).AsSpan();
        var start = ReadUInt64LittleEndian(dt[..8]);
        var end   = ReadUInt64LittleEndian(dt[8..]);
        var size = (ulong)GetBlockSizeSV(psb.Version);

        while (start < end)
        {
            var count = (end - start) / size;
            var mid = start + ((count >> 1) * size);
            var found = ReadUInt32LittleEndian(nx.ReadBytesAbsolute(mid, 4));
            if (found == key)
                return mid;

            if (found >= key)
                end = mid;
            else
                start = mid + size;
        }
        return 0;
    }

    private static int GetBlockSizeSV(LiveHeXVersion version) => version switch
    {
        >= LiveHeXVersion.SV_v130 => 48, // Thanks, santacrab!
        _ => 32,
    };
}