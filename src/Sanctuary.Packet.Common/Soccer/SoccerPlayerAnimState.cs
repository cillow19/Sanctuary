namespace Sanctuary.Packet.Common;

/// <summary>
/// Player/goalie action states used to keep everyone's animation in sync.
/// The names are confirmed from the client binary's string table (cSoccerPlayerState* /
/// cSoccerGoalieState* constants, found via Ghidra). The client only exposed these as debug
/// name strings, not a decompiled numeric mapping, so the ordinal values below are a
/// provisional, sequential placeholder ordering - NOT confirmed against the real protocol.
/// Re-number once a packet capture (or further static analysis) confirms the real ids.
/// </summary>
public enum SoccerPlayerAnimState
{
    None,
    Idle,
    Run,
    Jump,
    Land,
    TurnLeft,
    TurnRight,
    Juke,
    JukeWithBall,
    Juke360WithBall,
    TurnLeftWithBall,
    TurnRightWithBall,
    AcquireBall,
    ReceiveBall,
    RunningReceiveBall,
    RunWithBall,
    KickLeft,
    KickRight,
    KickBack,
    HighKickLeft,
    HighKickRight,
    HighKickBack,
    KickLeftRunning,
    KickRightRunning,
    KickLeftRunningStop,
    KickRightRunningStop,
    PassKickRunning,
    HighPassKickRunning,
    PassKickBack,
    HighPassKickBack,
    LobKickRunning,
    SuperKick,
    SuperKickBack,
    StealTackle,
    SlideTackleStart,
    SlideTackleMiddle,
    SlideTackleEnd,
    Stumble,
    Impact,
    HitLow,
    HitMid,
    HitHigh,
    GetupBack,
    Celebrate1,
    Celebrate2,
    Celebrate3,
    Disappointed1,
    Disappointed2,
    Disappointed3,

    // Goalie-only states.
    GoalieIdleWithBallInHand,
    GoalieThrow,
    GoalieKick,
    GoalieCatchLow,
    GoalieCatchMid,
    GoalieCatchHigh,
    GoalieBlockCenter,
    GoalieBlockLowLeft,
    GoalieBlockLowRight,
    GoalieBlockHighLeft,
    GoalieBlockHighRight,
    GoalieGetupLeft,
    GoalieGetupRight
}
