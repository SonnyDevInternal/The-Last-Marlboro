using System;
using UnityEngine;

public abstract class AnimationHandler<T> : MonoBehaviour where T : Enum
{
    public delegate void OnAnimationHandlerCalled(AnimationHandler<T> _this, T value);

    protected OnAnimationHandlerCalled onAnimationHandlerCalled;

    public void BindOnAnimationHandlerCalled(OnAnimationHandlerCalled handler) { onAnimationHandlerCalled += handler; }
    public void UnbindOnAnimationHandlerCalled(OnAnimationHandlerCalled handler) { onAnimationHandlerCalled -= handler; }
}