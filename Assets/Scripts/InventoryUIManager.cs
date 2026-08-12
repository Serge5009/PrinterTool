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
        Debug.Log("[Inventory UI] Add button clicked! Time to open the Master Catalog.");
    }
}