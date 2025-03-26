using UnityEngine;

public enum EItemEvent
{
    OnDestroyed,
    OnUsed,
    OnDropped,
    OnPickedUp,
    OnActivated,
    OnDeactivated
}

[System.Serializable]
public struct ItemSettings
{
    public bool toggleCollidersOnPickup;
    public bool toggleRigidBodyOnPickup;

    [Tooltip("Should be by Default Active!")]
    public bool toggleMeshRenderersOnPickup;

    [Tooltip("If the Items States should be Updated on Start")]
    public bool setStateOnStart;
}

public abstract class Item : Interactable
{
    static protected int interactableLayer = 0;
    static protected int defaultLayer = 0;
    static protected int hitboxLayer = 0;
    static protected int IgnoreRaycastLayer = 0;

    static protected bool hasFoundAllLayerMasks = false;

    [SerializeField, Tooltip("The Name of this Item. (Will be used for UI Later!)")]
    private string itemName = "";

    [SerializeField, Tooltip("The Mesh Renderers that belong to this Item. (Important for Toggeling Visibility!)")]
    private MeshRenderer[] meshRenderers = null;

    [SerializeField, Tooltip("Insert all Colliders that you want to toggle on Pickup. ('toggleCollidersOnPickup' has to be turned on!)")]
    private Collider[] owningColliders = null;

    [SerializeField, Tooltip("The Owning RigidBody that belongs to this Item!. ('toggleRigidBodyOnPickup' has to be turned on!)")]
    private Rigidbody owningRigidBody = null;

    private Player owningPlayer = null;

    [SerializeField]
    private ItemSettings itemSettings = new ItemSettings();

    public delegate void OnItemEvent(Item _this, EItemEvent Event);

    private OnItemEvent onItemEvent = delegate { };

    private bool hasItemMeshRenderers = false;
    private bool hasItemColliders = false;
    private bool hasRigidBody = false;

    private bool hasBeenPickedUp = false;

    protected bool isActive = false;

    public void BindOnItemEvent(OnItemEvent onItemEvent) { this.onItemEvent += onItemEvent; }

    public void UnbindOnItemEvent(OnItemEvent onItemEvent) { this.onItemEvent -= onItemEvent; }

    private void Start()
    {
        if(!hasFoundAllLayerMasks)
        {
            hasFoundAllLayerMasks = true;

            interactableLayer = LayerMask.NameToLayer("Interactable");
            defaultLayer = LayerMask.NameToLayer("Default");
            hitboxLayer = LayerMask.NameToLayer("Hitbox");
            IgnoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        }

        hasItemMeshRenderers = (meshRenderers != null && meshRenderers.Length > 0);
        hasItemColliders = (owningColliders != null && owningColliders.Length > 0);
        hasRigidBody = (owningRigidBody != null);

        if(itemSettings.setStateOnStart)
            UpdateItemValues(true);

        OnItemStart();
    }

    private void OnDestroy()
    {
        CallItemEvent(EItemEvent.OnDestroyed);

        OnItemDestroyed();
    }

    public override bool CanInteract(Vector3 playerPos)
    {
        return base.CanInteract(playerPos) && !hasBeenPickedUp;
    }

    protected override void OnInteract(Player player)
    {
        var inventory = player.GetInventory();

        inventory.AddItem(this);
    }

    public virtual bool CanUseItem()
    {
        return hasBeenPickedUp && isActive;
    }

    protected virtual void OnPickUpItem_Implementation(GameObject storingLocation)
    {

    }

    protected virtual void OnDropItem_Implementation(Vector3 dropPosition)
    {

    }
    protected virtual void OnUseItem()
    {

    }

    protected virtual void OnItemStart()
    {

    }
    protected virtual void OnItemDestroyed()
    {

    }

    protected virtual void OnItemChangedParent(Transform newParent, bool worldPosStays)
    {
        transform.SetParent(newParent, worldPosStays);
    }

    protected void SetCollidersActive(bool active)
    {
        if (!hasItemColliders)
            return;

        for (int i = 0; i < owningColliders.Length; i++)
        {
            owningColliders[i].enabled = active;
        }
    }

    protected void SetMeshRenderersActive(bool active)
    {
        if (!hasItemMeshRenderers)
            return;

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].enabled = active;
        }
    }

    protected void SetRigidBodyActive(bool active)
    {
        if (!hasRigidBody)
            return;

        owningRigidBody.isKinematic = !active;
    }

    protected void CallItemEvent(EItemEvent Event)
    {
        this.onItemEvent.Invoke(this, Event);
    }

    private void UpdateItemValues(bool itemActive)
    {
        this.isActive = itemActive;

        if (hasItemColliders && itemSettings.toggleCollidersOnPickup)
            SetCollidersActive(itemActive);

        if (hasItemMeshRenderers && itemSettings.toggleMeshRenderersOnPickup)
            SetMeshRenderersActive(itemActive);

        if (hasRigidBody && itemSettings.toggleRigidBodyOnPickup)
            SetRigidBodyActive(itemActive);
    }

    private void SwitchLayerMask(bool interactable)
    {
        if (interactable)
            gameObject.layer = interactableLayer;
        else
            gameObject.layer = defaultLayer;
    }

    private void OnPlayerDestroyed(Player _this, bool byScene)
    {
        DropItem();
    }

    private void BindPlayer(Player player)
    {
        this.owningPlayer = player;

        player.BindOnDestroy(OnPlayerDestroyed);
    }

    private void UnbindPlayer()
    {
        if(this.owningPlayer != null)
        {
            this.owningPlayer.UnbindOnDestroy(OnPlayerDestroyed);

            this.owningPlayer = null;
        }
    }

    private void OnPickUpItem(GameObject storingLocation)
    {
        UpdateItemValues(true);

        this.hasBeenPickedUp = true;

        SwitchLayerMask(false);

        var storingTransform = storingLocation.transform;

        OnItemChangedParent(storingTransform, false);

        OnPickUpItem_Implementation(storingLocation);

        CallItemEvent(EItemEvent.OnPickedUp);
    }
    private void OnDropItem(Vector3 dropPosition)
    {
        UpdateItemValues(false);

        this.hasBeenPickedUp = false;

        SwitchLayerMask(true);

        OnItemChangedParent(null, true);

        transform.position = dropPosition;

        OnDropItem_Implementation(dropPosition);

        CallItemEvent(EItemEvent.OnDropped);
    }

    public void UseItem()
    {
        if (!CanUseItem())
            return;

        OnUseItem();

        CallItemEvent(EItemEvent.OnUsed);
    }

    public void PickUpItem(Player player)
    {
        BindPlayer(player);

        OnPickUpItem(player.GetInteractionHolder());
    }

    public void DropItem()
    {
        UnbindPlayer();

        OnDropItem(transform.position);
    }

    public void ActivateItem(bool active)
    {
        UpdateItemValues(active);
    }

    public void ToggleItem()
    {
        UpdateItemValues(!isActive);
    }

    public bool IsItemActive()
    {
        return isActive;
    }

    public string GetItemName()
    {
        return itemName;
    }

    public Player GetOwningPlayer()
    {
        return owningPlayer;
    }

    static public int GetInteractableLayer()
    {
        return interactableLayer;
    }

    static public int GetHitboxLayer()
    {
        return hitboxLayer;
    }
}