using AvoidClaws.code.dotnet.Networking.Data;

namespace AvoidClaws.code.dotnet.Glue;

/// <summary>
///     Interface methods for all spawnable objects in the scene(Actors, Controllers, etc.)
/// </summary>
public interface IGameObject : IKableObject
{
    /// <summary>
    ///     Handle object initialization only. Do not access any non-service objects yet.
    /// </summary>
    public void Setup();

    /// <summary>
    ///     Object is now properly registered into the game world. Access to non-service objects is now safe.
    /// </summary>
    public void Start();

    /// <summary>
    ///     Called every game tick by the world management systems
    /// </summary>
    public void GameTick();

    /// <summary>
    ///     Object is being told to stop all processing(to pause, essentially)
    /// </summary>
    public void Stop();

    /// <summary>
    ///     Object is being told it is about to be unregistered from the game world
    /// </summary>
    public void Teardown();
}