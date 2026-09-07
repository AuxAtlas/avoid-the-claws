using Godot;

namespace AvoidClaws.code.dotnet.Buffs;

public partial class TimerBuff : BaseBuff
{
    [Export]
    public float SecondsToLive { get; protected set; } = 5f;

    public override void HandleNetTick(uint tick)
    {
        base.HandleNetTick(tick);
        
        if (!IsProcessing)
            return;
        SecondsToLive -= TickDeltaTimeF;

        if (SecondsToLive < 0f)
        {
            ParentActor?.RemoveBuff(this, tick);
        }
    }
}