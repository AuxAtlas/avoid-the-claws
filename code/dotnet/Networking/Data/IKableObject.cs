namespace AvoidClaws.code.dotnet.Networking.Data;

public interface IKableObject
{
    public KableId KableId { get; }

    public uint SpawnedOnTick { get; }

    public void KableSetup(KableId kableId);
}