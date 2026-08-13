#region

using System.Collections.Generic;
using AvoidClaws.code.dotnet.Buffs;
using AvoidClaws.code.dotnet.Components;
using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Glue;
using AvoidClaws.code.dotnet.Networking.Data;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Actors;

public interface IActor : IGameObject
{
    public BoxShape3D? HurtBox { get; }
    public Vector3 GlobalPosition { get; }
    public Vector3 GlobalRotation { get; }

    public bool IsDead { get; }

    public void SetInputs(ref readonly ControllerInputs inputs);

    public T? GetComponent<T>() where T : IComponent;

    public void Destroy();

    public void SetClientFocused();

    public void ApplyBuff(IBuff buff);
    public void RemoveBuff(IBuff buff);
    public IEnumerable<IBuff> GetBuffs();
    public void Destroy(IKableObject? source);

    public void TeleportTo(Vector3 position, Vector3? rotation = null);
    public void Respawn();
}