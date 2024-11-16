using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using Xunit;
using static PKHeX.Core.GameVersion;

namespace AutoModTests;

public static class LivingDexTests
{
    static LivingDexTests() => TestUtil.InitializePKHeXEnvironment();

    private static readonly GameVersion[] GetGameVersionsToTest =
    [
        SL,
        BD,
        PLA,
        SW,
        US,
        SN,
        OR,
        X,
        B2,
        B,
        Pt,
        E,
        C,
        RD,
    ];

    private static GenerateResult SingleSaveTest(this GameVersion s, LivingDexConfig cfg)
    {
        var trainer = new SimpleTrainerInfo(s) { OT = "ALMUT" };
        var personal = GameData.GetPersonal(s);
        RecentTrainerCache.SetRecentTrainer(trainer);

        var expected = trainer.GetExpectedDexCount(personal, cfg);
        expected.Should().NotBe(0);

        var pkms = trainer.GenerateLivingDex(personal, cfg).ToArray();
        var genned = pkms.Length;
        return new GenerateResult(genned == expected, expected, genned);
    }

    public static IEnumerable<object[]> GetLivingDexTestData()
    {
        var cfgs = new LivingDexConfig[16];
        for (int i = 0; i < 16; i++)
            cfgs[i] = new LivingDexConfig((byte)i);

        foreach (var ver in GetGameVersionsToTest)
        {
            foreach (var cf in cfgs)
                yield return [ver, cf];
        }
    }

    [Theory]
    [MemberData(nameof(GetLivingDexTestData))]
    public static void VerifyDex(GameVersion game, LivingDexConfig cfg)
    {
        APILegality.Timeout = 99999;
        Legalizer.EnableEasterEggs = false;
        APILegality.SetAllLegalRibbons = false;

        var res = game.SingleSaveTest(cfg);
        res.Success.Should().BeTrue($"GameVersion: {game}\n{cfg}\nExpected: {res.Expected}\nGenerated: {res.Generated}");
    }

    private readonly record struct GenerateResult(bool Success, int Expected, int Generated);

    // Ideally should use purely PKHeX's methods or known total counts so that we're not verifying against ourselves.
    private static int GetExpectedDexCount(this ITrainerInfo sav, IPersonalTable personal, LivingDexConfig cfg)
    {
        Dictionary<ushort, List<byte>> speciesDict = [];
        var context = sav.Context;
        var generation = sav.Generation;
        for (ushort s = 1; s <= personal.MaxSpeciesID; s++)
        {
            if (!personal.IsSpeciesInGame(s))
                continue;

            List<byte> forms = [];
            var formCount = personal[s].FormCount;
            var str = GameInfo.Strings;
            if (formCount == 1 && cfg.IncludeForms) // Validate through form lists
                formCount = (byte)FormConverter.GetFormList(s, str.types, str.forms, GameInfo.GenderSymbolUnicode, context).Length;
            if (s == (ushort)Species.Alcremie)
                formCount = (byte)(formCount * 6);
            uint formarg = 0;
            byte acform = 0;
            for (byte f = 0; f < formCount; f++)
            {
                byte form = f;
                if (s == (ushort)Species.Alcremie)
                {
                    form = acform;
                    if (f % 6 == 0)
                    {
                        acform++;
                        formarg = 0;
                    }
                    else
                    {
                        formarg++;
                    }
                }
                if (!personal.IsPresentInGame(s, form) || FormInfo.IsFusedForm(s, form, generation) || FormInfo.IsBattleOnlyForm(s, form, generation) || (FormInfo.IsTotemForm(s, form) && context is not EntityContext.Gen7) || FormInfo.IsLordForm(s, form, context))
                    continue;

                var valid = sav.GetRandomEncounter(s, form, cfg.SetShiny, cfg.SetAlpha, cfg.NativeOnly, out PKM? pk);
                if (pk is null || !valid || pk.Form != form)
                    continue;

                forms.Add(form);
                if (!cfg.IncludeForms)
                    break;
            }

            if (forms.Count > 0)
                speciesDict.TryAdd(s, forms);
        }

        return cfg.IncludeForms ? speciesDict.Values.Sum(x => x.Count) : speciesDict.Count;
    }
}