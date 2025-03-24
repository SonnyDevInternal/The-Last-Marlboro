using UnityEngine;

public class TestGun : Item
{
    protected override void OnUseItem()
    {
        Debug.Log("Pow pow");
    }

    protected override void OnItemChangedParent(Transform newParent, bool worldPosStays)
    {
        if(!worldPosStays)
        {
            transform.parent.SetParent(newParent, false);
            transform.parent.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));

            transform.localRotation = Quaternion.Euler(-90.0f, 0.0f, -90.0f);
        }
    }

    protected override void OnPickUpItem_Implementation(GameObject storingLocation)
    {

    }
}
