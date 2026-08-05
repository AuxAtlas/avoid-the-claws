using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Networking.Data;
using LiteNetLib.Utils;

namespace AvoidClaws.code.dotnet.Networking.Packets.Objects;

public class SpawnActorPacket : IGamePacket
{
    public KableId ActorId { get; set; }
    public KableConnectionId AuthorityConnectionId { get; set; }
    public CoreGame.ActorType Type { get; set; }

    public ObjectState State { get; set; } = new();

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ActorId);
        writer.Put(AuthorityConnectionId);
        writer.Put((byte)Type);
        writer.Put(State);
    }

    public void Deserialize(NetDataReader reader)
    {
        ActorId = reader.GetKableId();
        AuthorityConnectionId = reader.GetKableConnectionId();
        Type = (CoreGame.ActorType)reader.GetByte();
        State.Deserialize(reader);
    }
}