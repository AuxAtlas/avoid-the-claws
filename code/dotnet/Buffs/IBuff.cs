#region

using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Glue;

#endregion

namespace AvoidClaws.code.dotnet.Buffs;

/// <summary>
/// A 'IBuff' is essentially an 'IComponent' that can be added/removed during gameplay
/// </summary>
public interface IBuff : IGameObject
{
    public IActor? ParentActor { get; }
    /// <summary>
    /// If a buff has already been previously attached to an IActor and then removed, mark buff as dirty
    /// </summary>
    bool IsDirty { get; }

    public void OnAttachedHandler(IActor parentActor, uint tick);
    public void OnDetachedHandler(uint tick);
}