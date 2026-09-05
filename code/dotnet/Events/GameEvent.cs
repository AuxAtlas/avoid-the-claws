namespace AvoidClaws.code.dotnet.Events;

public abstract record GameEvent
{
    private bool _cancelled;

    public bool GetCancelled()
    {
        return _cancelled;
    }

    public void SetCancelled()
    {
        _cancelled = true;
    }
}