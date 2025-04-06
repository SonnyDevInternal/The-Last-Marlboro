using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
internal struct ItemLoaderListIndex
{
    [SerializeField]
    private GameObject prefabItem;

    [SerializeField]
    private EItemID owningID;

    public EItemID GetItemID()
    {
        return owningID;
    }

    public GameObject GetPrefab()
    {
        return prefabItem;
    }
}

[System.Serializable]
internal struct ItemLoaderList
{
    public ItemLoaderListIndex[] itemArray;
}

public class ItemLoader : MonoBehaviour
{
    [SerializeField]
    private ItemLoaderList itemLoaderList;

    private Dictionary<EItemID, GameObject> itemList = new Dictionary<EItemID, GameObject>();

    private bool wasInitialized = false;

    private void Start()
    {
        Intialize();
    }

    private void Intialize()
    {
        if (wasInitialized)
            return;

        wasInitialized = true;

        var array = itemLoaderList.itemArray;

        if (array != null)
        {
            for (int i = 0; i < array.Length; i++)
            {
                itemList.Add(array[i].GetItemID(), array[i].GetPrefab());
            }
        }
    }

    private GameObject GetItemPrefab(EItemID itemID)
    {
        if (!wasInitialized)
            Intialize();

        if (itemList.TryGetValue(itemID, out GameObject prefab))
        {
            return prefab;
        }

        return null;
    }

    public GameObject LoadItem(EItemID itemID, Vector3 position, Quaternion rotation)
    {
        if(!wasInitialized)
            Intialize();

        var prefab = GetItemPrefab(itemID);

        if (prefab != null)
        {
            var instantiatedItem = Instantiate(prefab, position, rotation);

            if (instantiatedItem != null)
                return instantiatedItem;
        }

        return null;
    }
}
