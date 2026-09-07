namespace AvoidClaws.code.dotnet.Glue.interfaces;

public interface ILifecycleObject
{
    public bool IsProcessing { get; }
    /// <summary>
    ///     Object is spawned but not in scene tree yet. Do not begin processing or accessing anything external.
    /// </summary>
    /// <param name="spawnedTick"></param>
    public void Spawned(uint spawnedTick);
    
    /// <summary>
    ///     Object is now properly registered into the game world. Access to non-service objects is now safe. 'Unpaused', if relevant.
    /// </summary>
    public void Start(uint tick);

    /// <summary>
    ///     Object is being told to stop all processing(to pause, essentially)
    /// </summary>
    public void Stop(uint tick);

    /// <summary>
    ///     Object is being told it is about to be unregistered from the game world
    /// </summary>
    public void Teardown(uint tick);
}