using UnityEngine;

public enum EFreaksterAnimationEvent
{
    SlashEnd,
}

public class FreaksterTongueAnimationHandler : AnimationHandler<EFreaksterAnimationEvent>
{
    private void OnAnimationSlashEnd()
    {
        this.onAnimationHandlerCalled(EFreaksterAnimationEvent.SlashEnd);
    }
}
