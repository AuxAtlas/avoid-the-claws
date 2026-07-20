using AvoidClaws.code.dotnet.Services;
using Godot;

namespace AvoidClaws.code.dotnet.UI;

public partial class BasicScreen : Control
{
    [Inject]
    protected CoreGame Core { get; } = null!;
}