#region

using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Networking.Data;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Packets.Objects;

public record SpawnActorPacket : IGamePacket
{
    public KableId ActorId { get; set; }
    public KableConnectionId AuthorityConnectionId { get; set; }
    public CoreGame.ActorTypesEnum TypesEnum { get; set; }

    public ObjectState State { get; set; } = new();

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ActorId);
        writer.Put(AuthorityConnectionId);
        writer.Put((byte)TypesEnum);
        writer.Put(State);
    }

    public void Deserialize(NetDataReader reader)
    {
        ActorId.SetKableId(reader.GetUInt());
        AuthorityConnectionId.SetKableConnectionId(reader.GetUInt());
        TypesEnum = (CoreGame.ActorTypesEnum)reader.GetByte();
        State.Deserialize(reader);
    }
}