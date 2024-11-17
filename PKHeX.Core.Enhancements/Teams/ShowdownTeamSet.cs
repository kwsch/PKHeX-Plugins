using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace PKHeX.Core.Enhancements;

/// <summary>
/// Full party worth of <see cref="ShowdownSet"/> data, and page metadata.
/// </summary>
public record ShowdownTeamSet(string TeamName, IReadOnlyList<ShowdownSet> Team, string Format)
{
    public const string MagicMark = "===";

    public string Summary => $"{Format}: {TeamName}";

    public string Export(string language = GameLanguage.DefaultLanguage)
    {
        var sb = new StringBuilder();
        return Export(sb, language);
    }

    public string Export(StringBuilder sb, string language = GameLanguage.DefaultLanguage)
    {
        sb.AppendLine(GetHeading(Format, TeamName));
        foreach (var set in Team)
            sb.AppendLine(set.LocalizedText(language));
        return sb.ToString();
    }

    public static string GetHeading(ReadOnlySpan<char> format, ReadOnlySpan<char> name)
        => $"{MagicMark} [{format}] {name} {MagicMark}";

    public static bool IsLineShowdownTeam(ReadOnlySpan<char> line)
    {
        var trim = line.Trim(); // Just to be sure.
        // Allow for `===*===`, and not `===` as the inner portion has Team details.
        return trim.Length > MagicMark.Length * 2 && trim.StartsWith(MagicMark) && trim.EndsWith(MagicMark);
    }

    public static List<ShowdownTeamSet> GetTeams(string paste)
    {
        string[] lines = paste.Split('\n');
        var result = new List<ShowdownTeamSet>();
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!IsLineShowdownTeam(line))
                continue;

            if (!TryReadTeamLine(line, out var format, out var name))
                continue; // No name

            // find end
            int end = i + 1;
            while (end < lines.Length)
            {
                if (IsLineShowdownTeam(lines[end]))
                    break;
                end++;
            }

            var teamlines = lines.Skip(i + 1).Take(end - i - 1);
            var sets = ShowdownParsing.GetShowdownSets(teamlines).ToList();
            if (sets.Count == 0)
                continue;

            result.Add(new ShowdownTeamSet(name, sets, format));

            i = end - 1;
        }
        return result;
    }

    private static bool TryReadTeamLine(ReadOnlySpan<char> line,
        [NotNullWhen(true)] out string? format,
        [NotNullWhen(true)] out string? name)
    {
        // ===[Gen 8] OU===
        format = name = null;
        var start = line.IndexOf(MagicMark);
        if (start == -1) return false;
        var end = line.LastIndexOf(MagicMark);
        if (end == -1) return false;

        var inner = line[(start + 3)..end].Trim();
        if (inner.IndexOf(MagicMark) != -1)
            return false; // Shouldn't have.

        return TryGetFormatAndName(inner, out format, out name);
    }

    private static bool TryGetFormatAndName(ReadOnlySpan<char> line,
        [NotNullWhen(true)] out string? format,
        [NotNullWhen(true)] out string? name)
    {
        // [Gen 8] OU
        format = name = null;
        if (!line.StartsWith('['))
            return false;
        var formatEnd = line.IndexOf(']');
        if (formatEnd == -1)
            return false;

        var nameStart = formatEnd + 1;
        if (nameStart >= line.Length)
            return false;
        var end = line.Length - 1;

        format = line[1..formatEnd].Trim().ToString();
        name = line[nameStart..end].Trim().ToString();
        return true;
    }
}