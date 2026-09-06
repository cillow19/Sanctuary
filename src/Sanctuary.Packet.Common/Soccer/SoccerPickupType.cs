namespace Sanctuary.Packet.Common;

/// <summary>
/// Field pickup effects. Names confirmed from the client's asset manifest and audio/texture
/// names (bbe_soccer_pickup_speed/toughness/charge, Soccer_Pickup_KnockDown, Soccer_Pickup_
/// MultiBall, Soccer_Pickup_Spring). Ordinal values are a provisional placeholder ordering.
/// </summary>
public enum SoccerPickupType
{
    Speed,
    Toughness,
    Charge,
    Knockdown,
    MultiBall,
    Spring
}
