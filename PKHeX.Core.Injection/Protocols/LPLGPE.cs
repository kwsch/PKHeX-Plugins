using System;
using System.Collections.Generic;
using static PKHeX.Core.Injection.LiveHeXVersion;

namespace PKHeX.Core.Injection;

public sealed class LPLGPE(LiveHeXVersion lv, bool useCache) : InjectionBase(lv, useCache)
{
    public static ReadOnlySpan<LiveHeXVersion> SupportedVersions => [LGPE_v102];

    public override byte[] ReadBox(PokeSysBotMini psb, int box, int len, List<byte[]> allpkm)
    {
        var bytes = psb.com.ReadBytes(psb.GetBoxOffset(box), len);
        if (psb.GapSize == 0)
            return bytes;

        var currofs = 0;
        for (int i = 0; i < psb.SlotCount; i++)
        {
            var StoredLength = psb.SlotSize - 0x1C;
            var stored = bytes.AsSpan().Slice(currofs, StoredLength).ToArray();
            var party = bytes.AsSpan().Slice(currofs + StoredLength + 0x70, 0x1C).ToArray();
            allpkm.Add(ArrayUtil.ConcatAll(stored, party));
            currofs += psb.SlotSize + psb.GapSize;
        }
        return ArrayUtil.ConcatAll(allpkm.ToArray());
    }

    public override byte[] ReadSlot(PokeSysBotMini psb, int box, int slot)
    {
        var bytes = psb.com.ReadBytes(psb.GetSlotOffset(box, slot), psb.SlotSize + psb.GapSize);
        var StoredLength = psb.SlotSize - 0x1C;
        var stored = bytes.AsSpan()[..StoredLength].ToArray();
        var party = bytes.AsSpan().Slice(StoredLength + 0x70, 0x1C).ToArray();
        return ArrayUtil.ConcatAll(stored, party);
    }

    public override void SendSlot(PokeSysBotMini psb, ReadOnlySpan<byte> data, int box, int slot)
    {
        var slotofs = psb.GetSlotOffset(box, slot);
        var StoredLength = psb.SlotSize - 0x1C;
        psb.com.WriteBytes(data[..StoredLength], slotofs);
        psb.com.WriteBytes(data[StoredLength..], slotofs + (ulong)StoredLength + 0x70);
    }

    public override void SendBox(PokeSysBotMini psb, ReadOnlySpan<byte> boxData, int box)
    {
        int size = psb.SlotSize;
        for (int i = 0; i < psb.SlotCount; i++)
            SendSlot(psb, boxData.Slice(i * size, size), box, i);
    }

    public static readonly Func<PokeSysBotMini, byte[]?> GetTrainerData = psb =>
    {
        var lv = psb.Version;
        var ofs = RamOffsets.GetTrainerBlockOffset(lv);
        var size = RamOffsets.GetTrainerBlockSize(lv);
        if (size <= 0 || ofs == 0)
            return null;

        var data = psb.com.ReadBytes(ofs, size);
        return data;
    };
}