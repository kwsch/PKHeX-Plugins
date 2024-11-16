using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using Xunit;
using static PKHeX.Core.GameVersion;

namespace AutoModTests;

public static class TransferDexTests
{
    static TransferDexTests() => TestUtil.InitializePKHeXEnvironment();

    private static readonly GameVersion[] GetGameVersionsToTest =
    [
        RD,
        C,
        E,
        Pt,
        B,
        B2,
        X,
        OR,
        SN,
        US,
        SW,
        PLA,
        BD,
        SL,
    ];

    private static GenerateResult SingleSaveTest(this GameVersion s, LivingDexConfig cfg)
    {
        var sav = SaveUtil.GetBlankSAV(s, "ALMUT");
        RecentTrainerCache.SetRecentTrainer(sav);

        var expected = sav.GetExpectedDexCount(cfg);
        expected.Should().NotBe(0);

        var pkms = sav.GenerateTransferLivingDex(cfg).ToArray();
        var genned = pkms.Length;
        var val = new GenerateResult(genned == expected, expected, genned);
        return val;
    }

    public static IEnumerable<object[]> GetLivingDexTestData()
    {
        var cfgs = new LivingDexConfig[16];
        for (int i = 0; i < 16; i++)
            cfgs[i] = new LivingDexConfig((byte)i);
        foreach (var ver in GetGameVersionsToTest)
        {
            for (int i = Array.IndexOf(GetGameVersionsToTest, ver)+1; i < GetGameVersionsToTest.Length; i++)
            {
                foreach (var cf in cfgs)
                    yield return [ver, cf, GetGameVersionsToTest[i]];
            }
        }
    }

    [Theory]
    [MemberData(nameof(GetLivingDexTestData))]
    public static void VerifyDex(GameVersion game, LivingDexConfig cfg, GameVersion dest)
    {
        APILegality.Timeout = 99999;
        Legalizer.EnableEasterEggs = false;
        APILegality.SetAllLegalRibbons = false;
        APILegality.EnableDevMode = true;
        cfg = cfg with { TransferVersion = dest };
        var res = game.SingleSaveTest(cfg);
        res.Success.Should().BeTrue($"GameVersion: {game}\n{cfg}\nExpected: {res.Expected}\nGenerated: {res.Generated}");
    }

    private readonly record struct GenerateResult(bool Success, int Expected, int Generated);

    // Ideally should use purely PKHeX's methods or known total counts so that we're not verifying against ourselves.
    private static int GetExpectedDexCount(this SaveFile sav, LivingDexConfig cfg)
    {
        Dictionary<ushort, List<byte>> speciesDict = [];
        var personal = sav.Personal;
        var destpersonal = SaveUtil.GetBlankSAV(cfg.TransferVersion, "ALM");
        var species = Enumerable.Range(1, sav.MaxSpeciesID).Select(x => (ushort)x);
        foreach (ushort s in species)
        {
            if (!personal.IsSpeciesInGame(s))
                continue;

            List<byte> forms = [];
            var formCount = personal[s].FormCount;
            var str = GameInfo.Strings;
            if (formCount == 1 && cfg.IncludeForms) // Validate through form lists
                formCount = (byte)FormConverter.GetFormList(s, str.types, str.forms, GameInfo.GenderSymbolUnicode, sav.Context).Length;

            for (byte f = 0; f < formCount; f++)
            {
                if (!destpersonal.Personal.IsPresentInGame(s, f) || FormInfo.IsFusedForm(s, f, sav.Generation) || FormInfo.IsBattleOnlyForm(s, f, sav.Generation) || (FormInfo.IsTotemForm(s, f) && sav.Context is not EntityContext.Gen7) || FormInfo.IsLordForm(s, f, sav.Context))
                    continue;

                var valid = sav.GetRandomEncounter(s, f, cfg.SetShiny, cfg.SetAlpha, cfg.NativeOnly, out PKM? pk);
                if (pk is not null && valid && pk.Form == f && !forms.Contains(f))
                {
                    forms.Add(f);
                    if (!cfg.IncludeForms)
                        break;
                }
            }

            if (forms.Count > 0)
                speciesDict.TryAdd(s, forms);
        }

        return cfg.IncludeForms ? speciesDict.Values.Sum(x => x.Count) : speciesDict.Count;
    }
}