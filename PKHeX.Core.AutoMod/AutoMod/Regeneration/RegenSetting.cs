using System;
using System.Collections.Generic;

namespace PKHeX.Core.AutoMod;

public sealed class RegenSetting
{
    public Ball Ball { get; set; }
    public Shiny ShinyType { get; set; } = Shiny.Never;
    public LanguageID? Language { get; set; }
    public AbilityRequest Ability { get; set; } = AbilityRequest.Any;
    public bool Alpha { get; set; }

    public bool IsShiny => ShinyType != Shiny.Never;

    public bool SetRegenSettings(IEnumerable<string> lines)
    {
        var split = RegenUtil.Split(lines);
        bool any = false;
        foreach (var (key, value) in split)
            any |= IngestSetting(key, value);
        return any;
    }

    private bool IngestSetting(ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        switch (key)
        {
            case nameof(Ball):
                var ball = Aesthetics.GetBallFromString(value);
                if (ball == Ball.Strange)
                    return false;
                Ball = ball;
                return true;
            case nameof(Shiny):
                ShinyType = Aesthetics.GetShinyType(value);
                return true;
            case nameof(Language):
                Language = Aesthetics.GetLanguageId(value);
                return true;
            case nameof(Ability):
                Ability = Enum.TryParse(value, out AbilityRequest ar) ? ar : AbilityRequest.Any;
                return true;
            case nameof(Alpha):
                Alpha = value is "Yes";
                return true;
            default:
                return false;
        }
    }

    public string GetSummary()
    {
        var result = new List<string>();
        if (Ball != Ball.None)
            result.Add($"Ball: {Ball} Ball");

        if (ShinyType == Shiny.AlwaysStar)
            result.Add("Shiny: Star");
        else if (ShinyType == Shiny.AlwaysSquare)
            result.Add("Shiny: Square");
        else if (ShinyType == Shiny.Always)
            result.Add("Shiny: Yes");

        if (Language != null)
            result.Add($"Language: {Language}");

        if (Ability != AbilityRequest.Any)
            result.Add($"Ability: {Ability}");

        if (Alpha)
            result.Add("Alpha: Yes");

        return string.Join(Environment.NewLine, result);
    }
}