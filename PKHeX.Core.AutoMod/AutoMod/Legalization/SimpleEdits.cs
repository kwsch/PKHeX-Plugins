using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static PKHeX.Core.Species;

namespace PKHeX.Core.AutoMod;

public static class SimpleEdits
{
    // Make PKHeX use our own marking method
    static SimpleEdits() => MarkingApplicator.MarkingMethod = FlagIVsAutoMod;

    internal static ReadOnlySpan<ushort> RoamingMetLocationBDSP =>
    [
        197, 201,
        354, 355, 356, 357, 358, 359, 361, 362, 364, 365, 367, 373, 375, 377, 378, 379,
        383, 385, 392, 394, 395, 397, 400, 403, 404, 407, 411, 412, 414, 416, 420,
    ];

    internal static ReadOnlySpan<ushort> AlolanOriginForms =>
    [
        019, // Rattata
        020, // Raticate
        027, // Sandshrew
        028, // Sandslash
        037, // Vulpix
        038, // Ninetales
        050, // Diglett
        051, // Dugtrio
        052, // Meowth
        053, // Persian
        074, // Geodude
        075, // Graveler
        076, // Golem
        088, // Grimer
        089, // Muk
    ];

    public static bool IsShinyLockedSpeciesForm(ushort species, byte form) => (Species)species switch
    {
        Pikachu => form is not (0 or 8), // Cap Pikachus, Cosplay
        Pichu => form is 1, // Spiky-eared
        Victini or Keldeo => true,
        Scatterbug or Spewpa or Vivillon => form is 19, // Poké Ball
        Hoopa or Volcanion or Cosmog or Cosmoem => true,

        Magearna => true, // Even though a shiny is available via HOME, can't generate as legal.

        Kubfu or Urshifu or Zarude => true,
        Glastrier or Spectrier or Calyrex => true,
        Enamorus => true,
        Gimmighoul => form is 1,

        WoChien or ChienPao or TingLu or ChiYu => true,
        Koraidon or Miraidon => true,

        WalkingWake or IronLeaves => true,
        Okidogi or Munkidori or Fezandipiti => true,
        Ogerpon => true,
        GougingFire or RagingBolt or IronBoulder or IronCrown => true,
        Terapagos => true,
        Pecharunt => true,

        _ => false,
    };

    private static Func<int, int, int> FlagIVsAutoMod(PKM pk)
    {
        return pk.Format < 7 ? GetSimpleMarking : GetComplexMarking;

        // value, index
        static int GetSimpleMarking(int val, int _) => val == 31 ? 1 : 0;
        static int GetComplexMarking(int val, int _)
        {
            return val switch
            {
                31 => 1,
                1 => 2,
                0 => 2,
                _ => 0,
            };
        }
    }

    /// <summary>
    /// Set Encryption Constant based on PKM Generation
    /// </summary>
    /// <param name="pk">PKM to modify</param>
    /// <param name="enc">Encounter details</param>
    public static void SetEncryptionConstant(this PKM pk, IEncounterTemplate enc)
    {
        if (pk.Format < 6)
            return;

        if (enc is { Species: 658, Form: 1 } || APILegality.IsPIDIVSet(pk, enc)) // Ash-Greninja or raids
            return;

        if (enc.Generation is 3 or 4 or 5)
        {
            var ec = pk.PID;
            pk.EncryptionConstant = ec;
            var pidxor = ((pk.TID16 ^ pk.SID16 ^ (int)(ec & 0xFFFF) ^ (int)(ec >> 16)) & ~0x7) == 8;
            pk.PID = pidxor ? ec ^ 0x80000000 : ec;
            return;
        }
        var wIndex = WurmpleUtil.GetWurmpleEvoGroup(pk.Species);
        if (wIndex != WurmpleEvolution.None)
        {
            pk.EncryptionConstant = WurmpleUtil.GetWurmpleEncryptionConstant(wIndex);
            return;
        }

        if (enc is not ITeraRaid9 && pk is { Species: (ushort)Maushold, Form: 0 } or { Species: (ushort)Dudunsparce, Form: 1 })
        {
            pk.EncryptionConstant = pk.EncryptionConstant / 100 * 100;
            return;
        }

        if (pk.EncryptionConstant != 0)
            return;

        pk.EncryptionConstant = enc is WC8 { PIDType: ShinyType8.FixedValue, EncryptionConstant: 0 } ? 0 : Util.Rand32();
    }

    /// <summary>
    /// Sets shiny value to whatever boolean is specified. Takes in specific shiny as a boolean. Ignores it for stuff that is gen 5 or lower. Cant be asked to find out all legality quirks
    /// </summary>
    /// <param name="pk">PKM to modify</param>
    /// <param name="isShiny">Shiny value that needs to be set</param>
    /// <param name="enc">Encounter details</param>
    /// <param name="shiny">Set is shiny</param>
    public static void SetShinyBoolean(this PKM pk, bool isShiny, IEncounterTemplate enc, Shiny shiny)
    {
        if (IsShinyLockedSpeciesForm(pk.Species, pk.Form))
            return;

        if (pk.IsShiny == isShiny)
            return; // don't mess with stuff if pk is already shiny. Also do not modify for specific shinies (Most likely event shinies)

        if (!isShiny)
        {
            pk.SetUnshiny();
            return;
        }

        if (enc is EncounterStatic8N or EncounterStatic8NC or EncounterStatic8ND or EncounterStatic8U)
        {
            pk.SetRaidShiny(shiny, enc);
            return;
        }

        if (enc is WC8 { IsHOMEGift: true })
        {
            // Set XOR as 0 so SID comes out as 8 or less, Set TID based on that (kinda like a setshinytid)
            pk.TID16 = (ushort)(0 ^ (pk.PID & 0xFFFF) ^ (pk.PID >> 16));
            pk.SID16 = (ushort)Util.Rand.Next(8);
            return;
        }

        if (enc.Generation > 5 || pk.VC)
        {
            if (enc.Shiny is Shiny.FixedValue or Shiny.Never)
                return;

            while (true)
            {
                pk.SetShiny();
                switch (shiny)
                {
                    case Shiny.AlwaysSquare when pk.ShinyXor != 0:
                    case Shiny.AlwaysStar when pk.ShinyXor == 0:
                        continue;
                }
                return;
            }
        }

        if (enc is MysteryGift mg)
        {
            if (mg.IsEgg || mg is PGT { IsManaphyEgg: true })
            {
                pk.SetShinySID(); // not SID locked
                return;
            }

            pk.SetShiny();
            if (pk.Format < 6)
                return;

            do
            {
                pk.SetShiny();
            } while (IsBit3Set());

            bool IsBit3Set() => ((pk.TID16 ^ pk.SID16 ^ (int)(pk.PID & 0xFFFF) ^ (int)(pk.PID >> 16)) & ~0x7) == 8;
            return;
        }

        pk.SetShinySID(); // no mg = no lock
        if (isShiny && enc.Generation is 1 or 2)
            pk.SetShiny();
        if (enc.Generation != 5)
            return;

        while (true)
        {
            pk.PID = EntityPID.GetRandomPID(Util.Rand, pk.Species, pk.Gender, pk.Version, pk.Nature, pk.Form, pk.PID);
            if (shiny == Shiny.AlwaysSquare && pk.ShinyXor != 0)
                continue;

            if (shiny == Shiny.AlwaysStar && pk.ShinyXor == 0)
                continue;

            var isValidGen5SID = pk.SID16 & 1;
            pk.SetShinySID();
            pk.EncryptionConstant = pk.PID;
            var result = (pk.PID & 1) ^ (pk.PID >> 31) ^ (pk.TID16 & 1) ^ (pk.SID16 & 1);
            if ((isValidGen5SID == (pk.SID16 & 1)) && result == 0)
                break;
        }
    }

    public static void SetRaidShiny(this PKM pk, Shiny shiny, IEncounterTemplate enc)
    {
        if (pk.IsShiny)
            return;

        while (true)
        {
            pk.SetShiny();
            if (pk.Format <= 7)
                return;

            var xor = pk.ShinyXor;
            if (enc is EncounterStatic8U && xor != 1 && shiny != Shiny.AlwaysSquare)
                continue;

            if ((shiny == Shiny.AlwaysStar && xor == 1) || (shiny == Shiny.AlwaysSquare && xor == 0) || ((shiny is Shiny.Always or Shiny.Random) && xor < 2)) // allow xor1 and xor0 for den shinies
                return;
        }
    }

    public static void ClearRelearnMoves(this PKM pk)
    {
        pk.RelearnMove1 = 0;
        pk.RelearnMove2 = 0;
        pk.RelearnMove3 = 0;
        pk.RelearnMove4 = 0;
    }

    public static uint GetShinyPID(int tid, int sid, uint pid, int type)
    {
        return (uint)(((tid ^ sid ^ (pid & 0xFFFF) ^ type) << 16) | (pid & 0xFFFF));
    }

    public static void ApplyHeightWeight(this PKM pk, IEncounterTemplate enc, bool signed = true)
    {
        if (enc is { Generation: < 8, Context: not EntityContext.Gen7b } && pk.Format >= 8) // height and weight don't apply prior to GG
            return;

        if (pk is IScaledSizeValue obj) // Deal with this later -- restrictions on starters/statics/alphas, for now roll with whatever encounter DB provides
        {
            obj.HeightAbsolute = obj.CalcHeightAbsolute;
            obj.WeightAbsolute = obj.CalcWeightAbsolute;
            if (pk is PB7 pb1)
                pb1.ResetCP();

            return;
        }
        if (pk is not IScaledSize size)
            return;

        // fixed height and weight
        bool isFixedScale = enc switch
        {
            EncounterStatic9 { Size: not 0 } => true,
            EncounterTrade8b => true,
            EncounterTrade9 => true,
            EncounterStatic8a { HasFixedHeight: true } => true,
            EncounterStatic8a { HasFixedWeight: true } => true,
            _ => false,
        };
        if (isFixedScale)
            return;

        if (enc is WC8 { IsHOMEGift: true })
            return; // HOME gift. No need to set height and weight

        if (enc is WC9 wc9)
        {
            size.WeightScalar = (byte)wc9.WeightValue;
            size.HeightScalar = (byte)wc9.HeightValue;
            return;
        }

        if (enc is EncounterStatic8N or EncounterStatic8NC or EncounterStatic8ND)
            return;
        if (APILegality.IsPIDIVSet(pk, enc) && !(enc is EncounterEgg && GameVersion.BDSP.Contains(enc.Version)))
            return;

        var height = 0x12;
        var weight = 0x97;
        var scale = 0xFB;
        if (signed)
        {
            if (GameVersion.SWSH.Contains(pk.Version) || GameVersion.BDSP.Contains(pk.Version) || GameVersion.SV.Contains(pk.Version))
            {
                var top = (int)(pk.PID >> 16);
                var bottom = (int)(pk.PID & 0xFFFF);
                height = (top % 0x80) + (bottom % 0x81);
                weight = ((int)(pk.EncryptionConstant >> 16) % 0x80) + ((int)(pk.EncryptionConstant & 0xFFFF) % 0x81);
                scale = ((int)(pk.PID >> 16)*height % 0x80) + ((int)(pk.PID &0xFFFF)*height % 0x81);
            }
            else if (pk.GG)
            {
                height = (int)(pk.PID >> 16) % 0xFF;
                weight = (int)(pk.PID & 0xFFFF) % 0xFF;
                scale = (int)(pk.PID >> 8) % 0xFF;
            }
        }
        else
        {
            height = Util.Rand.Next(255);
            weight = Util.Rand.Next(255);
            scale = Util.Rand.Next(255);
        }
        size.HeightScalar = (byte)height;
        size.WeightScalar = (byte)weight;
        if (pk is IScaledSize3 sz3 && enc is not EncounterFixed9 && sz3.Scale != 128)
            sz3.Scale = (byte)scale;
    }

    public static void SetFriendship(this PKM pk, IEncounterTemplate enc)
    {
        if (enc.Generation <= 2)
        {
            pk.OriginalTrainerFriendship = (byte)GetBaseFriendship(EntityContext.Gen7, pk.Species, pk.Form); // VC transfers use SM personal info
            return;
        }

        bool wasNeverOriginalTrainer = !HistoryVerifier.GetCanOTHandle(enc, pk, enc.Generation);
        if (wasNeverOriginalTrainer)
        {
            pk.OriginalTrainerFriendship = (byte)GetBaseFriendship(enc);
            pk.HandlingTrainerFriendship = pk.HasMove(218) ? (byte)0 : (byte)255;
        }
        else
        {
            pk.CurrentFriendship = pk.HasMove(218) ? (byte)0 : (byte)255;
        }
    }

    public static void SetBelugaValues(this PKM pk)
    {
        if (pk is PB7 pb7)
            pb7.ResetCalculatedValues();
    }

    public static void SetAwakenedValues(this PKM pk, IBattleTemplate set)
    {
        if (pk is not PB7 pb7)
            return;

        Span<byte> result = stackalloc byte[6];
        AwakeningUtil.SetExpectedMinimumAVs(result, pb7);

        const byte max = AwakeningUtil.AwakeningMax;
        ReadOnlySpan<int> evs = set.EVs;
        pb7.AV_HP  = (byte)Math.Min(max, Math.Max(result[0], evs[0]));
        pb7.AV_ATK = (byte)Math.Min(max, Math.Max(result[1], evs[1]));
        pb7.AV_DEF = (byte)Math.Min(max, Math.Max(result[2], evs[2]));
        pb7.AV_SPA = (byte)Math.Min(max, Math.Max(result[3], evs[4]));
        pb7.AV_SPD = (byte)Math.Min(max, Math.Max(result[4], evs[5]));
        pb7.AV_SPE = (byte)Math.Min(max, Math.Max(result[5], evs[3]));
    }

    public static void SetHTLanguage(this PKM pk, byte prefer)
    {
        var preferID = (LanguageID)prefer;
        if (preferID is LanguageID.Hacked or LanguageID.UNUSED_6)
            prefer = 2; // prefer english

        if (pk is IHandlerLanguage h)
            h.HandlingTrainerLanguage = prefer;
    }

    public static void SetGigantamaxFactor(this PKM pk, IBattleTemplate set, IEncounterTemplate enc)
    {
        if (pk is not IGigantamax gmax || gmax.CanGigantamax == set.CanGigantamax)
            return;

        if (Gigantamax.CanToggle(pk.Species, pk.Form, enc.Species, enc.Form))
            gmax.CanGigantamax = set.CanGigantamax; // soup hax
    }

    public static void SetGimmicks(this PKM pk, IBattleTemplate set)
    {
        if (pk is IDynamaxLevel d)
            d.DynamaxLevel = d.GetSuggestedDynamaxLevel(pk, requested: set.DynamaxLevel);

        if (pk is ITeraType t && set.TeraType != MoveType.Any && t.GetTeraType() != set.TeraType)
            t.SetTeraType(set.TeraType);
    }

    internal static void HyperTrain(this IHyperTrain t, PKM pk, ReadOnlySpan<int> ivs)
    {
        t.HT_HP  = pk.IV_HP  != 31;
        t.HT_ATK = pk.IV_ATK != 31 && ivs[1] > 2;
        t.HT_DEF = pk.IV_DEF != 31;
        t.HT_SPA = pk.IV_SPA != 31 && ivs[4] > 2;
        t.HT_SPD = pk.IV_SPD != 31;
        t.HT_SPE = pk.IV_SPE != 31 && ivs[3] > 2;

        if (pk is PB7 pb)
            pb.ResetCP();
    }

    public static void SetSuggestedMemories(this PKM pk)
    {
        switch (pk)
        {
            case PK9 pk9 when !pk.IsUntraded:
                pk9.ClearMemoriesHT();
                break;
            case PA8 pa8 when !pk.IsUntraded:
                pa8.ClearMemoriesHT();
                break;
            case PB8 pb8 when !pk.IsUntraded:
                pb8.ClearMemoriesHT();
                break;
            case PK8 pk8 when !pk.IsUntraded:
                pk8.SetTradeMemoryHT8();
                break;
            case PK7 pk7 when !pk.IsUntraded:
                pk7.SetTradeMemoryHT6(true);
                break;
            case PK6 pk6 when !pk.IsUntraded:
                pk6.SetTradeMemoryHT6(true);
                break;
        }
    }

    private static int GetBaseFriendship(IEncounterTemplate enc) => enc switch
    {
        IFixedOTFriendship f => f.OriginalTrainerFriendship,
        { Version: GameVersion.BD or GameVersion.SP } => PersonalTable.SWSH.GetFormEntry(enc.Species, enc.Form).BaseFriendship,
        _ => GetBaseFriendship(enc.Context, enc.Species, enc.Form),
    };

    private static int GetBaseFriendship(EntityContext context, ushort species, byte form) => context switch
    {
        EntityContext.Gen1  => PersonalTable.USUM[species].BaseFriendship,
        EntityContext.Gen2  => PersonalTable.USUM[species].BaseFriendship,
        EntityContext.Gen6  => PersonalTable.AO  [species].BaseFriendship,
        EntityContext.Gen7  => PersonalTable.USUM[species].BaseFriendship,
        EntityContext.Gen7b => PersonalTable.GG  [species].BaseFriendship,
        EntityContext.Gen8  => PersonalTable.SWSH[species, form].BaseFriendship,
        EntityContext.Gen8a => PersonalTable.LA  [species, form].BaseFriendship,
        EntityContext.Gen8b => PersonalTable.BDSP[species, form].BaseFriendship,
        EntityContext.Gen9  => PersonalTable.SV  [species, form].BaseFriendship,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>
    /// Set TID, SID and OT
    /// </summary>
    /// <param name="pk">PKM to set trainer data to</param>
    /// <param name="trainer">Trainer data</param>
    public static void SetTrainerData(this PKM pk, ITrainerInfo trainer)
    {
        pk.TID16 = trainer.TID16;
        pk.SID16 = pk.Generation >= 3 ? trainer.SID16 : (ushort)0;
        pk.OriginalTrainerName = trainer.OT;
    }

    /// <summary>
    /// Set Handling Trainer data for a given PKM
    /// </summary>
    /// <param name="pk">PKM to modify</param>
    /// <param name="trainer">Trainer to handle the <see cref="pk"/></param>
    /// <param name="enc">Encounter template originated from</param>
    public static void SetHandlerAndMemory(this PKM pk, ITrainerInfo trainer, IEncounterTemplate enc)
    {
        if (IsUntradeableEncounter(enc))
            return;

        var expect = trainer.IsFromTrainer(pk) ? 0 : 1;
        if (pk.CurrentHandler == expect && expect == 0)
            return;

        pk.CurrentHandler = 1;
        pk.HandlingTrainerName = trainer.OT;
        pk.HandlingTrainerGender = trainer.Gender;
        pk.SetHTLanguage((byte)trainer.Language);
        pk.SetSuggestedMemories();
    }

    /// <summary>
    /// Set trainer data for a legal PKM
    /// </summary>
    /// <param name="pk">Legal PKM for setting the data</param>
    /// <param name="trainer"></param>
    /// <returns>PKM with the necessary values modified to reflect trainer data changes</returns>
    public static void SetAllTrainerData(this PKM pk, ITrainerInfo trainer)
    {
        pk.SetBelugaValues(); // trainer details changed?

        if (pk is not IGeoTrack gt)
            return;

        if (trainer is not IRegionOrigin o)
        {
            gt.ConsoleRegion = 1; // North America
            gt.Country = 49; // USA
            gt.Region = 7; // California
            return;
        }

        gt.ConsoleRegion = o.ConsoleRegion;
        gt.Country = o.Country;
        gt.Region = o.Region;
        if (pk is PK7 pk7 && pk.Generation <= 2)
            pk7.FixVCRegion();
        else if (pk.Species is (int)Vivillon or (int)Spewpa or (int)Scatterbug)
            pk.FixVivillonRegion();
    }

    /// <summary>
    /// Sets a moveset which is suggested based on calculated legality.
    /// </summary>
    /// <param name="pk">Legal PKM for setting the data</param>
    /// <param name="random">True for Random assortment of legal moves, false if current moves only.</param>
    public static void SetSuggestedMoves(this PKM pk, bool random = false)
    {
        Span<ushort> m = stackalloc ushort[4];
        pk.GetMoveSet(m, random);
        var moves = m.ToArray();
        if (moves.All(z => z == 0))
            return;

        if (pk.Moves.SequenceEqual(moves))
            return;

        pk.SetMoves(moves);
    }

    /// <summary>
    /// Set Dates for date-locked Pokémon
    /// </summary>
    /// <param name="pk">Pokémon file to modify</param>
    /// <param name="enc">encounter used to generate Pokémon file</param>
    public static void SetDateLocks(this PKM pk, IEncounterTemplate enc)
    {
        if (enc is WC8 { IsHOMEGift: true } wc8)
            SetDateLocksWC8(pk, wc8);
    }

    private static void SetDateLocksWC8(PKM pk, WC8 w)
    {
        var locked = EncounterServerDate.WC8Gifts.TryGetValue(w.CardID, out var time);
        if (locked)
            pk.MetDate = time.Start;
    }

    public static bool TryApplyHardcodedSeedWild8(PK8 pk, IEncounterTemplate enc, ReadOnlySpan<int> ivs, Shiny requestedShiny)
    {
        // Don't bother if there is no overworld correlation
        if (enc is not IOverworldCorrelation8 eo)
            return false;

        // Check if a seed exists
        var flawless = Overworld8Search.GetFlawlessIVCount(enc, ivs, out var seed);

        // Ensure requested criteria matches
        if (flawless == -1)
            return false;

        APILegality.FindWildPIDIV8(pk, requestedShiny, flawless, seed);
        return eo.IsOverworldCorrelationCorrect(pk) && requestedShiny switch
        {
            Shiny.AlwaysStar when pk.ShinyXor is 0 or > 15 => false,
            Shiny.Never when pk.ShinyXor < 16 => false,
            _ => true,
        };
    }

    public static bool ExistsInGame(this GameVersion destVer, ushort species, byte form)
    {
        // Don't process if Game is LGPE and requested PKM is not Kanto / Meltan / Melmetal
        // Don't process if Game is SWSH and requested PKM is not from the Galar Dex (Zukan8.DexLookup)
        if (GameVersion.GG.Contains(destVer))
            return species is <= 151 or 808 or 809;

        if (GameVersion.SWSH.Contains(destVer))
            return PersonalTable.SWSH.IsPresentInGame(species, form);

        if (GameVersion.PLA.Contains(destVer))
            return PersonalTable.LA.IsPresentInGame(species, form);

        return GameVersion.SV.Contains(destVer) ? PersonalTable.SV.IsPresentInGame(species, form) : (uint)species <= destVer.GetMaxSpeciesID();
    }

    public static GameVersion GetIsland(this GameVersion ver) => ver switch
    {
        GameVersion.BD or GameVersion.SP => GameVersion.BDSP,
        GameVersion.SW or GameVersion.SH => GameVersion.SWSH,
        GameVersion.GP or GameVersion.GE => GameVersion.GG,
        GameVersion.SN or GameVersion.MN => GameVersion.SM,
        GameVersion.US or GameVersion.UM => GameVersion.USUM,
        GameVersion.X or GameVersion.Y => GameVersion.XY,
        GameVersion.OR or GameVersion.AS => GameVersion.ORAS,
        GameVersion.B or GameVersion.W => GameVersion.BW,
        GameVersion.B2 or GameVersion.W2 => GameVersion.B2W2,
        GameVersion.HG or GameVersion.SS => GameVersion.HGSS,
        GameVersion.FR or GameVersion.LG => GameVersion.FRLG,
        GameVersion.D or GameVersion.P or GameVersion.Pt => GameVersion.DPPt,
        GameVersion.R or GameVersion.S or GameVersion.E => GameVersion.RSE,
        GameVersion.GD or GameVersion.SI or GameVersion.C => GameVersion.GSC,
        GameVersion.RD or GameVersion.BU or GameVersion.YW or GameVersion.GN => GameVersion.Gen1,
        _ => ver,
    };

    public static void ApplyPostBatchFixes(this PKM pk)
    {
        if (pk is IScaledSizeValue sv)
        {
            sv.ResetHeight();
            sv.ResetWeight();
        }
    }

    public static bool IsUntradeableEncounter(IEncounterTemplate enc) => enc switch
    {
        EncounterStatic7b { Location: 28 } => true, // LGP/E Starter
        _ => false,
    };

    public static void SetRecordFlags(this PKM pk, ReadOnlySpan<ushort> moves)
    {
        if (pk is ITechRecord tr and not PA8)
        {
            if (pk.Species == (ushort)Hydrapple)
            {
                ReadOnlySpan<ushort> dc = [(ushort)Move.DragonCheer];
                tr.SetRecordFlags(dc);
            }
            if (moves.Length != 0)
            {
                tr.SetRecordFlags(moves);
            }
            else
            {
                var permit = tr.Permit;
                for (int i = 0; i < permit.RecordCountUsed; i++)
                {
                    if (permit.IsRecordPermitted(i))
                        tr.SetMoveRecordFlag(i);
                }
            }
            return;
        }

        if (pk is IMoveShop8Mastery master)
            master.SetMoveShopFlags(pk);
    }

    public static void SetSuggestedContestStats(this PKM pk, IEncounterTemplate enc)
    {
        var la = new LegalityAnalysis(pk);
        pk.SetSuggestedContestStats(enc, la.Info.EvoChainsAllGens);
    }

    private static ReadOnlySpan<ushort> ArceusPlateIDs =>
    [
        303, 306, 304, 305, 309, 308, 310, 313, 298, 299, 301, 300, 307, 302, 311, 312, 644,
    ];

    public static ushort? GetArceusHeldItemFromForm(int form) => form is >= 1 and <= 17 ? ArceusPlateIDs[form - 1] : null;

    public static int? GetSilvallyHeldItemFromForm(int form) => form == 0 ? null : form + 903;

    public static int? GetGenesectHeldItemFromForm(int form) => form == 0 ? null : form + 115;
}