namespace AvoidClaws.code.dotnet.Data.State;

public interface IStateObject
{
    public ObjectState GetCurrentState(uint currentTick);
    public void SetCurrentState(in ObjectState state);
    public void IngestNetworkState(in ObjectState state);
    public ObjectState GetHistoricState(uint targetTick);
    public void RewindToTick(uint targetTick);
}