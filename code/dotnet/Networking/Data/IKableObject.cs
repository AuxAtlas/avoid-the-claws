using AvoidClaws.code.dotnet.Data.State;

namespace AvoidClaws.code.dotnet.Networking.Data;

public interface IKableObject
{
    public KableId KableId { get; }

    public KableConnectionId AuthorityConnectionId { get; }

    public uint SpawnedOnTick { get; }

    public void KableSetup(KableId kableId);

    public void SetKableAuthority(KableConnectionId connectionId);

    public bool ReconciliationMode { get; }

    public ObjectState GetCurrentState();
    public void SetCurrentState(ObjectState state);
    public void IngestNetworkState(ObjectState state);
}