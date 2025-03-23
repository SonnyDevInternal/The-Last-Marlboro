using UnityEngine;

[System.Serializable]
public struct ItemSettings
{
    public bool disableCollidersOnPickup;
}

public class Item : Interactable
{
    private string itemName = "";

    [SerializeField]
    private MeshRenderer[] meshRenderers = null;

    private Player owningPlayer = null;

    [SerializeField]
    private ItemSettings itemSettings = new ItemSettings();

    private bool hasItemMeshRenderers = false;
    private bool hasBeenPickedUp = false;

    private void Start()
    {
        hasItemMeshRenderers = (meshRenderers != null);
    }

    public override bool CanInteract(Vector3 playerPos)
    {
        return base.CanInteract(playerPos) && !hasBeenPickedUp;
    }
}
