using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    protected bool isInteractable = true;

    [SerializeField]
    protected float MinDistanceToInteract = 4.0f;

    public delegate void OnInteracted(Interactable _this, Player player);

    protected OnInteracted onInteracted;

    public void BindOnInteracted(OnInteracted onInteracted) { this.onInteracted += onInteracted; }
    public void UnbindOnInteracted(OnInteracted onInteracted) { this.onInteracted -= onInteracted; }

    public void Interact(Player player)
    {
        if (!CanInteract(player.transform.position))
            return;

        OnInteract();

        onInteracted?.Invoke(this, player);
    }

    protected virtual void OnInteract()
    {

    }

    public virtual bool CanInteract(Vector3 playerPos)
    {
        return Vector3.Distance(transform.position, playerPos) <= MinDistanceToInteract;
    }
}
