using AvoidClaws.code.dotnet.Data.State;
using Godot;

namespace AvoidClaws.code.dotnet.Components.Core;

public partial class DeathboxComponent : BaseComponent
{
	public void HandleObjectTouched(Node3D collisionObject)
	{
		if (collisionObject is RigidBody3D rigidBody)
		{
			rigidBody.GlobalPosition = new Vector3(0f, 10f, 0f);
			rigidBody.SetLinearVelocity(Vector3.Zero);
			rigidBody.SetAngularVelocity(Vector3.Zero);
		}
	}
	
	public override ObjectState GetCurrentState(uint currentTick)
	{
		return ObjectState.BlankStateRef;
	}
	public override void SetCurrentState(in ObjectState state)
	{
        
	}
}
