using System;

namespace AvoidClaws.code.dotnet.Util;

public class WeakAction(object target, Delegate action)
{
    private readonly WeakReference _target = new(target);
    private readonly System.Reflection.MethodInfo _method = action.Method;

    public bool IsAlive => _target.IsAlive;

    public void Invoke(object eventData)
    {
        var target = _target.Target;
        if (target != null)
        {
            _method.Invoke(target, [eventData]);
        }
    }

    public bool IsFrom(object target)
    {
        return target.Equals(_target.Target);
    }
}