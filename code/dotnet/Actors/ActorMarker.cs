#region

using System;

#endregion

namespace AvoidClaws.code.dotnet.Actors;

[AttributeUsage(AttributeTargets.Class)]
public class ActorMarker(CoreGame.ActorTypesEnum actorTypesEnum) : Attribute
{
    public CoreGame.ActorTypesEnum ActorTypesEnum { get; } = actorTypesEnum;
}