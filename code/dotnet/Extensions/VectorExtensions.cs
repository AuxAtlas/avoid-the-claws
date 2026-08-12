#region

using System;
using Godot;

#endregion

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

        public static bool IsEqualApprox(Vector3 a, Vector3 b, float tolerance)
        {
            return Mathf.IsEqualApprox(a.X, b.X, tolerance) && Mathf.IsEqualApprox(a.Y, b.Y, tolerance) && Mathf.IsEqualApprox(a.Z, b.Z, tolerance);
        }
    }
}