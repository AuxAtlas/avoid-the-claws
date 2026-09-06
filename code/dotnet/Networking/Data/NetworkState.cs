#region

using System.Collections.Generic;
using AvoidClaws.code.dotnet.Data.State;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Data;

public struct NetworkState : INetSerializable
{
    public KableConnectionId ServerKableId { get; set; }

    public uint NetworkTick { get; set; }

    public bool FinishedInitialSync { get; set; }

    public List<ObjectState> ObjectStates { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ServerKableId.Id);
        writer.Put(NetworkTick);
        writer.Put(FinishedInitialSync);

        writer.PutArray(ObjectStates.ToArray());
    }

    public void Deserialize(NetDataReader reader)
    {
        ServerKableId = new KableConnectionId(reader.GetUInt());
        NetworkTick = reader.GetUInt();
        FinishedInitialSync = reader.GetBool();

        ObjectStates.Clear();
        ObjectStates.AddRange(reader.GetArray<ObjectState>());
    }
}