#region

using AvoidClaws.code.dotnet.Networking.Data;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Packets.Objects;

public class SpawnControllerPacket : IGamePacket
{
    public KableId SpawnedObjectId { get; set; }
    public KableConnectionId AuthorityConnectionId { get; set; }
    public CoreGame.ControllerType Type { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(SpawnedObjectId);
        writer.Put(AuthorityConnectionId);
        writer.Put((byte)Type);
    }

    public void Deserialize(NetDataReader reader)
    {
        SpawnedObjectId.SetKableId(reader.GetUInt());
        AuthorityConnectionId.SetKableConnectionId(reader.GetUInt());
        Type = (CoreGame.ControllerType)reader.GetByte();
    }
}