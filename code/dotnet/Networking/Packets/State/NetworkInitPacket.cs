using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Networking.Data;
using LiteNetLib.Utils;

namespace AvoidClaws.code.dotnet.Networking.Packets.State;

public class NetworkInitPacket : IGamePacket
{
    public KableConnectionId AssignedConnectionId { get; set; }
    public KableConnectionId ServerConnectionId { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(AssignedConnectionId);
        writer.Put(ServerConnectionId);
    }

    public void Deserialize(NetDataReader reader)
    {
        AssignedConnectionId = reader.GetKableConnectionId();
        ServerConnectionId = reader.GetKableConnectionId();
    }
}