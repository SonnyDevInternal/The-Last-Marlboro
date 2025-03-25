using UnityEngine;

public enum EFreaksterTongueAnimationEvent
{
    SlashEnd,
}

public class FreaksterTongueAnimationHandler : AnimationHandler<EFreaksterTongueAnimationEvent>
{
    private void OnAnimationSlashEnd()
    {
        CallEvent(EFreaksterTongueAnimationEvent.SlashEnd);
    }
}
