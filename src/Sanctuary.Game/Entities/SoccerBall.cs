using System.Numerics;

using Sanctuary.Game.Zones;
using Sanctuary.Packet;

namespace Sanctuary.Game.Entities;

// A real, server-simulated physics ball - not a static prop. Every tick it integrates gravity
// and bounces off a flat ground plane at GroundY, then broadcasts its state via the actual
// BaseSoccerPacket protocol (SoccerPacketUpdateSoccerBall) so it's ready to be a genuine
// client-side physics SoccerBall the moment BaseSoccerPacket.OpCode is confirmed - see that
// file. Until then the client has no way to route this packet anywhere, so nothing renders
// from it yet; this exists so the server-side half of the system is already correct.
public sealed class SoccerBall : Npc
{
    // Tuning is a placeholder - the client's real values (its own "Gravity"/restitution
    // config, see BaseSoccerPacket.cs notes) were never recovered from the binary.
    private const float Gravity = -20f;
    private const float Restitution = 0.5f;
    private const float RollingFriction = 0.92f;

    private readonly float _groundY;

    public Vector3 Velocity { get; set; }

    public SoccerBall(IZone zone, float groundY) : base(zone)
    {
        _groundY = groundY;
    }

    public override void UpdateEveryTick()
    {
        var dt = Zone.TickDeltaSeconds;

        var velocity = Velocity;
        velocity.Y += Gravity * dt;

        var position = new Vector3(Position.X, Position.Y, Position.Z) + (velocity * dt);

        if (position.Y <= _groundY)
        {
            position.Y = _groundY;

            if (velocity.Y < 0f)
                velocity.Y = -velocity.Y * Restitution;

            velocity.X *= RollingFriction;
            velocity.Z *= RollingFriction;
        }

        Velocity = velocity;

        UpdatePosition(new Vector4(position, 1f), Rotation);

        BroadcastState();
    }

    private void BroadcastState()
    {
        var packet = new SoccerPacketUpdateSoccerBall
        {
            Position = Position,
            Velocity = new Vector4(Velocity, 0f),
            OwnerGuid = 0
        };

        foreach (var player in Zone.Players)
            player.SendTunneled(packet);
    }
}
