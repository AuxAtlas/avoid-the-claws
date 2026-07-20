using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Resources;
using LiteNetLib.Utils;

namespace AvoidClaws.code.dotnet.Networking.Packets.Objects;

public class SpawnControllerPacket : IGamePacket
{
    public KableId SpawnedObjectId { get; set; }
    public KableConnectionId AuthorityConnectionId { get; set; }
    public GameResources.ControllerType Type { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(SpawnedObjectId);
        writer.Put(AuthorityConnectionId);
        writer.Put((byte)Type);
    }

    public void Deserialize(NetDataReader reader)
    {
        SpawnedObjectId = reader.GetKableId();
        AuthorityConnectionId = reader.GetKableConnectionId();
        Type = (GameResources.ControllerType)reader.GetByte();
    }
}