using TMPro;
using UnityEngine;

public class UI_MaterialToolTip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI materialName;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = false; // Ensure the tooltip doesn't block raycasts
    }

    public void ShowToolTip(string _materialName)
    {
        materialName.text = _materialName;
        gameObject.SetActive(true);
    }

    public void HideToolTip()
    {
        gameObject.SetActive(false);
    }
}
