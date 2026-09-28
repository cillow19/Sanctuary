using System;

namespace Sanctuary.WebAPI.Models;

public record class ChatLogEntryModel
{
    public required DateTime Timestamp { get; set; }
    public required string Channel { get; set; }
    public required string Message { get; set; }
}
