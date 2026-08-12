using UnityEngine;
using System.Collections.Generic;

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
    
    public void OpenFilamentCatalog()
    {
        ClearContainer();

        if (database == null || database.allFilaments == null) return;

        foreach (FilamentProfileSO filament in database.allFilaments)
        {
            SpawnItem(filament);
        }
    }

    public void OpenPrinterCatalog()
    {
        ClearContainer();

        if (database == null || database.allPrinters == null) return;

        foreach (PrinterProfileSO printer in database.allPrinters)
        {
            SpawnItem(printer);
        }
    }

    private void SpawnItem(CatalogItemSO item)
    {
        CatalogItemUI newItemUI = Instantiate(itemPrefab, contentPanel);

        newItemUI.Setup(item);
    }

    private void ClearContainer()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
    }
}