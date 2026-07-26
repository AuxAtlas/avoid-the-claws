using System;
using AvoidClaws.code.dotnet.Services;

namespace AvoidClaws.code.dotnet.Buffs;

[AttributeUsage(AttributeTargets.Class)]
public class BuffAttribute(CoreGame.BuffType type) : Attribute
{
    public CoreGame.BuffType BuffType { get; } = type;
}