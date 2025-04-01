using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveInstance : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI preText = null;

    [SerializeField]
    private Button loadBtn = null;

    private int saveIndex = 0;

    public delegate void OnLoadSave(int saveIndex);

    private OnLoadSave onLoadSave = delegate { };

    private void Start()
    {
        loadBtn.onClick.AddListener(OnPressLoad);
    }

    private void OnDestroy()
    {
        loadBtn.onClick.RemoveAllListeners();
    }

    private void OnPressLoad()
    {
        onLoadSave(saveIndex);
    }

    public void SetSaveIndex(int saveIndex)
    {
        this.saveIndex = saveIndex;

        preText.text = $"Save: {saveIndex}";
    }
}