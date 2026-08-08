#region

using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Networking.Data;

#endregion

namespace AvoidClaws.code.dotnet.Glue;

/// <summary>
///     Interface methods for all spawnable objects in the scene(Actors, Controllers, etc.)
/// </summary>
public interface IGameObject : IKableObject, IStateObject
{
    /// <summary>
    /// The KableConnectionId that has authority over this object on the network
    /// </summary>
    public KableConnectionId AuthorityConnectionId { get; }

    /// <summary>
    ///     Handle object initialization only. Do not access any non-service objects yet.
    /// </summary>
    public void Setup();

    /// <summary>
    ///     Object is now properly registered into the game world. Access to non-service objects is now safe.
    /// </summary>
    public void Start(uint startTick);

    /// <summary>
    ///     Called every game tick by the world management systems
    /// </summary>
    public void HandleNetTick(uint tick);

    /// <summary>
    ///     Object is being told to stop all processing(to pause, essentially)
    /// </summary>
    public void Stop();

    /// <summary>
    ///     Object is being told it is about to be unregistered from the game world
    /// </summary>
    public void Teardown();

    internal void SetKableAuthority(KableConnectionId connectionId);
}