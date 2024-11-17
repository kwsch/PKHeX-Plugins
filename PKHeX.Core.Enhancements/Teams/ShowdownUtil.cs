using System;
using System.Collections.Generic;
using System.Linq;

namespace PKHeX.Core.Enhancements;

/// <summary>
/// Logic for handling <see cref="ShowdownSet"/> data.
/// </summary>
public static class ShowdownUtil
{
    /// <summary>
    /// Checks whether a paste is a showdown team backup
    /// </summary>
    /// <param name="paste">paste to check</param>
    /// <returns>Returns bool</returns>
    public static bool IsTeamBackup(ReadOnlySpan<char> paste) => paste.StartsWith(ShowdownTeamSet.MagicMark);

    /// <summary>
    /// A method to get a list of ShowdownSet(s) from a string paste
    /// Needs to be extended to hold several teams
    /// </summary>
    /// <param name="paste"></param>
    public static List<ShowdownSet> ShowdownSets(string paste)
    {
        paste = paste.Trim(); // Remove White Spaces
        if (IsTeamBackup(paste))
            return ShowdownTeamSet.GetTeams(paste).SelectMany(z => z.Team).ToList();
        return ShowdownParsing.GetShowdownSets(paste).ToList();
    }

    /// <summary>
    /// Checks the input text is a showdown set or not
    /// </summary>
    /// <param name="source">Concatenated showdown strings</param>
    /// <returns>boolean of the summary</returns>
    public static bool IsTextShowdownData(ReadOnlySpan<char> source)
    {
        source = source.Trim();
        if (IsTeamBackup(source))
            return true;

        int first = source.IndexOf('\n');
        if (first < 0)
            return false;
        var slice = source[..first].Trim();
        return new ShowdownSet(slice).Species != 0;
    }
}