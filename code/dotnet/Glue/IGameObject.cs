#region

using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Glue.interfaces;
using AvoidClaws.code.dotnet.Networking.Data;

#endregion

namespace AvoidClaws.code.dotnet.Glue;

/// <summary>
///     Interface methods for all spawnable objects in the scene(Actors, Controllers, etc.)
/// </summary>
public interface IGameObject : IKableObject, IStateObject, ILifecycleObject
{
    /// <summary>
    /// The KableConnectionId that has authority over this object on the network
    /// </summary>
    public KableConnectionId AuthorityConnectionId { get; }

    /// <summary>
    ///     Called every game tick by the world management systems
    /// </summary>
    public void HandleNetTick(uint tick);

    internal void SetKableAuthority(KableConnectionId connectionId);
}