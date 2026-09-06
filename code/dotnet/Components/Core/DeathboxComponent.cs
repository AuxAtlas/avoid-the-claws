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
	
	protected override void GetCurrentStateCustom(in ObjectState stateBuffer) { }
	protected override void SetCurrentStateCustom(in ObjectState state) { }
}
