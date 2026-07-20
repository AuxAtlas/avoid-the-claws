using System;
using AvoidClaws.code.dotnet.Resources;

namespace AvoidClaws.code.dotnet.Controllers;

[AttributeUsage(AttributeTargets.Class)]
public class ControllerAttribute(GameResources.ControllerType controllerType) : Attribute
{
    public GameResources.ControllerType ControllerType { get; } = controllerType;
}