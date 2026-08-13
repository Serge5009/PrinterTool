using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrinterItemUI : MonoBehaviour
{
    [Header("Visual References (From Catalog)")]
    public Image itemIcon;
    public Image brandIcon;
    public TextMeshProUGUI printerNameText;

    [Header("Dynamic References (From Instance)")]
    public TextMeshProUGUI statusText;
    public Button itemButton;

    private PrinterInstance currentPrinter;

    public void Setup(PrinterInstance printer)
    {
        currentPrinter = printer;

        PrinterProfileSO profile = InventoryManager.Instance.GetProfileForPrinter(printer);

        if (profile != null)
        {
            if (itemIcon != null) itemIcon.sprite = profile.icon;

            if (printerNameText != null)
                printerNameText.text = InventoryManager.Instance.GetPrinterDisplayName(printer);

            if (profile.brand != null && brandIcon != null)
                brandIcon.sprite = profile.brand.brandLogo;

            if (statusText != null)
            {
                if (AppManager.Instance.ActivePrinter == profile)
                    statusText.text = "<color=#00FF00>Active Printer</color>";
                else
                    statusText.text = $"{profile.buildVolume.x}x{profile.buildVolume.y}x{profile.buildVolume.z} mm";
            }
        }
        else
        {
            if (printerNameText != null) printerNameText.text = "Unknown/Deleted Printer";
        }

        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(OnItemClicked);
        }
    }

    private void OnItemClicked()
    {
        PrinterInventoryUIManager uiManager = GetComponentInParent<PrinterInventoryUIManager>();

        if (uiManager != null)
        {
            uiManager.OpenPrinterDetails(currentPrinter);
        }
    }
}