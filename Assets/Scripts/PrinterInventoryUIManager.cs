using UnityEngine;
using TMPro;

public class PrinterInventoryUIManager : MonoBehaviour
{
    [Header("UI References")]
    public PrinterItemUI printerPrefab;
    public Transform contentPanel;

    [Header("Manager Links")]
    public CatalogUIManager catalogManager;
    public PrinterDetailsUIManager detailsManager;

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
            RefreshUI();
        }

        if (AppManager.Instance != null)
        {
            AppManager.Instance.OnPrinterChanged += _ => RefreshUI();
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null) InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
        if (AppManager.Instance != null) AppManager.Instance.OnPrinterChanged -= _ => RefreshUI();
    }

    private void RefreshUI()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        if (InventoryManager.Instance == null || InventoryManager.Instance.ActiveData == null) return;

        foreach (PrinterInstance printer in InventoryManager.Instance.ActiveData.ownedPrinters)
        {
            PrinterItemUI newPrinterUI = Instantiate(printerPrefab, contentPanel);
            newPrinterUI.Setup(printer);
        }
    }

    public void OnAddButtonClicked()
    {
        if (catalogManager != null)
        {
            catalogManager.OpenPrinterCatalog(OnCatalogItemSelected, OnCustomPrinterRequested);
        }
    }

    private void OnCustomPrinterRequested(BrandSO preselectedBrand)
    {
        catalogManager.CloseCatalog();

        if (detailsManager != null)
        {
            detailsManager.OpenForCustomAdd(preselectedBrand);
        }
    }

    private void OnCatalogItemSelected(CatalogItemSO selectedItem)
    {
        if (selectedItem is PrinterProfileSO profile)
        {
            catalogManager.CloseCatalog();

            if (detailsManager != null)
                detailsManager.OpenForAdd(profile);
            else
                InventoryManager.Instance.AddPrinter(profile);
        }
    }

    public void OpenPrinterDetails(PrinterInstance printer)
    {
        if (detailsManager != null)
        {
            detailsManager.OpenForEdit(printer);
        }
    }
}