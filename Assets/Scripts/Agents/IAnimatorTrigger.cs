using System;

namespace Agents
{
    public interface IAnimatorTrigger
    {
        event Action OnAnimationEnd;
        event Action OnDamageCast;
        event Action OnFootStep;
    }
}