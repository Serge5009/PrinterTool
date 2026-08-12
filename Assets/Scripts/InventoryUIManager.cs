using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class InventoryUIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The prefab with the InventoryItemUI script attached.")]
    public InventoryItemUI spoolPrefab;

    [Tooltip("The Content transform inside your Scroll View.")]
    public Transform contentPanel;

    [Tooltip("Text to display the name of the current list (e.g., 'Owned Spools').")]
    public TextMeshProUGUI listTitleText;

    [Tooltip("Reference to the Catalog Manager so we can open it as a popup.")]
    public CatalogUIManager catalogManager;

    [Tooltip("Reference to the adaptive details page.")]
    public SpoolDetailsUIManager detailsManager;

    [Header("State")]
    public string activeListName = "Owned Spools";

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
            RefreshUI();
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
        }
    }

    public void SwitchList(string newListName)
    {
        activeListName = newListName;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (listTitleText != null) listTitleText.text = activeListName;

        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        if (InventoryManager.Instance == null || InventoryManager.Instance.ActiveData == null) return;

        InventoryList targetList = InventoryManager.Instance.ActiveData.allInventories
            .FirstOrDefault(l => l.listName == activeListName);

        if (targetList != null)
        {
            foreach (SpoolInstance spool in targetList.spools)
            {
                InventoryItemUI newSpoolUI = Instantiate(spoolPrefab, contentPanel);
                newSpoolUI.Setup(spool);
            }
        }
    }

    public void OnAddButtonClicked()
    {
        Debug.Log("[Inventory UI] Add button clicked! Opening the Master Catalog.");

        if (catalogManager != null)
        {
            catalogManager.OpenFilamentCatalog(OnCatalogItemSelected, OnCustomFilamentRequested);
        }
    }

    private void OnCustomFilamentRequested(BrandSO preselectedBrand)
    {
        catalogManager.CloseCatalog();

        if (detailsManager != null)
        {
            detailsManager.OpenForCustomAdd(activeListName, preselectedBrand);
        }
    }

    private void OnCatalogItemSelected(CatalogItemSO selectedItem)
    {
        if (selectedItem is FilamentProfileSO profile)
        {
            catalogManager.CloseCatalog();

            if (detailsManager != null)
            {
                detailsManager.OpenForAdd(profile, activeListName);
            }
            else
            {
                InventoryManager.Instance.AddSpoolToList(activeListName, profile);
            }
        }
    }

    public void OpenSpoolDetails(SpoolInstance spool)
    {
        if (detailsManager != null)
        {
            detailsManager.OpenForEdit(spool);
        }
    }
}