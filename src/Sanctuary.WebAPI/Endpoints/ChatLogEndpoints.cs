using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Sanctuary.WebAPI.Models;
using Sanctuary.WebAPI.Options;

namespace Sanctuary.WebAPI.Endpoints;

public static class ChatLogEndpoints
{
    private static ILogger _logger = null!;

    public static void MapChatLogEndpoints(this WebApplication app)
    {
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

        _logger = loggerFactory.CreateLogger(nameof(ChatLogEndpoints));

        app.MapGet("/chatlogs", GetChatLogsHandlerAsync);
    }

    private static async Task<IResult> GetChatLogsHandlerAsync(
        IOptionsSnapshot<WebAPIOptions> webAPIOptions,
        CancellationToken cancellationToken,
        DateOnly? date,
        string? channel,
        string? player)
    {
        var logDate = date ?? DateOnly.FromDateTime(DateTime.Now);
        var logDirectory = Path.Combine(AppContext.BaseDirectory, webAPIOptions.Value.ChatLogDirectory ?? "Logs");
        var filePath = Path.Combine(logDirectory, $"Chat-{logDate:yyyy-MM-dd}.log");

        if (!File.Exists(filePath))
        {
            _logger.LogDebug("Chat log file not found: {FilePath}", filePath);

            return Results.Ok(Array.Empty<ChatLogEntryModel>());
        }

        var entries = new List<ChatLogEntryModel>();

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);

        string? line;

        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            var parts = line.Split('|', 4);

            if (parts.Length < 4)
                continue;

            if (!DateTime.TryParseExact(parts[0], "yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp))
                continue;

            var message = parts[3];
            var separatorIndex = message.IndexOf('|');
            var entryChannel = separatorIndex < 0 ? string.Empty : message[..separatorIndex].Trim();

            if (channel is not null && !entryChannel.Contains(channel, StringComparison.OrdinalIgnoreCase))
                continue;

            if (player is not null && !message.Contains($"\"{player}\"", StringComparison.OrdinalIgnoreCase))
                continue;

            entries.Add(new ChatLogEntryModel
            {
                Timestamp = timestamp,
                Channel = entryChannel,
                Message = message
            });
        }

        return Results.Ok(entries);
    }
}
