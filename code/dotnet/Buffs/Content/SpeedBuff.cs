using AvoidClaws.code.dotnet.Components.Core;
using Godot;

namespace AvoidClaws.code.dotnet.Buffs.Content;

public partial class SpeedBuff : TimerBuff
{
    [Export]
    private float _speedBuffMultiplier = 2f;

    [Export]
    private float _accelerationMultiplier = 2f;
    
    protected override void OnAttachedCustom(uint tick)
    {
        base.OnAttachedCustom(tick);

        GroundMovementComponent? groundMovementComponent = ParentActor?.GetComponent<GroundMovementComponent>();
        if (groundMovementComponent == null)
            return;

        groundMovementComponent.BuffSpeedMultiplier *= _speedBuffMultiplier;
        groundMovementComponent.BuffAccelerationMultiplier *= _accelerationMultiplier;
    }

    protected override void OnDetachedCustom(uint tick)
    {
        base.OnDetachedCustom(tick);

        GroundMovementComponent? groundMovementComponent = ParentActor?.GetComponent<GroundMovementComponent>();
        if (groundMovementComponent == null)
            return;

        groundMovementComponent.BuffSpeedMultiplier /= _speedBuffMultiplier;
        groundMovementComponent.BuffAccelerationMultiplier /= _accelerationMultiplier;
    }
}