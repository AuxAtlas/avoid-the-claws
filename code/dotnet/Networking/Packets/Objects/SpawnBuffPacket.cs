#region

using AvoidClaws.code.dotnet.Networking.Data;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Packets.Objects;

public record SpawnBuffPacket : IGamePacket
{
    public KableId SpawnedBuffId { get; set; }
    public KableId OwnerActorId { get; set; }
    public KableConnectionId AuthorityConnectionId { get; set; }
    public CoreGame.BuffTypesEnum BuffTypesEnum { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(SpawnedBuffId);
        writer.Put(OwnerActorId);
        writer.Put(AuthorityConnectionId);
        writer.Put((byte)BuffTypesEnum);
    }

    public void Deserialize(NetDataReader reader)
    {
        SpawnedBuffId.SetKableId(reader.GetUInt());
        OwnerActorId.SetKableId(reader.GetUInt());
        AuthorityConnectionId.SetKableConnectionId(reader.GetUInt());
        BuffTypesEnum = (CoreGame.BuffTypesEnum)reader.GetByte();
    }
}