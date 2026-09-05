namespace AvoidClaws.code.dotnet.Glue.interfaces;

public interface ILifecycleObject
{
    /// <summary>
    ///     Notify object of what tick it was spawned on
    /// </summary>
    /// <param name="spawnedTick"></param>
    public void Spawned(uint spawnedTick);

    /// <summary>
    ///     Handle object initialization only. Do not access any non-service objects yet.
    /// </summary>
    public void Setup(uint tick);
    
    /// <summary>
    ///     Object is now properly registered into the game world. Access to non-service objects is now safe.
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