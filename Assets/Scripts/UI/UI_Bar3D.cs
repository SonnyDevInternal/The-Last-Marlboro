using UnityEngine;
using UnityEngine.UI;

public class UI_Bar3D : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer curBar = null;
    [SerializeField]
    private SpriteRenderer maxBar = null;

    private float maxDrawPercentage = 1.0f;
    private float curPercentage = 0;

    private bool canOverflow = false;

    private bool manipulateX = true;
    private bool manipulateY = false;

    void Awake()
    {
        if (curBar == null)
        curBar = GetComponent<SpriteRenderer>();

        if (maxBar == null)
            maxBar = transform.parent.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        
    }

    public void SetPercentage(float percentage) // 0 - 1
    {
        curPercentage = percentage;

        RecalculateBar();
    }

    private void RecalculateBar()
    {
        if (!curBar || !maxBar)
            return;

        if (!canOverflow)
        {
            curPercentage = Mathf.Clamp(curPercentage, 0.0f, maxDrawPercentage);
        }

        float currentPercentage = curPercentage / maxDrawPercentage;

        Vector3 maxSize = Vector3.one;
        Vector3 newSize = curBar.transform.localScale;

        if (manipulateX)
        {
            newSize.x = maxSize.x * currentPercentage;
        }

        if (manipulateY)
        {
            newSize.y = maxSize.y * currentPercentage;
        }

        curBar.transform.localScale = newSize;

        Vector3 newPosition = curBar.transform.localPosition;

        /*
        if (manipulateX)
            newPosition.x = -(maxSize.x / 2) + (newSize.x / 2);

        if (manipulateY)
            newPosition.y = -(maxSize.y / 2) + (newSize.y / 2);

        */

        curBar.transform.localPosition = newPosition;
    }


}
