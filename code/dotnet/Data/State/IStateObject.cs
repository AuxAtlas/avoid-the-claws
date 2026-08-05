using AvoidClaws.code.dotnet.Networking.Data;

namespace AvoidClaws.code.dotnet.Data.State;

public interface IStateObject
{
    public KableId KableId { get; }

    public uint SpawnedOnTick { get; }

    public void KableSetup(KableId kableId);
    public void ReadStateFrom(ref ObjectState state);
    public void WriteStateTo(ref ObjectState state);
}