using UnityEngine;

public enum EFreaksterAnimationEvent
{
    Death
}

public class FreaksterAnimationHandler : AnimationHandler<EFreaksterAnimationEvent>
{
    private void OnDeath()
    {
        CallEvent(EFreaksterAnimationEvent.Death);
    }
}
