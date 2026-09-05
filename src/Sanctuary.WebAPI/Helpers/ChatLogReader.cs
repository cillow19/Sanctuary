using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using Sanctuary.WebAPI.Models;

namespace Sanctuary.WebAPI.Helpers;

/// <summary>
/// Parses the plain-text chat logs written by Sanctuary.Gateway's "Chat" NLog logger
/// (see Sanctuary.Gateway/NLog.config, target "chatFile"/PacketChatHandler). Chat messages
/// are not currently persisted anywhere queryable, so this reads them back out of the
/// rendered log lines instead.
/// </summary>
public static class ChatLogReader
{
    // Default NLog layout: ${longdate}|${level:uppercase=true}|${logger}|${message}
    // Split with a count so a "|" inside the actual chat message can never truncate the line.
    private static readonly Regex LineRegex = new(
        @"^(?<timestamp>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d+)\|(?<level>[A-Z]+)\|(?<logger>[^|]+)\|(?<rest>.*)$",
        RegexOptions.Compiled);

    private static readonly Regex TellRegex = new(
        @"^From: ""(?<from>.*?)"" \(\d+\), To: ""(?<to>.*?)"" \(\d+\), Msg: ""(?<msg>.*)""$",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex AreaRegex = new(
        @"^Area: \d+, From: ""(?<from>.*?)"" \(\d+\), Msg: ""(?<msg>.*)""$",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex GuildRegex = new(
        @"^GuildGuid: \d+, From: ""(?<from>.*?)"" \(\d+\), Msg: ""(?<msg>.*)""$",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex FromOnlyRegex = new(
        @"^From: ""(?<from>.*?)"" \(\d+\), Msg: ""(?<msg>.*)""$",
        RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>
    /// Yields the "Chat-{yyyy-MM-dd}.log" paths (that actually exist) spanning the given date range.
    /// </summary>
    public static IEnumerable<string> GetLogFilePaths(string directory, DateOnly from, DateOnly to)
    {
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var path = Path.Combine(directory, $"Chat-{date:yyyy-MM-dd}.log");

            if (File.Exists(path))
                yield return path;
        }
    }

    public static IEnumerable<ChatLogEntryModel> ReadFile(string filePath)
    {
        foreach (var rawLine in File.ReadLines(filePath))
        {
            if (TryParseLine(rawLine, out var entry))
                yield return entry;
        }
    }

    private static bool TryParseLine(string line, out ChatLogEntryModel entry)
    {
        entry = null!;

        var lineMatch = LineRegex.Match(line);

        // Only the "Chat" logger's lines are chat messages; other loggers can share the same file layout.
        if (!lineMatch.Success || lineMatch.Groups["logger"].Value != "Chat")
            return false;

        if (!DateTime.TryParse(lineMatch.Groups["timestamp"].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp))
            return false;

        var rest = lineMatch.Groups["rest"].Value;

        // rest looks like "Tell|From: \"A\" (1), To: \"B\" (2), Msg: \"...\"" - split off the channel tag.
        var separatorIndex = rest.IndexOf('|');

        if (separatorIndex < 0)
            return false;

        var channel = rest[..separatorIndex];
        var body = rest[(separatorIndex + 1)..];

        var bodyMatch = channel switch
        {
            "Tell" => TellRegex.Match(body),
            "GuildSay" => GuildRegex.Match(body),
            "WorldTrade" or "WorldLfg" or "WorldArea" or "WorldMembersOnly" => AreaRegex.Match(body),
            _ => FromOnlyRegex.Match(body)
        };

        if (!bodyMatch.Success)
            return false;

        var toGroup = bodyMatch.Groups["to"];

        entry = new ChatLogEntryModel
        {
            Timestamp = timestamp,
            Channel = channel,
            FromName = bodyMatch.Groups["from"].Value,
            ToName = toGroup.Success ? toGroup.Value : null,
            Message = bodyMatch.Groups["msg"].Value
        };

        return true;
    }
}
