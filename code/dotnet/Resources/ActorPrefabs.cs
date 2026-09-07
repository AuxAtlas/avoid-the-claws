#region

using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Resources;

public partial class ActorPrefabs : Node
{
    [Export]
    public PackedScene? PlayerActorPrefab { get; private set; }
}