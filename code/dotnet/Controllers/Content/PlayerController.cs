using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Packets.State;
using Godot;

namespace AvoidClaws.code.dotnet.Controllers.Content;

public partial class PlayerController : BlankController
{
    private readonly ControllerInputsPacket _controllerInputsPacketReusable = new();
    
    public override void Start(uint tick)
    {
        base.Start(tick);
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void Stop(uint tick)
    {
        base.Stop(tick);
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    protected override void KableSetupCustom(KableId kableId)
    {
        base.KableSetupCustom(kableId);
        _controllerInputsPacketReusable.ControllerKableId = kableId;
    }

    public override void Attach(IActor actor)
    {
        base.Attach(actor);
        if(actor is LivingActor livingActor)
            livingActor.SetClientFocused();
    }

    protected override void HandleNetTickCustom(uint tick)
    {
        base.HandleNetTickCustom(tick);

        if (!IsLogicAuthority())
        {
            return;
        }
        
        Inputs.Clear();
        
        if (Input.IsActionPressed(ActionName.MoveForward.ToActionString()))
        {
            Inputs.MoveInput.Y -= 1f;
        }
        if (Input.IsActionPressed(ActionName.MoveBackward.ToActionString()))
        {
            Inputs.MoveInput.Y += 1f;
        }

        if (Input.IsActionPressed(ActionName.MoveRight.ToActionString()))
        {
            Inputs.MoveInput.X += 1f;
        }
        if (Input.IsActionPressed(ActionName.MoveLeft.ToActionString()))
        {
            Inputs.MoveInput.X -= 1f;
        }
        
        Inputs.ActionInputsPacked = Inputs.ActionInputsPacked.WithBitSet(0, Input.IsActionPressed(ActionName.MoveUp.ToActionString()));
        Inputs.ActionInputsPacked = Inputs.ActionInputsPacked.WithBitSet(1, Input.IsActionPressed(ActionName.MoveDown.ToActionString()));

        Inputs.AttackInputsPacked = Inputs.AttackInputsPacked.WithBitSet(0, Input.IsActionPressed(ActionName.AttackPrimary.ToActionString()));
        Inputs.AttackInputsPacked = Inputs.AttackInputsPacked.WithBitSet(1, Input.IsActionPressed(ActionName.AttackSecondary.ToActionString()));

        if (IsClient)
        {
            _controllerInputsPacketReusable.Inputs = Inputs;
            Core.Network.SendToAllReliableUnordered(_controllerInputsPacketReusable);
        }
    }
}