using System;

namespace AvoidClaws.code.dotnet.Controllers;

[AttributeUsage(AttributeTargets.Class)]
public class ControllerAttribute(CoreGame.ControllerType controllerType) : Attribute
{
    public CoreGame.ControllerType ControllerType { get; } = controllerType;
}