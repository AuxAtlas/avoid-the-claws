#region

using LiteNetLib;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Data;

public class KableConnection(NetPeer? netPeer, KableId kableId) : IKableObject
{
    public int LastLatency { get; internal set; } = 0;
    public NetPeer? NetPeer { get; } = netPeer;
    public KableId KableId { get; } = kableId;
    public KableConnectionId AuthorityConnectionId => ConnectionId;
    public KableConnectionId ConnectionId { get; private set; } = new(kableId.Id);

    public bool ReconciliationMode => false;

    public uint SpawnedOnTick { get; }

    public void SetConnectionId(KableConnectionId connectionId)
    {
        ConnectionId = connectionId;
    }

    public void KableSetup(KableId kableId)
    {
    }


    public void SendPacket(NetDataWriter writer, DeliveryMethod deliveryMethod)
    {
        NetPeer?.Send(writer, deliveryMethod);
    }
}