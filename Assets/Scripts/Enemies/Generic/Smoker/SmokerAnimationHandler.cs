using UnityEngine;

public enum ESmokerAnimationEvent
{
    OnSmokeSpawn,
    OnSmokeEnd,
    OnDeath
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

    private void OnDeath()
    {
        CallEvent(ESmokerAnimationEvent.OnDeath);
    }
}
