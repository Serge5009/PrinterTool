using UnityEngine;
using System.Collections.Generic;
using System;

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

    public void OpenFilamentCatalog(Action<CatalogItemSO> onItemSelected = null)
    {
        if (catalogPanel != null) catalogPanel.SetActive(true);
        ClearContainer();

        if (database == null || database.allFilaments == null) return;

        foreach (FilamentProfileSO filament in database.allFilaments)
        {
            SpawnItem(filament, onItemSelected);
        }
    }

    public void OpenPrinterCatalog(Action<CatalogItemSO> onItemSelected = null)
    {
        if (catalogPanel != null) catalogPanel.SetActive(true);
        ClearContainer();

        if (database == null || database.allPrinters == null) return;

        foreach (PrinterProfileSO printer in database.allPrinters)
        {
            SpawnItem(printer, onItemSelected);
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