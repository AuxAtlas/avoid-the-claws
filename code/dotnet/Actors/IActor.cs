using System.Collections.Immutable;
using System.ComponentModel;
using AvoidClaws.code.dotnet.Buffs;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Resources;
using Godot;

namespace AvoidClaws.code.dotnet.Actors;

public interface IActor : IKableObject
{
    public BoxShape3D? HurtBox { get; }
    public Vector3 GlobalPosition { get; }
    public Vector3 GlobalRotation { get; }

    public bool IsDead { get; }

    public void SetMovementInput(Vector2 input);
    public void SetRotationInput(Vector2 input);
    public void SetAttackInputsPacked(byte input);
    public void SetActionInputsPacked(byte input);

    public T? GetComponent<T>() where T : IComponent;

    public void Destroy();

    public void SetClientFocused();

    public void HandleNetTick(uint tick);

    public IBuff? CreateAndApplyBuff(GameResources.BuffType buffType, KableId? presetKableId = null);
    public void RemoveBuff(IBuff buff);
    public ImmutableArray<IBuff> GetBuffs();

    public void TakeDamage(float amount, IKableObject? source);
    public void TakeHeal(float amount, IKableObject? source);
    public void Kill(IKableObject? source);

    public void TeleportTo(Vector3 position, Vector3? rotation = null);
    public void Respawn();
}