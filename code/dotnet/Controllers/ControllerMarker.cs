#region

using System;

#endregion

namespace AvoidClaws.code.dotnet.Controllers;

[AttributeUsage(AttributeTargets.Class)]
public class ControllerMarker(CoreGame.ControllerTypesEnum controllerTypesEnum) : Attribute
{
    public CoreGame.ControllerTypesEnum ControllerTypesEnum { get; } = controllerTypesEnum;
}