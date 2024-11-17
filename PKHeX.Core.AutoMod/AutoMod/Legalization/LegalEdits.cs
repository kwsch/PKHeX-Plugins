namespace PKHeX.Core.AutoMod;

/// <summary>
/// Suggestion edits that rely on a <see cref="LegalityAnalysis"/> being done.
/// </summary>
public static class LegalEdits
{
    public static bool ReplaceBallPrefixLA { get; set; }

    private static Ball GetBallLA(Ball ball) => ball switch
    {
        Ball.Poke => Ball.LAPoke,
        Ball.Great => Ball.LAGreat,
        Ball.Ultra => Ball.LAUltra,
        Ball.Heavy => Ball.LAHeavy,
        _ => ball,
    };

    /// <summary>
    /// Set a valid Pokeball based on a legality check's suggestions.
    /// </summary>
    /// <param name="pk">Pokémon to modify</param>
    /// <param name="enc"></param>
    /// <param name="matching">Set matching ball</param>
    /// <param name="force"></param>
    /// <param name="ball"></param>
    public static void SetSuggestedBall(this PKM pk, IEncounterTemplate enc, bool matching = true, bool force = false, Ball ball = Ball.None)
    {
        var orig = pk.Ball;
        if (ball == Ball.None)
            force = false; // accept anything if no ball is specified

        if (enc is MysteryGift)
            return;

        if (ball != Ball.None)
        {
            if (pk.LA && ReplaceBallPrefixLA)
                ball = GetBallLA(ball);

            pk.Ball = (byte)ball;
        }
        else if (matching)
        {
            if (!pk.IsShiny)
                pk.SetMatchingBall();
            else
                Aesthetics.ApplyShinyBall(pk, enc);

            if (force || new LegalityAnalysis(pk).Valid)
                return;
        }

        if (force || new LegalityAnalysis(pk).Valid)
            return;

        if (pk is { Generation: 5, MetLocation: 75 })
        {
            if (pk.Species == (ushort)Species.Shedinja)
                pk.Ball = (int)Ball.Poke;
            else
                pk.Ball = (int)Ball.Dream;
        }
        else
        {
            pk.Ball = orig;
        }
    }

    /// <summary>
    /// Sets all ribbon flags according to a legality report.
    /// </summary>
    /// <param name="pk">Pokémon to modify</param>
    /// <param name="set"></param>
    /// <param name="enc">Encounter matched to</param>
    /// <param name="allValid">Set all valid ribbons only</param>
    public static void SetSuggestedRibbons(this PKM pk, IBattleTemplate set, IEncounterable enc, bool allValid)
    {
        if (!allValid)
            return;

        RibbonApplicator.SetAllValidRibbons(pk);
        if (pk is PK8 { Species: not (int)Species.Shedinja } pk8 && pk8.GetRandomValidMark(set, enc, out var mark))
            pk8.SetRibbonIndex(mark);
        if (pk is PK9 { Species: not (int)Species.Shedinja } pk9 && pk9.GetRandomValidMark(set, enc, out var mark9))
            pk9.SetRibbonIndex(mark9);
    }
}