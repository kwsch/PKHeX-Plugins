using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static PKHeX.Core.Injection.LiveHeXVersion;

namespace PKHeX.Core.Injection;

public sealed class LPBasic(LiveHeXVersion lv, bool useCache) : InjectionBase(lv, useCache)
{
    public static ReadOnlySpan<LiveHeXVersion> SupportedVersions =>
    [
        SWSH_v132, SWSH_v121, SWSH_v111,
        LGPE_v102,
        ORAS_v140,
        XY_v150,
        US_v120,
        UM_v120,
        SM_v120,
    ];

    private static BlockData Get(uint offset, uint key, string name, string display) => new()
    {
        Name = name,
        Display = display,
        SCBKey = key,
        Offset = offset,
    };

    public static readonly BlockData[] Blocks_Rigel2 =
    [
        Get(0x45068F18, 0xF25C070E, "KMyStatus", "Trainer Data"),
        Get(0x45067A98, 0x1177C2C4, "KItem", "Items"),
        Get(0x45072DF0, 0x1B882B09, "KMisc", "Miscellaneous"),
        Get(0x45127098, 0x874DA6FA, "KTrainerCard", "Trainer Card"),
        Get(0x450748E8, 0xD224F9AC, "KFashionUnlock", "Fashion"),
        Get(0x450C8A70, 0x9033eb7b, "Raid", "Raid"),
        Get(0x450C94D8, 0x158DA896, "RaidArmor", "RaidArmor"),
        Get(0x450C9F40, 0x148DA703, "RaidCrown", "RaidCrown"),
        Get(0x45069120, 0x4716C404, "KZukan", "Pokedex Base"),
        Get(0x45069120, 0x3F936BA9, "KZukanR1", "Pokedex Armor"),
        Get(0x45069120, 0x3C9366F0, "KZukanR2", "Pokedex Crown"),
    ];

    // LiveHexVersion -> Blockname -> List of <SCBlock Keys, OffsetValues>
    public static readonly Dictionary<LiveHeXVersion, BlockData[]> SCBlocks = new()
    {
        { SWSH_v132, Blocks_Rigel2 },
    };

    public override Dictionary<string, string> SpecialBlocks { get; } = new()
    {
        { "Items", "B_OpenItemPouch_Click" },
        { "Raid", "B_OpenRaids_Click" },
        { "RaidArmor", "B_OpenRaids_Click" },
        { "RaidCrown", "B_OpenRaids_Click" },
        { "Pokedex Base", "B_OpenPokedex_Click" },
        { "Pokedex Armor", "B_OpenPokedex_Click" },
        { "Pokedex Crown", "B_OpenPokedex_Click" },
    };

    public override byte[] ReadBox(PokeSysBotMini psb, int box, int len, List<byte[]> allpkm)
    {
        var bytes = psb.com.ReadBytes(psb.GetBoxOffset(box), len).AsSpan();
        if (psb.GapSize == 0)
            return bytes.ToArray();

        var currofs = 0;
        for (int i = 0; i < psb.SlotCount; i++)
        {
            var stored = bytes.Slice(currofs, psb.SlotSize);
            allpkm.Add(stored.ToArray());
            currofs += psb.SlotSize + psb.GapSize;
        }

        return ArrayUtil.ConcatAll(allpkm.ToArray());
    }

    public override byte[] ReadSlot(PokeSysBotMini psb, int box, int slot) => psb.com.ReadBytes(psb.GetSlotOffset(box, slot), psb.SlotSize + psb.GapSize);

    public override void SendSlot(PokeSysBotMini psb, ReadOnlySpan<byte> data, int box, int slot) => psb.com.WriteBytes(data, psb.GetSlotOffset(box, slot));

    public override void SendBox(PokeSysBotMini psb, ReadOnlySpan<byte> boxData, int box)
    {
        var size = psb.SlotSize;
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

    // Reflection method
    public override bool ReadBlockFromString(PokeSysBotMini psb, SaveFile sav, string block, out List<byte[]>? read)
    {
        read = null;
        try
        {
            var offsets = SCBlocks[psb.Version].Where(z => z.Display == block);
            var props = sav.GetType().GetProperty("Blocks") ?? throw new Exception("Blocks don't exist");
            var allblocks = props.GetValue(sav);
            if (allblocks is not SCBlockAccessor scba)
                return false;

            foreach (var sub in offsets)
            {
                var scbkey = sub.SCBKey;
                var offset = sub.Offset;
                var scb = scba.GetBlock(scbkey);
                var ram = psb.com.ReadBytes(offset, scb.Data.Length);
                ram.CopyTo(scb.Data, 0);
                if (read == null)
                {
                    read = [ram];
                }
                else
                {
                    read.Add(ram);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.StackTrace);
            return false;
        }
    }

    public override void WriteBlocksFromSAV(PokeSysBotMini psb, string block, SaveFile sav)
    {
        var props = sav.GetType().GetProperty("Blocks") ?? throw new Exception("Blocks don't exist");
        var allblocks = props.GetValue(sav);
        if (allblocks is not SCBlockAccessor scba)
            return;

        var offsets = SCBlocks[psb.Version].Where(z => z.Display == block);
        foreach (var sub in offsets)
        {
            var scbkey = sub.SCBKey;
            var offset = sub.Offset;
            var scb = scba.GetBlock(scbkey);
            psb.com.WriteBytes(scb.Data, offset);
        }
    }
}