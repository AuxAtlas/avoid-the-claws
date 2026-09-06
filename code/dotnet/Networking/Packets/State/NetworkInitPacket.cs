#region

using AvoidClaws.code.dotnet.Networking.Data;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Packets.State;

public record NetworkInitPacket : IGamePacket
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
        AssignedConnectionId.SetKableConnectionId(reader.GetUInt());
        ServerConnectionId.SetKableConnectionId(reader.GetUInt());
    }
}