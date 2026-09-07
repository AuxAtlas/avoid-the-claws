#region

using AvoidClaws.code.dotnet.Networking.Data;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Packets.Objects;

public record SpawnControllerPacket : IGamePacket
{
    public KableId SpawnedObjectId { get; set; }
    public KableConnectionId AuthorityConnectionId { get; set; }
    public CoreGame.ControllerTypesEnum TypesEnum { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(SpawnedObjectId);
        writer.Put(AuthorityConnectionId);
        writer.Put((byte)TypesEnum);
    }

    public void Deserialize(NetDataReader reader)
    {
        SpawnedObjectId.SetKableId(reader.GetUInt());
        AuthorityConnectionId.SetKableConnectionId(reader.GetUInt());
        TypesEnum = (CoreGame.ControllerTypesEnum)reader.GetByte();
    }
}