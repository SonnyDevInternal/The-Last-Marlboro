using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct UI_CrosshairSettings
{
    public Color crosshairColor;

    public float crosshairSize;

    public float interpolationTime_Color;
    public float interpolationTime_Size;

    public bool interpolateColor;
    public bool interpolateSize;
}

public class UI_Crosshair : MonoBehaviour
{
    [SerializeField]
    private Image verticalImage = null;

    [SerializeField]
    private Image horizontalImage = null;

    [SerializeField]
    private UI_CrosshairSettings crosshairSettings = new UI_CrosshairSettings();

    private Color currentCrosshairColor = Color.white;
    private float currentCrosshairSize = 1f;

    private float interpolationTime_ColorCurrent = 0f;
    private float interpolationTime_SizeCurrent = 0f;

    private bool isInterpolatingColor = false;
    private bool isInterpolatingSize = false;

    private void Start()
    {
        OnSetCrosshairColor(crosshairSettings.crosshairColor);
        OnSetCrosshairSize(crosshairSettings.crosshairSize);
    }

    private void Update()
    {
        if(isInterpolatingColor)
        {
            if(interpolationTime_ColorCurrent >= crosshairSettings.interpolationTime_Color)
            {
                isInterpolatingColor = false;
                interpolationTime_ColorCurrent = 0.0f;

                currentCrosshairColor = crosshairSettings.crosshairColor;

                OnSetCrosshairColor(crosshairSettings.crosshairColor);
            }
            else
            {
                float t = Mathf.Clamp01(interpolationTime_ColorCurrent / crosshairSettings.interpolationTime_Color);

                OnSetCrosshairColor(Color.Lerp(currentCrosshairColor, crosshairSettings.crosshairColor, t));

                interpolationTime_ColorCurrent += Time.deltaTime;
            }
        }

        if(isInterpolatingSize)
        {
            if(interpolationTime_SizeCurrent >= crosshairSettings.interpolationTime_Size)
            {
                isInterpolatingSize = false;
                interpolationTime_SizeCurrent = 0.0f;

                currentCrosshairSize = crosshairSettings.crosshairSize;

                OnSetCrosshairSize(currentCrosshairSize);
            }
            else
            {
                float t = Mathf.Clamp01(interpolationTime_SizeCurrent / crosshairSettings.interpolationTime_Size);

                OnSetCrosshairSize(Mathf.Lerp(currentCrosshairSize, crosshairSettings.crosshairSize, t));

                interpolationTime_SizeCurrent += Time.deltaTime;
            }
        }
    }

    private void OnSetCrosshairSize(float crosshairSize)
    {
        this.transform.localScale = Vector3.one * crosshairSize;
    }

    private void OnSetCrosshairColor(Color crosshairColor)
    {
        this.verticalImage.color = crosshairColor;
        this.horizontalImage.color = crosshairColor;
    }

    public void SetCrosshairSize(float crosshairSize)
    {
        this.crosshairSettings.crosshairSize = crosshairSize;

        if(this.crosshairSettings.interpolateSize)
            this.isInterpolatingSize = true;
        else
            OnSetCrosshairSize(crosshairSize);
    }

    public void SetCrosshairColor(Color crosshairColor)
    {
        this.crosshairSettings.crosshairColor = crosshairColor;

        if(this.crosshairSettings.interpolateColor)
            this.isInterpolatingColor = true;
        else
            OnSetCrosshairColor(crosshairColor);
    }

    public void SetInterpolationTimeColor(float interpolationTimeColor)
    {
        this.crosshairSettings.interpolationTime_Color = interpolationTimeColor;
    }

    public void SetInterpolationTimeSize(float interpolationTimeSize)
    {
        this.crosshairSettings.interpolationTime_Size = interpolationTimeSize;
    }
}
