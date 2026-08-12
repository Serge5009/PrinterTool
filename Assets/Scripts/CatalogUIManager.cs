using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class CatalogUIManager : MonoBehaviour
{
    [Header("Data References")]
    [Tooltip("The master database we created earlier.")]
    public CatalogDatabaseSO database;

    [Header("UI References")]
    [Tooltip("The prefab with the CatalogItemUI script attached.")]
    public CatalogItemUI itemPrefab;

    [Tooltip("The Content transform inside your Scroll View.")]
    public Transform contentPanel;

    [Tooltip("The parent GameObject of the Catalog UI so it can hide/show itself.")]
    public GameObject catalogPanel;

    [Header("Step Controls")]
    [Tooltip("The prefab with the BrandItemUI script attached.")]
    public BrandItemUI brandPrefab;

    [Tooltip("The button used to skip brand filtering and view all items.")]
    public Button skipBrandButton;

    [Tooltip("Text to display 'Select Brand' or 'Catalog'.")]
    public TextMeshProUGUI catalogTitleText;

    private Action<CatalogItemSO> currentCallback;
    private bool isSelectingFilament;

    private void Start()
    {
        if (skipBrandButton != null)
        {
            skipBrandButton.onClick.AddListener(OnSkipBrandClicked);
        }
    }

    public void OpenFilamentCatalog(Action<CatalogItemSO> onItemSelected = null)
    {
        currentCallback = onItemSelected;
        isSelectingFilament = true;

        if (catalogPanel != null) catalogPanel.SetActive(true);

        ShowBrandSelection();
    }

    public void OpenPrinterCatalog(Action<CatalogItemSO> onItemSelected = null)
    {
        currentCallback = onItemSelected;
        isSelectingFilament = false;

        if (catalogPanel != null) catalogPanel.SetActive(true);

        ShowBrandSelection();
    }

    private void ShowBrandSelection()
    {
        ClearContainer();

        if (catalogTitleText != null) catalogTitleText.text = "Select a Brand";
        if (skipBrandButton != null) skipBrandButton.gameObject.SetActive(true);

        if (database == null || database.allBrands == null) return;

        foreach (BrandSO brand in database.allBrands)
        {
            BrandItemUI newBrandUI = Instantiate(brandPrefab, contentPanel);

            newBrandUI.Setup(brand, ShowItemsForBrand);
        }
    }

    private void OnSkipBrandClicked()
    {
        ShowItemsForBrand(null);
    }

    private void ShowItemsForBrand(BrandSO selectedBrand)
    {
        ClearContainer();

        if (skipBrandButton != null) skipBrandButton.gameObject.SetActive(false);

        if (catalogTitleText != null)
        {
            catalogTitleText.text = selectedBrand != null ? $"{selectedBrand.brandName} Catalog" : "All Items";
        }

        if (isSelectingFilament)
        {
            var filaments = selectedBrand != null
                ? database.allFilaments.Where(f => f.brand == selectedBrand)
                : database.allFilaments;

            foreach (FilamentProfileSO filament in filaments)
            {
                SpawnItem(filament, currentCallback);
            }
        }
        else
        {
            var printers = selectedBrand != null
                ? database.allPrinters.Where(p => p.brand == selectedBrand)
                : database.allPrinters;

            foreach (PrinterProfileSO printer in printers)
            {
                SpawnItem(printer, currentCallback);
            }
        }
    }

    public void CloseCatalog()
    {
        if (catalogPanel != null) catalogPanel.SetActive(false);
    }

    private void SpawnItem(CatalogItemSO item, Action<CatalogItemSO> onItemSelected)
    {
        CatalogItemUI newItemUI = Instantiate(itemPrefab, contentPanel);

        newItemUI.Setup(item, onItemSelected);
    }

    private void ClearContainer()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
    }
}