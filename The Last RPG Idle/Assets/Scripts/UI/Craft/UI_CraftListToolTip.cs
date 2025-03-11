using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_CraftListToolTip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI craftName;
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
    public void ShowCraftListToolTip(string _craftName)
    {
        craftName.text = _craftName;
        gameObject.SetActive(true);
    }

    public void HideCraftListTooltip() => gameObject.SetActive(false);

}
