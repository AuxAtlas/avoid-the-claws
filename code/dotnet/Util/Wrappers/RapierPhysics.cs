using Godot;

namespace AvoidClaws.code.dotnet.Util.Wrappers;

public partial class RapierPhysics : RefCounted
{
    private GodotObject _rapierPhysics3D = (GodotObject)ClassDB.Singleton.Instantiate("RapierPhysicsServer3D");
    
    
    public void SpaceStep3D(Rid space, double delta)
    {
        _rapierPhysics3D.Call("space_step", space, delta);
    }
    public void SpaceFlushQueries3D(Rid space)
    {
        _rapierPhysics3D.Call("space_flush_queries", space);
    }
}