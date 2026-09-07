using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Glue.Managers;
using Godot;

namespace AvoidClaws.code.dotnet.Components.Core;

public partial class FpvCameraComponent : BaseComponent
{
#region EXPORTS
    
    [Export(PropertyHint.Range, "0.1,20,0.1")]
    private float _mouseSensitivity = 0.4f;
    
    [Export(PropertyHint.Range, "-90,0,5")]
    private float _minPitch = -85;
    
    [Export(PropertyHint.Range, "0,90,5")]
    private float _maxPitch = 85;
    
    [Export]
    public Camera3D? TargetCamera { get; set; }
    
#endregion
    private Vector2 _cameraRotBuffer;
    

    public override void HandleNetTick(uint tick)
    {
        base.HandleNetTick(tick);
        
        if (LivingParentActor == null || TargetCamera == null)
            return;
        
        _cameraRotBuffer.X = Mathf.Clamp(_cameraRotBuffer.X,  Mathf.DegToRad(_minPitch), Mathf.DegToRad(_maxPitch));
        Vector3 camRot = TargetCamera.Rotation;
        Vector3 actorRot = LivingParentActor.Rotation;
        camRot.X = _cameraRotBuffer.X;
        actorRot.Y = _cameraRotBuffer.Y;
        
        TargetCamera.Rotation = camRot;
        LivingParentActor.Rotation = actorRot;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            _cameraRotBuffer.X -= mouseMotion.Relative.Y * _mouseSensitivity * NetworkManager.TickDeltaTimeF;
            _cameraRotBuffer.Y -= mouseMotion.Relative.X * _mouseSensitivity * NetworkManager.TickDeltaTimeF;
        }
    }
}