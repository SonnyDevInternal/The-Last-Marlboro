using UnityEngine;

public enum EColliderEvent
{
    OnDestroyed,
    OnColliderEnter,
    OnColliderExit, 
    OnColliderStay,
}

public class ColliderHandler : MonoBehaviour
{
    private Collider owningCollider = null;

    private Collision currentCollision = null;

    public delegate void OnTriggerColliderEvent(ColliderHandler _this, EColliderEvent colliderEvent);

    private OnTriggerColliderEvent onTriggerColliderEvent;

    public void BindOnTriggerColliderEvent(OnTriggerColliderEvent onTriggerColliderEvent) { this.onTriggerColliderEvent += onTriggerColliderEvent; }
    public void UnbindOnTriggerColliderEvent(OnTriggerColliderEvent onTriggerColliderEvent) { this.onTriggerColliderEvent -= onTriggerColliderEvent; }

    private void Start()
    {
        owningCollider = GetComponent<Collider>();
    }

    private void TriggerColliderEvent(EColliderEvent Event)
    {
        onTriggerColliderEvent?.Invoke(this, Event);
    }

    private void OnDestroy()
    {
        TriggerColliderEvent(EColliderEvent.OnDestroyed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        currentCollision = collision;

        TriggerColliderEvent(EColliderEvent.OnColliderEnter);

        currentCollision = null;
    }

    private void OnCollisionExit(Collision collision)
    {
        currentCollision = collision;

        TriggerColliderEvent(EColliderEvent.OnColliderExit);

        currentCollision = null;
    }

    private void OnCollisionStay(Collision collision)
    {
        currentCollision = collision;

        TriggerColliderEvent(EColliderEvent.OnColliderStay);

        currentCollision = null;
    }

    public Collision GetCollidingObject()
    {
        return currentCollision;
    }

    public Collider GetOwningCollider()
    {
        return owningCollider;
    }
}
