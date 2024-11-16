using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PKHeX.Core.Enhancements;

public static class PKSMUtil
{
    /// <summary>
    /// Creates PKSM's bank.bin to individual <see cref="PKM"/> files (v1)
    /// </summary>
    /// <param name="dir">Folder to export all dumped files to</param>
    public static int CreateBank(string dir)
    {
        var files = Directory.GetFiles(dir, "*.p??", SearchOption.TopDirectoryOnly);
        var ver = Enum.GetValues<PKSMBankVersion>().Cast<int>().Max();
        var version = BitConverter.GetBytes(ver + 1); // Latest bank version
        var pksmsize = GetBankSize((PKSMBankVersion)ver);
        var boxcount = (files.Length / 30) + 1;
        var bank = new byte[8 + 4 + 4 + (boxcount * pksmsize * 30)];
        var ctr = 0;
        var magic = "PKSMBANK"u8.ToArray(); // PKSMBANK
        magic.CopyTo(bank, 0);
        version.CopyTo(bank, 8);
        BitConverter.GetBytes(boxcount).CopyTo(bank, 12); // Number of bank boxes.
        foreach (var f in files)
        {
            var prefer = EntityFileExtension.GetContextFromExtension(f, EntityContext.None);
            var pk = EntityFormat.GetFromBytes(File.ReadAllBytes(f), prefer);
            if (pk == null)
                continue;

            if (pk.Species == 0 && pk.Species >= pk.MaxSpeciesID)
                continue;

            var ofs = 16 + (ctr * pksmsize);
            BitConverter.GetBytes((int)GetPKSMFormat(pk)).CopyTo(bank, ofs);
            pk.DecryptedBoxData.CopyTo(bank, ofs + 4);
            byte[] temp = Enumerable.Repeat((byte)0xFF, pksmsize - pk.DecryptedBoxData.Length - 8).ToArray();
            temp.CopyTo(bank, ofs + pk.DecryptedBoxData.Length + 4);
            temp = Enumerable.Repeat((byte)0x00, 4).ToArray();
            temp.CopyTo(bank, ofs + pksmsize - 4);
            ctr++;
        }
        var empty = (boxcount * 30) - files.Length;
        for (int i = 0; i < empty; i++)
        {
            var ofs = 16 + (ctr * pksmsize);
            byte[] temp = Enumerable.Repeat((byte)0xFF, pksmsize).ToArray();
            temp.CopyTo(bank, ofs);
            ctr++;
        }
        File.WriteAllBytes(Path.Combine(dir, "pksm_1.bnk"), bank);
        return ctr - empty;
    }

    /// <summary>
    /// Exports PKSM's bank.bin to individual <see cref="PKM"/> files (v1)
    /// </summary>
    /// <param name="bank">PKSM format bank storage</param>
    /// <param name="dir">Folder to export all dumped files to</param>
    /// <param name="previews">Preview data</param>
    public static int ExportBank(ReadOnlySpan<byte> bank, string dir, out List<PKMPreview> previews)
    {
        Directory.CreateDirectory(dir);
        var ctr = 0;
        var version = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bank[8..]);
        var ver = (PKSMBankVersion)(version & 0xFF);
        var pkmsize = GetBankSize(ver);
        var start = GetBankStartIndex(ver);
        previews = [];
        for (int i = start; i < bank.Length; i += pkmsize)
        {
            var pk = GetPKSMStoredPKM(bank[i..]);
            if (pk == null)
                continue;

            if (pk.Species == 0 && pk.Species >= pk.MaxSpeciesID)
                continue;

            var strings = GameInfo.Strings;
            previews.Add(new PKMPreview(pk, strings));
            File.WriteAllBytes(Path.Combine(dir, Util.CleanFileName(pk.FileName)), pk.DecryptedPartyData);
            ctr++;
        }
        return ctr;
    }

    private static PKM? GetPKSMStoredPKM(ReadOnlySpan<byte> data)
    {
        // get format
        var metadata = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data);
        var format = (PKSMStorageFormat)(metadata & 0xFF);
        if (format >= PKSMStorageFormat.MAX_COUNT)
            return null;

        data = data[4..];
        // gen4+ presence check; won't work for prior gens
        if (!EntityDetection.IsPresent(data))
            return null;
        int length = GetLength(format);
        if (length == 0)
            return null;

        var raw = data[..length].ToArray();
        return GetFromFormat(raw, format);
    }

    private static PKM GetFromFormat(byte[] data, PKSMStorageFormat format) => format switch
    {
        PKSMStorageFormat.ONE => new PK1(data),
        PKSMStorageFormat.TWO => new PK2(data),
        PKSMStorageFormat.THREE => new PK3(data),
        PKSMStorageFormat.FOUR => new PK4(data),
        PKSMStorageFormat.FIVE => new PK5(data),
        PKSMStorageFormat.SIX => new PK6(data),
        PKSMStorageFormat.SEVEN => new PK7(data),
        PKSMStorageFormat.LGPE => new PB7(data),
        PKSMStorageFormat.EIGHT => new PK8(data),
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
    };

    private static int GetLength(PKSMStorageFormat format) => format switch
    {
        PKSMStorageFormat.ONE => 33,
        PKSMStorageFormat.TWO => 32,
        PKSMStorageFormat.THREE => 80,
        PKSMStorageFormat.FOUR => 136,
        PKSMStorageFormat.FIVE => 136,
        PKSMStorageFormat.SIX => 232,
        PKSMStorageFormat.SEVEN => 232,
        PKSMStorageFormat.LGPE => 232,
        PKSMStorageFormat.EIGHT => 328,
        _ => 0,
    };

    private static int GetBankSize(PKSMBankVersion v) => v switch
    {
        PKSMBankVersion.VERSION1 => 264,
        PKSMBankVersion.VERSION2 => 264,
        PKSMBankVersion.VERSION3 => 336,
        _ => 336,
    };

    private static int GetBankStartIndex(PKSMBankVersion v) => v switch
    {
        PKSMBankVersion.VERSION1 => 12,
        PKSMBankVersion.VERSION2 => 16,
        PKSMBankVersion.VERSION3 => 16,
        _ => 16,
    };

    private static PKSMStorageFormat GetPKSMFormat(PKM pk) => pk switch
    {
        PK1 => PKSMStorageFormat.ONE,
        PK2 => PKSMStorageFormat.TWO,
        PK3 => PKSMStorageFormat.THREE,
        PK4 => PKSMStorageFormat.FOUR,
        PK5 => PKSMStorageFormat.FIVE,
        PK6 => PKSMStorageFormat.SIX,
        PK7 => PKSMStorageFormat.SEVEN,
        PB7 => PKSMStorageFormat.LGPE,
        PK8 => PKSMStorageFormat.EIGHT,
        _ => PKSMStorageFormat.UNUSED,
    };

    private enum PKSMBankVersion
    {
        VERSION1,
        VERSION2,
        VERSION3,
    }

    private enum PKSMStorageFormat
    {
        FOUR,
        FIVE,
        SIX,
        SEVEN,
        LGPE,
        EIGHT,
        ONE,
        TWO,
        THREE,
        MAX_COUNT,
        UNUSED = 0xFF,
    }
}