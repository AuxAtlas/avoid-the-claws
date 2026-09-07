#region

using AvoidClaws.code.dotnet.Data.State;
using Godot;

#endregion

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
        get => _maxHealth * BuffMaxHealthMultiplier;
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

    private readonly ObjectState _stateCache = new();

    private readonly HealthUpdateInfo _healthUpdateInfoReusable = new();

    public float BuffMaxHealthMultiplier
    {
        get => _buffMaxHealthMultiplier;
        set
        {
            if (value < 0.01f)
                value = 0.01f;
            
            if (value * _maxHealth < CurrentHealth)
            {
                CurrentHealth = (_maxHealth * value);
            }

            _buffMaxHealthMultiplier = value;
        }
    }
    private float _buffMaxHealthMultiplier = 1f;

    public override void Start(uint tick)
    {
        base.Start(tick);
        Revive();
    }


    protected override void GetCurrentStateCustom(in ObjectState state)
    {
        _stateCache.Put(CurrentHealth);
        _stateCache.Put(MaxHealth);
        _stateCache.Put(IsDead ? (byte)1 : (byte)0);
    }
    protected override void SetCurrentStateCustom(in ObjectState state)
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
            _healthUpdateInfoReusable.OldHealth = oldHealth;
            _healthUpdateInfoReusable.NewHealth = CurrentHealth;
            _healthUpdateInfoReusable.MaxHealth = MaxHealth;
            _healthUpdateInfoReusable.SuppressSoundEffect = true;
            EmitSignalHealthChanged(_healthUpdateInfoReusable);
        }
    }

    public void TakeDamage(float amount, bool suppressSoundEffect = false)
    {
        var oldHealth = CurrentHealth;
        if (amount > 0f)
            CurrentHealth -= amount;

        if (CurrentHealth < 0)
            CurrentHealth = 0;

        _healthUpdateInfoReusable.OldHealth = oldHealth;
        _healthUpdateInfoReusable.NewHealth = CurrentHealth;
        _healthUpdateInfoReusable.MaxHealth = MaxHealth;
        _healthUpdateInfoReusable.SuppressSoundEffect = suppressSoundEffect;
        EmitSignalHealthChanged(_healthUpdateInfoReusable);
        
        if (!IsDead && !HasHealthRemaining)
            Die();
    }

    public void TakeHeal(float amount, bool suppressSoundEffect = false)
    {
        var oldHealth = CurrentHealth;
        if (amount > 0f)
            CurrentHealth += amount;

        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
        _healthUpdateInfoReusable.OldHealth = oldHealth;
        _healthUpdateInfoReusable.NewHealth = CurrentHealth;
        _healthUpdateInfoReusable.MaxHealth = MaxHealth;
        _healthUpdateInfoReusable.SuppressSoundEffect = suppressSoundEffect;

        EmitSignalHealthChanged(_healthUpdateInfoReusable);
        
        if (!IsDead && !HasHealthRemaining)
            Die();
    }

    public void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        _healthUpdateInfoReusable.OldHealth = 0f;
        _healthUpdateInfoReusable.NewHealth = 0f;
        _healthUpdateInfoReusable.MaxHealth = MaxHealth;
        _healthUpdateInfoReusable.SuppressSoundEffect = true;

        EmitSignalHealthChanged(_healthUpdateInfoReusable);

        EmitSignalDied();
    }

    public void Revive()
    {
        IsDead = false;
        CurrentHealth = MaxHealth;

        _healthUpdateInfoReusable.OldHealth = 0f;
        _healthUpdateInfoReusable.NewHealth = CurrentHealth;
        _healthUpdateInfoReusable.MaxHealth = MaxHealth;
        _healthUpdateInfoReusable.SuppressSoundEffect = true;
        
        EmitSignalHealthChanged(_healthUpdateInfoReusable);

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