#region

using AvoidClaws.code.dotnet.Extensions;
using Godot;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Data.State;

public class ControllerInputs : INetSerializable
{
    public uint NetworkTick;
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public byte AttackInputsPacked;
    public byte ActionInputsPacked;
    public void Serialize(NetDataWriter writer)
    {
        writer.Put(NetworkTick);
        writer.Put(MoveInput);
        writer.Put(LookInput);
        writer.Put(AttackInputsPacked);
        writer.Put(ActionInputsPacked);
    }
    public void Deserialize(NetDataReader reader)
    {
        NetworkTick = reader.GetUInt();
        MoveInput = reader.GetVector2();
        LookInput = reader.GetVector2();
        AttackInputsPacked = reader.GetByte();
        ActionInputsPacked = reader.GetByte();
    }
}