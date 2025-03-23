using UnityEngine;

public enum ESmokerAnimationEvent
{
    OnSmokeSpawn,
    OnSmokeEnd
}

public class SmokerAnimationHandler : AnimationHandler<ESmokerAnimationEvent>
{
    private void OnSmokeSpawn()
    {
        CallEvent(ESmokerAnimationEvent.OnSmokeSpawn);
    }

    private void OnSmokingEnded()
    {
        CallEvent(ESmokerAnimationEvent.OnSmokeEnd);
    }
}
