namespace Sanctuary.Packet.Common;

/// <summary>
/// Match state machine values. Confirmed from the client binary (FreeRealms.exe), which contains
/// a state-id -> name lookup function used for logging (decompiled via Ghidra). Gaps between the
/// known values (e.g. 2-6, 9, 13-15, 19-20) exist in the client too - it falls back to "Unknown"
/// for anything not listed below, so those ids were left out rather than guessed at.
/// </summary>
public enum SoccerGameState
{
    InitializeGame = 1,
    StartGame = 7,
    Halftime = 8,
    EndGame = 10,
    KickOff = 11,
    PlayingSoccer = 12,
    StartGoalCelebration = 16,
    GoalCelebration = 17,
    EndGoalCelebration = 18,
    SuperShot = 21,
    Winner = 22
}
