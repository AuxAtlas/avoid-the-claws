using System;
using AvoidClaws.code.dotnet.Resources;

namespace AvoidClaws.code.dotnet.Buffs;

[AttributeUsage(AttributeTargets.Class)]
public class BuffAttribute(GameResources.BuffType type) : Attribute
{
    public GameResources.BuffType BuffType { get; } = type;
}