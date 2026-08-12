using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CatalogItemUI : MonoBehaviour
{
    [Header("Universal Elements")]
    public Button itemButton;
    public Image itemIcon;
    public Image brandIcon;
    public TextMeshProUGUI brandNameText;
    public TextMeshProUGUI itemNameText;

    [Header("Filament Specifics")]
    [Tooltip("The parent object holding the color square, to be hidden for printers.")]
    public GameObject colorSquareContainer;
    public Image colorSquare;

    private CatalogItemSO currentItem;
    private Action<CatalogItemSO> onClickCallback;

    public void Setup(CatalogItemSO item, Action<CatalogItemSO> onClick = null)
    {
        currentItem = item;
        onClickCallback = onClick;

        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(OnItemClicked);
        }

        if (itemIcon != null) itemIcon.sprite = item.icon;

        if (item.brand != null)
        {
            if (brandIcon != null) brandIcon.sprite = item.brand.brandLogo;
            if (brandNameText != null) brandNameText.text = item.brand.brandName;
        }

        if (item is FilamentProfileSO filament)
        {
            SetupFilamentUI(filament);
        }
        else if (item is PrinterProfileSO printer)
        {
            SetupPrinterUI(printer);
        }
    }

    private void SetupFilamentUI(FilamentProfileSO filament)
    {
        if (itemNameText != null)
        {
            itemNameText.text = filament.GetDisplayName();
        }

        if (colorSquareContainer != null) colorSquareContainer.SetActive(true);
        if (colorSquare != null) colorSquare.color = filament.displayColor;
    }

    private void SetupPrinterUI(PrinterProfileSO printer)
    {
        if (itemNameText != null)
        {
            itemNameText.text = printer.itemName;
        }

        if (colorSquareContainer != null) colorSquareContainer.SetActive(false);
    }

    private void OnItemClicked()
    {
        onClickCallback?.Invoke(currentItem);
    }
}