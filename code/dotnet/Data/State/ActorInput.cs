using Godot;

namespace AvoidClaws.code.dotnet.Data.State;

public struct ActorInput
{
    public uint NetworkTick;
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public byte AttackInputsPacked;
    public byte ActionInputsPacked;
}