using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    protected float MinDistanceToInteract = 4.0f;

    [SerializeField]
    protected string InteractableName = "Interact";

    [SerializeField]
    protected bool isInteractable = true;

    public delegate void OnInteracted(Interactable _this, Player player);

    protected OnInteracted onInteracted;

    public void BindOnInteracted(OnInteracted onInteracted) { this.onInteracted += onInteracted; }
    public void UnbindOnInteracted(OnInteracted onInteracted) { this.onInteracted -= onInteracted; }

    protected virtual void OnInteract(Player player)
    {

    }

    public void Interact(Player player)
    {
        if (!enabled || !CanInteract(player.transform.position))
            return;

        OnInteract(player);

        onInteracted?.Invoke(this, player);
    }

    public void SetIsInteractable(bool value)
    {
        isInteractable = value;
    }

    public bool GetIsInteractable()
    {
        return isInteractable;
    }

    public virtual bool CanInteract(Vector3 playerPos)
    {
        return Vector3.Distance(transform.position, playerPos) <= MinDistanceToInteract;
    }

    public virtual string GetInteractableName()
    {
        return "Press E To " + InteractableName;
    }
}
