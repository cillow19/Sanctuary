using System;

namespace Sanctuary.WebAPI.Models;

public class ChatLogRequestModel
{
    public string? CharacterName { get; set; }
    public string? Channel { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? Limit { get; set; }
}
