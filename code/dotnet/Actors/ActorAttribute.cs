#region

using System;

#endregion

namespace AvoidClaws.code.dotnet.Actors;

[AttributeUsage(AttributeTargets.Class)]
public class ActorAttribute(CoreGame.ActorType actorType) : Attribute
{
    public CoreGame.ActorType ActorType { get; } = actorType;
}