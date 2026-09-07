#region

using System;

#endregion

namespace AvoidClaws.code.dotnet.Buffs;

[AttributeUsage(AttributeTargets.Class)]
public class BuffMarker(CoreGame.BuffTypesEnum typesEnum) : Attribute
{
    public CoreGame.BuffTypesEnum BuffTypesEnum { get; } = typesEnum;
}