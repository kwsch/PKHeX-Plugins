using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace PKHeX.Core.AutoMod;

public sealed class RegenSet
{
    public static readonly RegenSet Default = new([], PKX.Generation);

    public RegenSetting Extra { get; }
    public ITrainerInfo? Trainer { get; }
    public StringInstructionSet Batch { get; }
    public IReadOnlyList<StringInstruction> EncounterFilters { get; }
    public IReadOnlyList<StringInstruction> VersionFilters { get; }

    public readonly bool HasExtraSettings;
    public readonly bool HasTrainerSettings;
    public bool HasBatchSettings => Batch.Filters.Count != 0 || Batch.Instructions.Count != 0;

    public RegenSet(PKM pk) : this([], pk.Format)
    {
        Extra.Ball = (Ball)pk.Ball;
        Extra.ShinyType = pk.ShinyXor == 0 ? Shiny.AlwaysSquare : pk.IsShiny ? Shiny.AlwaysStar : Shiny.Never;
        if (pk is IAlphaReadOnly { IsAlpha: true })
            Extra.Alpha = true;
        HasExtraSettings = true;
        var tr = new SimpleTrainerInfo(pk.Version) { OT = pk.OriginalTrainerName, TID16 = pk.TID16, SID16 = pk.SID16, Gender = pk.OriginalTrainerGender };
        Trainer = tr;
        HasTrainerSettings = true;
        VersionFilters = RegenUtil.GetVersionFilters([$"~=Version={pk.Version}"]);

        List<string> modified = [];
        var ribbons = RibbonInfo.GetRibbonInfo(pk);
        foreach (var rib in ribbons)
        {
            if (rib.HasRibbon)
                modified.Add($".{rib.Name}=true");
        }

        modified.Add($".MetLocation={pk.MetLocation}");
        modified.Add($".MetDate={pk.MetDate}");
        modified.Add($".MetLevel={pk.MetLevel}");
        if (pk is IFormArgument { FormArgument: not 0 } fa)
            modified.Add($".FormArgument={fa.FormArgument}");
        if (modified.Count > 0)
            Batch = new StringInstructionSet(modified.ToArray().AsSpan());
    }

    public RegenSet(ICollection<string> lines, byte format, Shiny shiny = Shiny.Never)
    {
        var modified = lines.Select(z => z.Replace(">=", "≥").Replace("<=", "≤")).ToArray();

        Extra = new RegenSetting { ShinyType = shiny };
        HasExtraSettings = Extra.SetRegenSettings(modified);
        HasTrainerSettings = RegenUtil.GetTrainerInfo(modified, format, out var tr);
        Trainer = tr;
        Batch = new StringInstructionSet(modified);
        EncounterFilters = RegenUtil.GetEncounterFilters(modified);
        VersionFilters = RegenUtil.GetVersionFilters(modified);
    }

    public string GetSummary()
    {
        var sb = new StringBuilder();
        if (HasExtraSettings)
            sb.AppendLine(RegenUtil.GetSummary(Extra));

        if (HasTrainerSettings && Trainer != null)
            sb.AppendLine(RegenUtil.GetSummary(Trainer));

        if (HasBatchSettings)
            sb.AppendLine(RegenUtil.GetSummary(Batch));

        if (EncounterFilters.Any())
            sb.AppendLine(RegenUtil.GetSummary(EncounterFilters));

        if (VersionFilters.Count > 0)
            sb.AppendLine(RegenUtil.GetSummary(VersionFilters));

        return sb.ToString();
    }

    public bool AnyInstructionStartsWith(string name, string value)
    {
        foreach (var z in Batch.Instructions)
        {
            if (!z.PropertyName.StartsWith(name))
                continue;
            if (!z.PropertyValue.StartsWith(value))
                continue;
            return true;
        }
        return false;
    }

    public bool TryGetBatchValue(ReadOnlySpan<char> key, [NotNullWhen(true)] out string? value)
    {
        foreach (var instruction in Batch.Instructions)
        {
            if (!key.SequenceEqual(instruction.PropertyName))
                continue;

            value = instruction.PropertyValue;
            return true;
        }
        value = null;
        return false;
    }
}