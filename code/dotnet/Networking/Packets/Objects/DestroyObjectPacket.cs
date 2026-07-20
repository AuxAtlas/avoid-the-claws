using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Networking.Data;
using LiteNetLib.Utils;

namespace AvoidClaws.code.dotnet.Networking.Packets.Objects;

public class DestroyObjectPacket : IGamePacket
{
    public KableId TargetObjectId { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(TargetObjectId);
    }

    public void Deserialize(NetDataReader reader)
    {
        TargetObjectId = reader.GetKableId();
    }
}