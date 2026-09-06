#region

using AvoidClaws.code.dotnet.Networking.Data;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Packets.State;

public record NetworkStatePacket : IGamePacket
{
    public NetworkState State { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(State);
    }

    public void Deserialize(NetDataReader reader)
    {
        State = reader.Get<NetworkState>();
    }
}