using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;
using Godot;

namespace AvoidClaws.code.dotnet.Components.Core;

public partial class HealthComponent : BaseComponent
{
    [Export]
    private float _maxHealth = 100;

    [Signal]
    public delegate void HealthChangedEventHandler(HealthUpdateInfo healthUpdateInfo);

    [Signal]
    public delegate void DiedEventHandler();

    [Signal]
    public delegate void RevivedEventHandler();

    public float CurrentHealth { get; private set; } = 100f;

    public float MaxHealth
    {
        get => _maxHealth;
        private set
        {
            _maxHealth = value;
            if (CurrentHealth > _maxHealth)
                CurrentHealth = _maxHealth;
        }
    }

    public float CurrentHealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
    public bool HasHealthRemaining => CurrentHealth > 0f;
    public bool IsDamaged => CurrentHealth < MaxHealth;
    public bool IsDead { get; private set; }


    public override void SetupComponent()
    {
        Revive();
    }

    public override void ReadStateFrom(ObjectState state)
    {
        var oldHealth = CurrentHealth;
        CurrentHealth = state.ReadFloat();

        MaxHealth = state.ReadFloat();

        var oldIsDead = IsDead;
        IsDead = state.ReadByte() == 1;

        if (oldIsDead != IsDead)
        {
            if (IsDead)
                Die();
            else
                Revive();
        }
        else if (!Mathf.IsEqualApprox(oldHealth, CurrentHealth))
        {
            HealthUpdateInfo info = new()
            {
                OldHealth = oldHealth,
                NewHealth = CurrentHealth,
                MaxHealth = _maxHealth,
                SuppressSoundEffect = true
            };
            EmitSignalHealthChanged(info);
        }
    }

    public override void WriteStateTo(ObjectState state)
    {
        state.Put(CurrentHealth);
        state.Put(MaxHealth);
        state.Put(IsDead ? (byte)1 : (byte)0);
    }

    public void TakeDamage(float amount, bool suppressSoundEffect = false)
    {
        var oldHealth = CurrentHealth;
        if (amount > 0f)
            CurrentHealth -= amount;

        if (CurrentHealth < 0)
            CurrentHealth = 0;

        HealthUpdateInfo info = new()
        {
            OldHealth = oldHealth,
            NewHealth = CurrentHealth,
            MaxHealth = _maxHealth,
            SuppressSoundEffect = suppressSoundEffect
        };
        EmitSignalHealthChanged(info);
        if (!IsDead && !HasHealthRemaining) Die();
    }

    public void TakeHeal(float amount, bool suppressSoundEffect = false)
    {
        var oldHealth = CurrentHealth;
        if (amount > 0f)
            CurrentHealth += amount;

        if (CurrentHealth > _maxHealth)
            CurrentHealth = _maxHealth;

        HealthUpdateInfo info = new()
        {
            OldHealth = oldHealth,
            NewHealth = CurrentHealth,
            MaxHealth = _maxHealth,
            SuppressSoundEffect = suppressSoundEffect
        };

        EmitSignalHealthChanged(info);
        if (!IsDead && !HasHealthRemaining) Die();
    }

    public void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        HealthUpdateInfo info = new()
        {
            OldHealth = 0f,
            NewHealth = 0f,
            MaxHealth = _maxHealth,
            SuppressSoundEffect = true
        };
        EmitSignalHealthChanged(info);

        EmitSignalDied();
    }

    public void Revive()
    {
        IsDead = false;
        CurrentHealth = MaxHealth;

        HealthUpdateInfo info = new()
        {
            OldHealth = 0f,
            NewHealth = CurrentHealth,
            MaxHealth = _maxHealth,
            SuppressSoundEffect = true
        };
        EmitSignalHealthChanged(info);

        EmitSignalRevived();
    }


    public partial class HealthUpdateInfo : GodotObject
    {
        public float MaxHealth;
        public float OldHealth;
        public float NewHealth;
        public bool SuppressSoundEffect;

        public float NewHealthPercent => MaxHealth > 0 ? NewHealth / MaxHealth : 0f;
        public float OldHealthPercent => MaxHealth > 0 ? OldHealth / MaxHealth : 0f;
    }
}