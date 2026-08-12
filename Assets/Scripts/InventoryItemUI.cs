using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [Header("Visual References (From Catalog)")]
    public Image itemIcon;
    public Image brandIcon;
    public Image colorSquare;
    public TextMeshProUGUI itemNameText;

    [Header("Dynamic References (From Spool)")]
    public TextMeshProUGUI weightText;
    public Button itemButton;

    private SpoolInstance currentSpool;

    public void Setup(SpoolInstance spool)
    {
        currentSpool = spool;

        FilamentProfileSO profile = InventoryManager.Instance.GetProfileForSpool(spool);

        if (profile != null)
        {
            if (itemIcon != null) itemIcon.sprite = profile.icon;
            if (colorSquare != null) colorSquare.color = InventoryManager.Instance.GetSpoolColor(spool);

            if (itemNameText != null)
                itemNameText.text = InventoryManager.Instance.GetSpoolDisplayName(spool);

            if (profile.brand != null && brandIcon != null)
                brandIcon.sprite = profile.brand.brandLogo;
        }
        else
        {
            if (itemNameText != null) itemNameText.text = "Unknown/Deleted Spool";
        }

        if (weightText != null)
        {
            weightText.text = $"{Mathf.RoundToInt(spool.remainingWeightGrams)}g / {Mathf.RoundToInt(spool.originalWeightGrams)}g";
        }

        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(OnItemClicked);
        }
    }

    private void OnItemClicked()
    {
        InventoryUIManager uiManager = GetComponentInParent<InventoryUIManager>();

        if (uiManager != null)
        {
            uiManager.OpenSpoolDetails(currentSpool);
        }
    }
}