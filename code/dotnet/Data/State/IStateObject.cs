namespace AvoidClaws.code.dotnet.Data.State;

public interface IStateObject
{
    public ObjectState GetCurrentState(uint currentTick);
    public void SetCurrentState(ObjectState state);
    public void IngestNetworkState(ObjectState state);
    public ObjectState GetHistoricState(uint targetTick);
    public void RewindToTick(uint targetTick);
    public void HandleReconciliationUntilTick(uint targetTick);
}