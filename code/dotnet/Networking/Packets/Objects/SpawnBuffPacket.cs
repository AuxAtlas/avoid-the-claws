#region

using AvoidClaws.code.dotnet.Networking.Data;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Packets.Objects;

public class SpawnBuffPacket : IGamePacket
{
    public KableId SpawnedBuffId { get; set; }
    public KableId OwnerActorId { get; set; }
    public KableConnectionId AuthorityConnectionId { get; set; }
    public CoreGame.BuffType BuffType { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(SpawnedBuffId);
        writer.Put(OwnerActorId);
        writer.Put(AuthorityConnectionId);
        writer.Put((byte)BuffType);
    }

    public void Deserialize(NetDataReader reader)
    {
        SpawnedBuffId.SetKableId(reader.GetUInt());
        OwnerActorId.SetKableId(reader.GetUInt());
        AuthorityConnectionId.SetKableConnectionId(reader.GetUInt());
        BuffType = (CoreGame.BuffType)reader.GetByte();
    }
}