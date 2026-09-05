using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Packets.State;
using Godot;

namespace AvoidClaws.code.dotnet.Controllers.Content;

public partial class PlayerController : BlankController
{
    protected override void KableSetupCustom(KableId kableId)
    {
        base.KableSetupCustom(kableId);
        _controllerInputsPacket.ControllerKableId = kableId;
    }

    protected override void HandleNetTickCustom(uint tick)
    {
        base.HandleNetTickCustom(tick);

        Inputs.Clear();
        
        if (Input.IsActionPressed(ActionName.MoveForward.ToActionString()))
        {
            Inputs.MoveInput.Y += 1f;
        }
        else if (Input.IsActionPressed(ActionName.MoveBackward.ToActionString()))
        {
            Inputs.MoveInput.Y -= -1f;
        }

        if (Input.IsActionPressed(ActionName.MoveRight.ToActionString()))
        {
            Inputs.MoveInput.X += 1f;
        }
        else if (Input.IsActionPressed(ActionName.MoveLeft.ToActionString()))
        {
            Inputs.MoveInput.X -= -1f;
        }
        
        Inputs.ActionInputsPacked = Inputs.ActionInputsPacked.WithBitSet(0, Input.IsActionPressed(ActionName.MoveUp.ToActionString()));
        Inputs.ActionInputsPacked = Inputs.ActionInputsPacked.WithBitSet(1, Input.IsActionPressed(ActionName.MoveDown.ToActionString()));

        Inputs.AttackInputsPacked = Inputs.AttackInputsPacked.WithBitSet(0, Input.IsActionPressed(ActionName.AttackPrimary.ToActionString()));
        Inputs.AttackInputsPacked = Inputs.AttackInputsPacked.WithBitSet(1, Input.IsActionPressed(ActionName.AttackSecondary.ToActionString()));

        if (IsClient)
        {
            _controllerInputsPacket.Inputs = Inputs;
            Core.Network.SendToAllReliableUnordered(_controllerInputsPacket);
        }
    }

    private readonly ControllerInputsPacket _controllerInputsPacket = new();
}