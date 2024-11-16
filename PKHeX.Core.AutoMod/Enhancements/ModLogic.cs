﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using static PKHeX.Core.Species;

namespace PKHeX.Core.AutoMod;

/// <summary>
/// Miscellaneous enhancement methods
/// </summary>
public static class ModLogic
{
    // Living Dex Settings
    public static LivingDexConfig Config { get; set; } = new()
    {
        IncludeForms = false,
        SetShiny = false,
        SetAlpha = false,
        NativeOnly = false,
        TransferVersion = GameVersion.SL,
    };

    public static bool IncludeForms { get; set; }
    public static bool SetShiny { get; set; }
    public static bool SetAlpha { get; set; }
    public static bool NativeOnly { get; set; }
    public static GameVersion TransferVersion { get; set; }
    /// <summary>
    /// Exports the <see cref="SaveFile.CurrentBox"/> to <see cref="ShowdownSet"/> as a single string.
    /// </summary>
    /// <param name="provider">Save File to export from</param>
    /// <returns>Concatenated string of all sets in the current box.</returns>
    public static string GetRegenSetsFromBoxCurrent(this ISaveFileProvider provider) => GetRegenSetsFromBox(provider.SAV, provider.CurrentBox);

    /// <summary>
    /// Exports the <see cref="box"/> to <see cref="ShowdownSet"/> as a single string.
    /// </summary>
    /// <param name="sav">Save File to export from</param>
    /// <param name="box">Box to export from</param>
    /// <returns>Concatenated string of all sets in the specified box.</returns>
    public static string GetRegenSetsFromBox(this SaveFile sav, int box)
    {
        Span<PKM> data = sav.GetBoxData(box);
        var sep = Environment.NewLine + Environment.NewLine;
        return data.GetRegenSets(sep);
    }

    /// <summary>
    /// Gets a living dex (one per species, not every form)
    /// </summary>
    /// <param name="sav">Save File to receive the generated <see cref="PKM"/>.</param>
    /// <returns>Consumable list of newly generated <see cref="PKM"/> data.</returns>
    public static IEnumerable<PKM> GenerateLivingDex(this SaveFile sav) => sav.GenerateLivingDex(Config);

    /// <summary>
    /// Gets a living dex (one per species, not every form)
    /// </summary>
    /// <param name="sav">Save File to receive the generated <see cref="PKM"/>.</param>
    /// <returns>Consumable list of newly generated <see cref="PKM"/> data.</returns>
    public static IEnumerable<PKM> GenerateLivingDex(this SaveFile sav, LivingDexConfig cfg)
    {
        var pklist = new ConcurrentBag<PKM>();
        var tr = APILegality.UseTrainerData ? TrainerSettings.GetSavedTrainerData(sav.Generation) : sav;
        var pt = sav.Personal;
        var species = Enumerable.Range(1, sav.MaxSpeciesID).Select(x => (ushort)x);
        Parallel.ForEach(species, s =>
        {
            if (!pt.IsSpeciesInGame(s))
                return;

            var num_forms = pt[s].FormCount;
            var str = GameInfo.Strings;
            if (num_forms == 1 && cfg.IncludeForms) // Validate through form lists
                num_forms = (byte)FormConverter.GetFormList(s, str.types, str.forms, GameInfo.GenderSymbolUnicode, sav.Context).Length;
            if (s == (ushort)Alcremie)
                num_forms = (byte)(num_forms * 6);
            uint formarg = 0;
            byte acform = 0;
            for (byte f = 0; f < num_forms; f++)
            {
                var form = cfg.IncludeForms ? f : GetBaseForm((Species)s, f, sav);
                if (s == (ushort)Alcremie)
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

                if (!sav.Personal.IsPresentInGame(s, form) || FormInfo.IsLordForm(s, form, sav.Context) || FormInfo.IsBattleOnlyForm(s, form, sav.Generation) || FormInfo.IsFusedForm(s, form, sav.Generation) || (FormInfo.IsTotemForm(s, form) && sav.Context is not EntityContext.Gen7))
                    continue;
                var pk = AddPKM(sav, tr, s, form, cfg.SetShiny, cfg.SetAlpha, cfg.NativeOnly);
                if (pk is not null && !pklist.Any(x => x.Species == pk.Species && x.Form == pk.Form && x.Species !=869))
                {
                    if (s == (ushort)Alcremie) pk.ChangeFormArgument(formarg);
                    pklist.Add(pk);
                    if (!cfg.IncludeForms)
                        break;
                }
            }
        });
        return pklist.OrderBy(z=>z.Species);
    }
    public static IEnumerable<PKM> GenerateTransferLivingDex(this SaveFile sav) => sav.GenerateTransferLivingDex(Config);
    public static IEnumerable<PKM> GenerateTransferLivingDex(this SaveFile sav, LivingDexConfig cfg)
    {
        var resetevent = new ManualResetEvent(false);
        var DestinationSave = SaveUtil.GetBlankSAV(cfg.TransferVersion, "ALM");
        ConcurrentBag<PKM> pklist = [];
        var tr = APILegality.UseTrainerData ? TrainerSettings.GetSavedTrainerData(sav.Version, sav.Generation, fallback: sav, lang: (LanguageID)sav.Language) : sav;
        var pt = sav.Personal;
        var species = Enumerable.Range(1, sav.MaxSpeciesID).Select(x => (ushort)x);
        Parallel.ForEach(species, s =>
        {
            if (!pt.IsSpeciesInGame(s))
                return;
            if (!DestinationSave.Personal.IsSpeciesInGame(s))
                return;
            var num_forms = pt[s].FormCount;
            var str = GameInfo.Strings;
            if (num_forms == 1 && cfg.IncludeForms) // Validate through form lists
                num_forms = (byte)FormConverter.GetFormList(s, str.types, str.forms, GameInfo.GenderSymbolUnicode, sav.Context).Length;

            for (byte f = 0; f < num_forms; f++)
            {
                if (!DestinationSave.Personal.IsPresentInGame(s, f) || FormInfo.IsLordForm(s, f, sav.Context) || FormInfo.IsBattleOnlyForm(s, f, sav.Generation) || FormInfo.IsFusedForm(s, f, sav.Generation) || (FormInfo.IsTotemForm(s, f) && sav.Context is not EntityContext.Gen7))
                    continue;
                var form = cfg.IncludeForms ? f : GetBaseForm((Species)s, f, sav);
                var pk = AddPKM(sav, tr, s, form, cfg.SetShiny, cfg.SetAlpha, cfg.NativeOnly);
                if (pk is not null && !pklist.Any(x => x.Species == pk.Species && x.Form == pk.Form))
                {
                    pklist.Add(pk);
                    if (!cfg.IncludeForms)
                        break;
                }
            }
        });
        return pklist.OrderBy(z => z.Species);
    }

    private static bool IsRegionalSV(Species s) => s is Tauros or Wooper;
    private static bool IsRegionalLA(Species s) => s is Growlithe or Arcanine or Voltorb or Electrode or Typhlosion or Qwilfish or Sneasel or Samurott or Lilligant or Zorua or Zoroark or Braviary or Sliggoo or Goodra or Avalugg or Decidueye;
    private static bool IsRegionalSH(Species s) => s is Meowth or Slowpoke or Ponyta or Rapidash or Slowbro or Slowking or Farfetchd or Weezing or MrMime or Articuno or Moltres or Zapdos or Corsola or Zigzagoon or Linoone or Darumaka or Darmanitan or Yamask or Stunfisk;
    private static bool IsRegionalSM(Species s) => s is Rattata or Raticate or Raichu or Sandshrew or Sandslash or Vulpix or Ninetales or Diglett or Dugtrio or Meowth or Persian or Geodude or Graveler or Golem or Grimer or Muk or Marowak;

    public static byte GetBaseForm(Species s, byte f, SaveFile sav)
    {
        var HasRegionalForm = sav.Version switch
        {
            GameVersion.VL or GameVersion.SL => IsRegionalSV(s),
            GameVersion.PLA => IsRegionalLA(s),
            GameVersion.SH or GameVersion.SW => IsRegionalSH(s),
            GameVersion.SN or GameVersion.MN or GameVersion.UM or GameVersion.US => IsRegionalSM(s),
            _ => false,
        };
        if (HasRegionalForm)
        {
            if (sav.Version is GameVersion.SW or GameVersion.SH && s is Slowbro or Meowth or Darmanitan)
                return 2;
            return 1;
        }

        return f;
    }
    private static PKM? AddPKM(SaveFile sav, ITrainerInfo tr, ushort species, byte form, bool shiny, bool alpha, bool nativeOnly)
    {
        if (sav.GetRandomEncounter(species, form, shiny, alpha, nativeOnly, out var pk) && pk?.Species > 0)
        {
            pk.Heal();
            return pk;
        }

        // If we didn't get an encounter, we still need to consider a Gen1->Gen2 trade.
        if (sav is not { Generation: 2 })
            return null;

        tr = new SimpleTrainerInfo(GameVersion.YW) { Language = tr.Language, OT = tr.OT, TID16 = tr.TID16 };
        var enc = tr.GetRandomEncounter(species, form, shiny, alpha, nativeOnly, out var pkm);
        if (enc && pkm is PK1 pk1)
            return pk1.ConvertToPK2();
        return null;
    }

    /// <summary>
    /// Gets a legal <see cref="PKM"/> from a random in-game encounter's data.
    /// </summary>
    /// <param name="sav">Save File to receive the generated <see cref="PKM"/>.</param>
    /// <param name="species">Species ID to generate</param>
    /// <param name="form">Form to generate; if left null, picks first encounter</param>
    /// <param name="shiny"></param>
    /// <param name="alpha"></param>
    /// <param name="nativeOnly"></param>
    /// <param name="pk">Result legal pkm</param>
    /// <returns>True if a valid result was generated, false if the result should be ignored.</returns>
    public static bool GetRandomEncounter(this SaveFile sav, ushort species, byte form, bool shiny, bool alpha, bool nativeOnly, out PKM? pk) => ((ITrainerInfo)sav).GetRandomEncounter(species, form, shiny, alpha, nativeOnly, out pk);

    /// <summary>
    /// Gets a legal <see cref="PKM"/> from a random in-game encounter's data.
    /// </summary>
    /// <param name="tr">Trainer Data to use in generating the encounter</param>
    /// <param name="species">Species ID to generate</param>
    /// <param name="form">Form to generate; if left null, picks first encounter</param>
    /// <param name="shiny"></param>
    /// <param name="alpha"></param>
    /// <param name="nativeOnly"></param>
    /// <param name="pk">Result legal pkm</param>
    /// <returns>True if a valid result was generated, false if the result should be ignored.</returns>
    public static bool GetRandomEncounter(this ITrainerInfo tr, ushort species, byte form, bool shiny, bool alpha, bool nativeOnly, out PKM? pk)
    {
        var blank = EntityBlank.GetBlank(tr);
        pk = GetRandomEncounter(blank, tr, species, form, shiny, alpha, nativeOnly);
        if (pk is null)
            return false;

        pk = EntityConverter.ConvertToType(pk, blank.GetType(), out _);
        return pk is not null;
    }

    /// <summary>
    /// Gets a legal <see cref="PKM"/> from a random in-game encounter's data.
    /// </summary>
    /// <param name="blank">Template data that will have its properties modified</param>
    /// <param name="tr">Trainer Data to use in generating the encounter</param>
    /// <param name="species">Species ID to generate</param>
    /// <param name="form">Form to generate; if left null, picks first encounter</param>
    /// <param name="shiny"></param>
    /// <param name="alpha"></param>
    /// <param name="nativeOnly"></param>
    /// <returns>Result legal pkm, null if data should be ignored.</returns>
    private static PKM? GetRandomEncounter(PKM blank, ITrainerInfo tr, ushort species, byte form, bool shiny, bool alpha, bool nativeOnly)
    {
        blank.Species = species;
        blank.Gender = blank.GetSaneGender();
        if (species is ((ushort)Meowstic) or ((ushort)Indeedee))
        {
            blank.Gender = form;
            blank.Form = blank.Gender;
        }
        else
        {
            blank.Form = form;
        }

        var template = EntityBlank.GetBlank(tr.Generation, tr.Version);
        var item = GetFormSpecificItem(tr.Version, blank.Species, blank.Form);
        if (item is not null)
            blank.HeldItem = (int)item;

        if (blank is { Species: (ushort)Keldeo, Form: 1 })
            blank.Move1 = (ushort)Move.SecretSword;

        if (blank.GetIsFormInvalid(tr, blank.Form))
            return null;

        var setText = new ShowdownSet(blank).Text.Split('\r')[0];
        if ((shiny && !SimpleEdits.IsShinyLockedSpeciesForm(blank.Species, blank.Form))||(shiny && tr.Generation!=6 && blank.Species != (ushort)Vivillon && blank.Form !=18))
            setText += Environment.NewLine + "Shiny: Yes";

        if (template is IAlphaReadOnly && alpha && tr.Version == GameVersion.PLA)
            setText += Environment.NewLine + "Alpha: Yes";

        var sset = new ShowdownSet(setText);
        var set = new RegenTemplate(sset) { Nickname = string.Empty };
        template.ApplySetDetails(set);

        var t = template.Clone();
        var almres = tr.TryAPIConvert(set, t, nativeOnly);
        var pk = almres.Created;
        var success = almres.Status;

        if (success == LegalizationResult.Regenerated)
            return pk;

        sset = new ShowdownSet(setText.Split('\r')[0]);
        set = new RegenTemplate(sset) { Nickname = string.Empty };
        template.ApplySetDetails(set);

        t = template.Clone();
        almres = tr.TryAPIConvert(set, t, nativeOnly);
        pk = almres.Created;
        success = almres.Status;
        if (pk.Species is (ushort)Gholdengo)
        {
            pk.SetSuggestedFormArgument();
            pk.SetSuggestedMoves();
            success = LegalizationResult.Regenerated;
        }

        return success == LegalizationResult.Regenerated ? pk : null;
    }

    private static bool GetIsFormInvalid(this PKM pk, ITrainerInfo tr, byte form)
    {
        var generation = tr.Generation;
        var species = pk.Species;
        switch ((Species)species)
        {
            case Floette when form == 5:
                return true;
            case Shaymin or Furfrou or Hoopa when form != 0 && generation <= 6:
                return true;
            case Arceus when generation == 4 && form == 9: // ??? form
                return true;
            case Scatterbug or Spewpa when form == 19:
                return true;
        }
        if (FormInfo.IsBattleOnlyForm(pk.Species, form, generation))
            return true;

        if (form == 0)
            return false;

        if (species == 25 || SimpleEdits.AlolanOriginForms.Contains(species))
        {
            if (generation >= 7 && pk.Generation is < 7 and not 0)
                return true;
        }

        return false;
    }

    private static int? GetFormSpecificItem(GameVersion game, ushort species, byte form)
    {
        if (game == GameVersion.PLA)
            return null;

        var generation = game.GetGeneration();
        return species switch
        {
            (ushort)Arceus => generation != 4 || form < 9 ? SimpleEdits.GetArceusHeldItemFromForm(form) : SimpleEdits.GetArceusHeldItemFromForm(form - 1),
            (ushort)Silvally => SimpleEdits.GetSilvallyHeldItemFromForm(form),
            (ushort)Genesect => SimpleEdits.GetGenesectHeldItemFromForm(form),
            (ushort)Giratina => form == 1 && generation < 9 ? 112 : form == 1 ? 1779 : null, // Griseous Orb
            (ushort)Zacian => form == 1 ? 1103 : null, // Rusted Sword
            (ushort)Zamazenta => form == 1 ? 1104 : null, // Rusted Shield
            _ => null,
        };
    }

    /// <summary>
    /// Legalizes all <see cref="PKM"/> in the specified <see cref="box"/>.
    /// </summary>
    /// <param name="sav">Save File to legalize</param>
    /// <param name="box">Box to legalize</param>
    /// <returns>Count of Pokémon that are now legal.</returns>
    public static int LegalizeBox(this SaveFile sav, int box)
    {
        if ((uint)box >= sav.BoxCount)
            return -1;

        var data = sav.GetBoxData(box);
        var ctr = sav.LegalizeAll(data);
        if (ctr > 0)
            sav.SetBoxData(data, box);

        return ctr;
    }

    /// <summary>
    /// Legalizes all <see cref="PKM"/> in all boxes.
    /// </summary>
    /// <param name="sav">Save File to legalize</param>
    /// <returns>Count of Pokémon that are now legal.</returns>
    public static int LegalizeBoxes(this SaveFile sav)
    {
        if (!sav.HasBox)
            return -1;

        var ctr = 0;
        for (int i = 0; i < sav.BoxCount; i++)
        {
            var result = sav.LegalizeBox(i);
            if (result < 0)
                return result;

            ctr += result;
        }
        return ctr;
    }

    /// <summary>
    /// Legalizes all <see cref="PKM"/> in the provided <see cref="data"/>.
    /// </summary>
    /// <param name="sav">Save File context to legalize with</param>
    /// <param name="data">Data to legalize</param>
    /// <returns>Count of Pokémon that are now legal.</returns>
    public static int LegalizeAll(this SaveFile sav, IList<PKM> data)
    {
        var ctr = 0;
        for (int i = 0; i < data.Count; i++)
        {
            var pk = data[i];
            if (pk.Species <= 0 || new LegalityAnalysis(pk).Valid)
                continue;

            var result = sav.Legalize(pk);
            result.Heal();
            if (!new LegalityAnalysis(result).Valid)
                continue; // failed to legalize

            data[i] = result;
            ctr++;
        }

        return ctr;
    }
    public static PKM[] GetSixRandomMons(this SaveFile sav)
    {
        var RandomTeam = new List<PKM>();
        Span<int> ivs = stackalloc int[6];
        var selectedSpecies = new HashSet<ushort>();
        var rng = new Random();

        while (RandomTeam.Count < 6)
        {
            var spec = (ushort)rng.Next(sav.MaxSpeciesID);

            if (selectedSpecies.Contains(spec))
                continue;

            var rough = EntityBlank.GetBlank(sav);
            rough.Species = spec;
            rough.Gender = rough.GetSaneGender();

            if (!sav.Personal.IsSpeciesInGame(rough.Species))
                continue;

            if (APILegality.RandTypes.Length > 0 && (!APILegality.RandTypes.Contains((MoveType)rough.PersonalInfo.Type1) || !APILegality.RandTypes.Contains((MoveType)rough.PersonalInfo.Type2)))
                continue;

            var formnumb = sav.Personal[rough.Species].FormCount;
            if (formnumb == 1)
                formnumb = (byte)FormConverter.GetFormList(rough.Species, GameInfo.Strings.types, GameInfo.Strings.forms, GameInfo.GenderSymbolUnicode, sav.Context).Length;

            do
            {
                if (formnumb == 0) break;
                rough.Form = (byte)rng.Next(formnumb);
            }
            while (!sav.Personal.IsPresentInGame(rough.Species, rough.Form) || FormInfo.IsLordForm(rough.Species, rough.Form, sav.Context) || FormInfo.IsBattleOnlyForm(rough.Species, rough.Form, sav.Generation) || FormInfo.IsFusedForm(rough.Species, rough.Form, sav.Generation) || (FormInfo.IsTotemForm(rough.Species, rough.Form) && sav.Context is not EntityContext.Gen7));

            if (rough.Species is ((ushort)Meowstic) or ((ushort)Indeedee))
            {
                rough.Gender = rough.Form;
                rough.Form = rough.Gender;
            }

            var item = GetFormSpecificItem(sav.Version, rough.Species, rough.Form);
            if (item is not null)
                rough.HeldItem = (int)item;

            if (rough is { Species: (ushort)Keldeo, Form: 1 })
                rough.Move1 = (ushort)Move.SecretSword;

            if (GetIsFormInvalid(rough, sav, rough.Form))
                continue;

            try
            {
                var goodset = new SmogonSetGenerator(rough);
                if (goodset is { Valid: true, Sets.Count: not 0 })
                {
                    var checknull = sav.GetLegalFromSet(goodset.Sets[rng.Next(goodset.Sets.Count)]);
                    if (checknull.Status != LegalizationResult.Regenerated)
                        continue;
                    checknull.Created.ResetPartyStats();
                    RandomTeam.Add(checknull.Created);
                    selectedSpecies.Add(rough.Species);
                    continue;
                }
            }
            catch (Exception) { Debug.Write("Smogon Issues"); }

            var showstring = new ShowdownSet(rough).Text.Split('\r')[0];
            showstring += "\nLevel: 100\n";
            ivs.Clear();
            EffortValues.SetMax(ivs, rough);
            showstring += $"EVs: {ivs[0]} HP / {ivs[1]} Atk / {ivs[2]} Def / {ivs[3]} SpA / {ivs[4]} SpD / {ivs[5]} Spe\n";
            var m = new ushort[4];
            rough.GetMoveSet(m, true);
            showstring += $"- {GameInfo.MoveDataSource.First(z => z.Value == m[0]).Text}\n- {GameInfo.MoveDataSource.First(z => z.Value == m[1]).Text}\n- {GameInfo.MoveDataSource.First(z => z.Value == m[2]).Text}\n- {GameInfo.MoveDataSource.First(z => z.Value == m[3]).Text}";
            showstring += "\n\n";
            var nullcheck = sav.GetLegalFromSet(new ShowdownSet(showstring));
            if (nullcheck.Status != LegalizationResult.Regenerated)
                continue;
            nullcheck.Created.ResetPartyStats();
            RandomTeam.Add(nullcheck.Created);
            selectedSpecies.Add(rough.Species);
        }

        return [.. RandomTeam];
    }
}