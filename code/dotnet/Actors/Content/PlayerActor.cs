using AvoidClaws.code.dotnet.Data.State;

namespace AvoidClaws.code.dotnet.Actors.Content;

public partial class PlayerActor : LivingActor
{
    protected override void ProcessInputCustom(float deltaTimeF, ControllerInputs input)
    {
        Inputs.MoveInput = input.MoveInput.Clamp(-1f, 1f);
        Inputs.LookInput = input.LookInput.Clamp(-1f, 1f);
        Inputs.AttackInputsPacked = input.AttackInputsPacked;
        Inputs.ActionInputsPacked = input.ActionInputsPacked;
    }
    protected override void GetCurrentStateCustom(in ObjectState stateBuffer)
    {
        
    }
    protected override void SetCurrentStateCustom(in ObjectState objectState)
    {
        
    }
}