using System;
using System.Collections.Generic;
using System.Linq;
using static PKHeX.Core.Injection.LiveHeXVersion;

namespace PKHeX.Core.Injection;

public abstract class InjectionBase(LiveHeXVersion lv, bool useCache) : PointerCache(lv, useCache)
{
    public const decimal BotbaseVersion = 2.3m;

    private const ulong Ovlloader_ID = 0x420000000007e51a;
    private const ulong Dmnt_ID = 0x010000000000000d;

    private const string LetsGoPikachu_ID = "010003F003A34000";
    private const string LetsGoEevee_ID = "0100187003A36000";

    private const string Sword_ID = "0100ABF008968000";
    private const string Shield_ID = "01008DB008C2C000";

    private const string ShiningPearl_ID = "010018E011D92000";
    private const string BrilliantDiamond_ID = "0100000011D90000";

    private const string LegendsArceus_ID = "01001F5010DFA000";

    private const string Scarlet_ID = "0100A3D008C5C000";
    private const string Violet_ID = "01008F6008C5E000";

    private static readonly Dictionary<string, LiveHeXVersion[]> SupportedTitleVersions = new()
    {
        { LetsGoPikachu_ID, [LGPE_v102] },
        { LetsGoEevee_ID, [LGPE_v102] },
        { Sword_ID,  [SWSH_v111, SWSH_v121, SWSH_v132] },
        { Shield_ID, [SWSH_v111, SWSH_v121, SWSH_v132] },
        { ShiningPearl_ID,     [SP_v100, SP_v110, BDSP_v112, BDSP_v113, BDSP_v120, SP_v130] },
        { BrilliantDiamond_ID, [BD_v100, BD_v110, BDSP_v112, BDSP_v113, BDSP_v120, BD_v130] },
        { LegendsArceus_ID, [LA_v100, LA_v101, LA_v102, LA_v111] },
        { Scarlet_ID, [SV_v101, SV_v110, SV_v120, SV_v130, SV_v131, SV_v132, SV_v201, SV_v202, SV_v300, SV_v301] },
        { Violet_ID,  [SV_v101, SV_v110, SV_v120, SV_v130, SV_v131, SV_v132, SV_v201, SV_v202, SV_v300, SV_v301] },
    };

    public virtual Dictionary<string, string> SpecialBlocks { get; } = [];

    protected static InjectionBase GetInjector(LiveHeXVersion version, bool useCache)
    {
        if (LPLGPE.SupportedVersions.Contains(version))
            return new LPLGPE(version, useCache);

        if (LPBDSP.SupportedVersions.Contains(version))
            return new LPBDSP(version, useCache);

        if (LPPointer.SupportedVersions.Contains(version))
            return new LPPointer(version, useCache);

        if (!LPBasic.SupportedVersions.Contains(version))
            throw new ArgumentOutOfRangeException(nameof(version), version, $"Unknown {nameof(LiveHeXVersion)}.");

        return new LPBasic(version, useCache);
    }

    public virtual byte[] ReadBox(PokeSysBotMini psb, int box, int len, List<byte[]> allpkm) => [];

    public virtual byte[] ReadSlot(PokeSysBotMini psb, int box, int slot) => [];

    public virtual void SendBox(PokeSysBotMini psb, ReadOnlySpan<byte> boxData, int box) { }

    public virtual void SendSlot(PokeSysBotMini psb, ReadOnlySpan<byte> data, int box, int slot) { }

    public virtual void WriteBlocksFromSAV(PokeSysBotMini psb, string block, SaveFile sav) { }

    public virtual void WriteBlockFromString(PokeSysBotMini psb, string block, ReadOnlySpan<byte> data, object sb) { }

    public virtual bool ReadBlockFromString(PokeSysBotMini psb, SaveFile sav, string block, out List<byte[]>? read)
    {
        read = null;
        return false;
    }

    public static bool SaveCompatibleWithTitle(SaveFile sav, string titleID) => sav switch
    {
        SAV9SV when titleID is Scarlet_ID or Violet_ID => true,
        SAV8LA when titleID is LegendsArceus_ID => true,
        SAV8BS when titleID is BrilliantDiamond_ID or ShiningPearl_ID => true,
        SAV8SWSH when titleID is Sword_ID or Shield_ID => true,
        SAV7b when titleID is LetsGoPikachu_ID or LetsGoEevee_ID => true,
        _ => false,
    };

    public static LiveHeXVersion GetVersionFromTitle(string titleID, string gameVersion)
    {
        if (!SupportedTitleVersions.TryGetValue(titleID, out var versions))
            return Unknown;

        versions = versions.Reverse().ToArray();
        var sanitized = gameVersion.Replace(".", "");
        foreach (var version in versions)
        {
            var name = Enum.GetName(version);
            if (name is null)
                continue;

            name = name.Split('v')[1];
            if (name == sanitized)
                return version;
        }
        return Unknown;
    }

    public static bool CheckRAMShift(PokeSysBotMini psb, out string msg)
    {
        msg = "";
        if (psb.com is not ICommunicatorNX nx)
            return false;

        if (nx.IsProgramRunning(Ovlloader_ID))
            msg += "Tesla overlay";

        if (nx.IsProgramRunning(Dmnt_ID))
            msg += msg != "" ? " and dmnt (cheats?)" : "Dmnt (cheats?)";

        bool detected = msg != "";
        msg += detected ? " detected.\n\nPlease remove or close the interfering applications and reboot your Switch." : "";
        return detected;
    }
}