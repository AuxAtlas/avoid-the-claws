using Godot;

namespace AvoidClaws.code.dotnet.Extensions;

public static class VectorExtensions
{
    extension(Vector3 vec)
    {
        public Vector3 ProjectOntoPlane(Vector3 plane)
        {
            var normal = plane.Normalized();
            var projectionOntoNormal = normal * vec.Dot(normal);

            return vec - projectionOntoNormal;
        }
    }
}