using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Glue.Managers;
using Godot;

namespace AvoidClaws.code.dotnet.Components.Core;

public partial class GroundMovementComponent : BaseComponent
{
    private LivingActor? _livingParentActor;

#region EXPORTS

    [Export]
    private float _speed = 8f;
    
    [Export]
    private float _acceleration = 6f;

#endregion

    public override void _Ready()
    {
        base._Ready();
        
        if (ParentActor is LivingActor livingActor)
            _livingParentActor = livingActor;
    }

    public override void ProcessInput(ControllerInputs inputs, uint tickToProcess)
    {
        if (_livingParentActor == null)
            return;

        Vector3 vel = _livingParentActor.Velocity;
        Vector3 moveDirection = (_livingParentActor.Transform.Basis * new Vector3(inputs.MoveInput.X, 0, inputs.MoveInput.Y)).Normalized();

        if (moveDirection.IsZeroApprox())
        {
            vel.X = Mathf.Lerp(vel.X, 0f, _acceleration * NetworkManager.TickDeltaTimeF);
            vel.Z = Mathf.Lerp(vel.Z, 0f, _acceleration * NetworkManager.TickDeltaTimeF);
        }
        else
        {
            vel.X = Mathf.Lerp(vel.X, moveDirection.X * _speed,_acceleration * NetworkManager.TickDeltaTimeF);
            vel.Z = Mathf.Lerp(vel.Z, moveDirection.Z * _speed,_acceleration * NetworkManager.TickDeltaTimeF);
        }

        if (!_livingParentActor.IsOnFloor())
        {
            vel.Y += _livingParentActor.GetGravity().Y * NetworkManager.TickDeltaTimeF;
        }

        _livingParentActor.Velocity = vel;
    }
}