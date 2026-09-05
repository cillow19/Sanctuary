using System;

namespace Sanctuary.WebAPI.Models;

public class ChatLogEntryModel
{
    public required DateTime Timestamp { get; set; }
    public required string Channel { get; set; }
    public required string FromName { get; set; }
    public string? ToName { get; set; }
    public required string Message { get; set; }
}
