using System;
using AvoidClaws.code.dotnet.Resources;

namespace AvoidClaws.code.dotnet.Actors;

[AttributeUsage(AttributeTargets.Class)]
public class ActorAttribute(GameResources.ActorType actorType) : Attribute
{
    public GameResources.ActorType ActorType { get; } = actorType;
}