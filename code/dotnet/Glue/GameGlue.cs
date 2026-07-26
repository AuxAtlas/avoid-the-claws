using AvoidClaws.code.dotnet.Services;
using Godot;

namespace AvoidClaws.code.dotnet.Glue;

public partial class GameGlue : Node
{
    [Inject]
    protected CoreGame Core { get; } = null!;

    protected bool IsClient => Core.Network.IsClient;
    protected bool IsServer => Core.Network.IsServer;
}