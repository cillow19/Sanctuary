using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Sanctuary.WebAPI.Helpers;
using Sanctuary.WebAPI.Models;
using Sanctuary.WebAPI.Options;

namespace Sanctuary.WebAPI.Endpoints;

public static class AdminEndpoints
{
    private const int DefaultChatLogLimit = 200;
    private const int MaxChatLogLimit = 1000;

    private static ILogger _logger = null!;

    public static void MapAdminEndpoints(this WebApplication app)
    {
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

        _logger = loggerFactory.CreateLogger(nameof(AdminEndpoints));

        // app.MapPost("/banUser", LoginHandlerAsync);
        // app.MapPost("/unbanUser", RegisterHandlerAsync);
        // app.MapPost("/muteUser", RegisterHandlerAsync);
        // app.MapPost("/unmuteUser", RegisterHandlerAsync);
        app.MapGet("/getChatLogs", ChatLogsHandlerAsync);
        // app.MapGet("/getNameRequests", RegisterHandlerAsync);
        // app.MapPost("/approveNameRequest", RegisterHandlerAsync);
        // app.MapPost("/declineNameRequest", RegisterHandlerAsync);
        // app.MapPost("/getGuildRequests", RegisterHandlerAsync);
        // app.MapPost("/approveGuildRequest", RegisterHandlerAsync);
        // app.MapPost("/declineGuildRequest", RegisterHandlerAsync);
    }

    private static Task<IResult> ChatLogsHandlerAsync(
        [AsParameters] ChatLogRequestModel request,
        IOptions<WebAPIOptions> webAPIOptions,
        CancellationToken cancellationToken)
    {
        var chatLogsPath = webAPIOptions.Value.ChatLogsPath;

        if (string.IsNullOrWhiteSpace(chatLogsPath) || !Directory.Exists(chatLogsPath))
        {
            _logger.LogError("WebAPI:ChatLogsPath is not configured or does not exist: {Path}", chatLogsPath);

            return Task.FromResult(Results.Problem("Chat logs directory is not configured on the server."));
        }

        var to = request.To ?? DateTime.UtcNow;
        var from = request.From ?? to.Date;

        if (from > to)
            return Task.FromResult(Results.BadRequest("'From' must not be later than 'To'."));

        var limit = Math.Clamp(request.Limit ?? DefaultChatLogLimit, 1, MaxChatLogLimit);

        var entries = new List<ChatLogEntryModel>();

        foreach (var filePath in ChatLogReader.GetLogFilePaths(chatLogsPath, DateOnly.FromDateTime(from), DateOnly.FromDateTime(to)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var entry in ChatLogReader.ReadFile(filePath))
            {
                if (entry.Timestamp < from || entry.Timestamp > to)
                    continue;

                if (!string.IsNullOrWhiteSpace(request.Channel) &&
                    !string.Equals(entry.Channel, request.Channel, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.IsNullOrWhiteSpace(request.CharacterName) && !MatchesCharacter(entry, request.CharacterName))
                    continue;

                entries.Add(entry);
            }
        }

        var result = entries
            .OrderByDescending(x => x.Timestamp)
            .Take(limit)
            .ToList();

        return Task.FromResult(Results.Ok(result));
    }

    private static bool MatchesCharacter(ChatLogEntryModel entry, string characterName)
    {
        return entry.FromName.Contains(characterName, StringComparison.OrdinalIgnoreCase) ||
               (entry.ToName?.Contains(characterName, StringComparison.OrdinalIgnoreCase) ?? false);
    }
}
