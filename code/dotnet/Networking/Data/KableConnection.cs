using AvoidClaws.code.dotnet.Data.State;
using LiteNetLib;
using LiteNetLib.Utils;

namespace AvoidClaws.code.dotnet.Networking.Data;

public class KableConnection(NetPeer netPeer, KableId kableId) : IKableObject
{
    public int LastLatency { get; internal set; } = 0;
    public NetPeer NetPeer { get; } = netPeer;
    public KableId KableId { get; } = kableId;
    public KableConnectionId AuthorityConnectionId => ConnectionId;
    public KableConnectionId ConnectionId { get; private set; } = new(kableId.Id);


    public bool ReconciliationMode => false;

    public ObjectState GetCurrentState()
    {
        var state = new ObjectState
        {
            NetworkTick = 0,
            ObjectId = KableId,
            AuthorityConnectionId = AuthorityConnectionId
        };

        return state;
    }

    public void SetCurrentState(ObjectState state)
    {
    }

    public void IngestNetworkState(ObjectState state)
    {
    }

    public uint SpawnedOnTick { get; }

    public void SetConnectionId(KableConnectionId connectionId)
    {
        ConnectionId = connectionId;
    }

    public void KableSetup(KableId kableId)
    {
    }

    public void SetKableAuthority(KableConnectionId connectionId)
    {
    }

    public virtual void RewindToTick(uint rewindTick)
    {
    }


    public void SendPacket(NetDataWriter writer, DeliveryMethod deliveryMethod)
    {
        NetPeer.Send(writer, deliveryMethod);
    }
}