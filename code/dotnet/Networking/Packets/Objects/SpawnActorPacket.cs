using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Resources;
using LiteNetLib.Utils;

namespace AvoidClaws.code.dotnet.Networking.Packets.Objects;

public class SpawnActorPacket : IGamePacket
{
    public KableId ActorId { get; set; }
    public KableConnectionId AuthorityConnectionId { get; set; }
    public GameResources.ActorType Type { get; set; }

    public ObjectState State { get; set; }

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
        Type = (GameResources.ActorType)reader.GetByte();
        State = reader.Get<ObjectState>();
    }
}