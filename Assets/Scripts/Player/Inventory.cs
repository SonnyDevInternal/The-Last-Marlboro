using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct InventorySettings
{
    public int MaxItemCount;

    public bool activateItemOnPickup;
    public bool shouldDropItemsOnDestroy;
}

public struct InventorySaveData
{
    public InventorySettings settings;
    public ItemSaveData[] items;
}

public class Inventory : MonoBehaviour
{
    private Player player = null;

    private List<Item> itemHolders = new List<Item>(4);

    [SerializeField]
    private InventorySettings inventorySettings;

    private Item activeItem = null;

    private bool hasActiveItem = false;
    private bool hasBeenSaved = false;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    private void OnDestroy()
    {
        if(inventorySettings.shouldDropItemsOnDestroy)
            DropAllItems();
    }

    private void OnItemEvent(Item item, EItemEvent Event)
    {
        switch (Event)
        {
            case EItemEvent.OnDestroyed:
                RemoveItem(item, false);
                break;
            case EItemEvent.OnDropped: 
                RemoveItem(item); 
                break;

            default:
                break;
        }
    }

    private void ActiveItemSetActive(bool active)
    {
        if (hasActiveItem)
        {
            activeItem.ActivateItem(active);
        }
    }

    private void OnActivateItem(Item item, bool active, bool flag_SkipActiveCheck = false)
    {
        if (!flag_SkipActiveCheck && hasActiveItem && activeItem == item)
            return;

#if DEBUG
        if(flag_SkipActiveCheck)
            Debug.Log("Skipped Active Check!");
#endif

        ActiveItemSetActive(false);

        hasActiveItem = true;

        activeItem = item;

        item.ActivateItem(active);
    }

    private void OnAddItem(Item item)
    {
        item.PickUpItem(player);

        item.BindOnItemEvent(OnItemEvent);

        itemHolders.Add(item);

        if (inventorySettings.activateItemOnPickup)
            ActivateItem(item, true);
    }

    private void DropAllItems()
    {
        for (int i = 0; i < itemHolders.Count; i++)
        {
            var item = itemHolders[i];

            if(item != null)
            {
                item.UnbindOnItemEvent(OnItemEvent);

                item.DropItem();
            }
        }

        itemHolders.Clear();
    }

    private void OnRemoveItem(Item item, bool dropItem)
    {
        if(hasActiveItem && item == activeItem)
        {
            hasActiveItem = false;
            activeItem = null;
        }    

        itemHolders.Remove(item);

        item.UnbindOnItemEvent(OnItemEvent);

        if(dropItem)
            item.DropItem();
    }

    private void OnToggleItem(Item item)
    {
        OnActivateItem(item, !item.IsItemActive(), true);
    }

    public bool IsFull()
    {
        return itemHolders.Count >= inventorySettings.MaxItemCount;
    }

    public void AddItem(Item item)
    {
        if (IsFull())
            return;

        OnAddItem(item);
    }

    public void RemoveItem(Item item, bool dropItem = true)
    {
        OnRemoveItem(item, dropItem);
    }

    public void ActivateItem(Item item, bool active)
    {
        OnActivateItem(item, active);
    }

    public void ToggleItemOnIndex(int index)
    {
        if (index >= 0 && index < itemHolders.Count)
        {
            var item = itemHolders[index];

            if(item != null)
            {
                OnToggleItem(item);
            }
        }
    }

    public InventorySaveData GetSaveData()
    {
        InventorySaveData saveData = new InventorySaveData();

        saveData.
    }

    public Item GetActiveItem()
    {
        return (hasActiveItem == true ? activeItem : null);
    }

    public bool HasActiveItem()
    {
        return hasActiveItem;
    }
}
