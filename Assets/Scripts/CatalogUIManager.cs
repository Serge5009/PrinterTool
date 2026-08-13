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

    [Header("Custom Filament Controls")]
    public Button addCustomButton;

    private Action<CatalogItemSO> currentCallback;
    private Action<BrandSO> customAddCallback;
    private Action<BrandSO> standaloneBrandCallback;

    private bool isSelectingFilament;
    private BrandSO currentlySelectedBrand;

    private void Start()
    {
        if (skipBrandButton != null)
        {
            skipBrandButton.onClick = new Button.ButtonClickedEvent();
            skipBrandButton.onClick.AddListener(OnSkipBrandClicked);
        }
    }

    public void OpenFilamentCatalog(Action<CatalogItemSO> onItemSelected = null, Action<BrandSO> onCustomSelected = null)
    {
        currentCallback = onItemSelected;
        customAddCallback = onCustomSelected;
        standaloneBrandCallback = null;
        isSelectingFilament = true;

        if (catalogPanel != null)
        {
            catalogPanel.SetActive(true);
            catalogPanel.transform.SetAsLastSibling();
        }

        ShowBrandSelection();
    }

    public void OpenPrinterCatalog(Action<CatalogItemSO> onItemSelected = null, Action<BrandSO> onCustomSelected = null)
    {
        currentCallback = onItemSelected;
        customAddCallback = onCustomSelected;
        standaloneBrandCallback = null;
        isSelectingFilament = false;

        if (catalogPanel != null)
        {
            catalogPanel.SetActive(true);
            catalogPanel.transform.SetAsLastSibling();
        }

        ShowBrandSelection();
    }

    private void ShowBrandSelection()
    {
        ClearContainer();

        if (catalogTitleText != null) catalogTitleText.text = "Select a Brand";
        if (skipBrandButton != null) skipBrandButton.gameObject.SetActive(true);
        if (addCustomButton != null) addCustomButton.gameObject.SetActive(false);

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

        currentlySelectedBrand = selectedBrand;

        if (skipBrandButton != null) skipBrandButton.gameObject.SetActive(false);

        if (addCustomButton != null)
        {
            addCustomButton.gameObject.SetActive(customAddCallback != null);

            addCustomButton.onClick = new Button.ButtonClickedEvent();
            addCustomButton.onClick.AddListener(OnAddCustomClicked);
        }

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

    private void OnAddCustomClicked()
    {
        customAddCallback?.Invoke(currentlySelectedBrand);
    }

    public void OpenBrandSelector(Action<BrandSO> onBrandSelected)
    {
        standaloneBrandCallback = onBrandSelected;

        ClearContainer();
        if (catalogPanel != null)
        {
            catalogPanel.SetActive(true);
            catalogPanel.transform.SetAsLastSibling();
        }

        if (catalogTitleText != null) catalogTitleText.text = "Select Brand";
        if (skipBrandButton != null) skipBrandButton.gameObject.SetActive(false);
        if (addCustomButton != null) addCustomButton.gameObject.SetActive(false);

        if (database == null || database.allBrands == null) return;

        foreach (BrandSO brand in database.allBrands)
        {
            BrandItemUI newBrandUI = Instantiate(brandPrefab, contentPanel);
            newBrandUI.Setup(brand, OnStandaloneBrandClicked);
        }
    }

    private void OnStandaloneBrandClicked(BrandSO brand)
    {
        standaloneBrandCallback?.Invoke(brand);
        CloseCatalog();
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