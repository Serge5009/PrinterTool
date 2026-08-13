using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

public class MaterialSelectorUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public MaterialToggleItemUI togglePrefab;
    public Transform contentPanel;

    [Header("Action Buttons")]
    public Button confirmButton;
    public Button closeButton;

    private List<MaterialFamilySO> activeSelection = new List<MaterialFamilySO>();
    private Action<List<MaterialFamilySO>> onConfirmCallback;

    private void Start()
    {
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    public void Open(List<MaterialFamilySO> currentSelection, Action<List<MaterialFamilySO>> onConfirm)
    {
        activeSelection = new List<MaterialFamilySO>(currentSelection ?? new List<MaterialFamilySO>());
        onConfirmCallback = onConfirm;

        PopulateList();

        if (panel != null)
        {
            panel.SetActive(true);
            panel.transform.SetAsLastSibling();
        }
    }

    private void PopulateList()
    {
        foreach (Transform child in contentPanel) Destroy(child.gameObject);

        if (AppManager.Instance == null || AppManager.Instance.masterDatabase == null) return;

        var allFamilies = AppManager.Instance.masterDatabase.allMaterialFamilies;

        var sortedFamilies = allFamilies
            .OrderByDescending(f => activeSelection.Contains(f))
            .ThenBy(f => f.familyAbbreviation)
            .ToList();

        foreach (var family in sortedFamilies)
        {
            MaterialToggleItemUI newToggle = Instantiate(togglePrefab, contentPanel);
            newToggle.Setup(family, activeSelection.Contains(family), OnFamilyToggled);
        }
    }

    private void OnFamilyToggled(MaterialFamilySO family, bool isSelected)
    {
        if (isSelected && !activeSelection.Contains(family))
        {
            activeSelection.Add(family);
        }
        else if (!isSelected && activeSelection.Contains(family))
        {
            activeSelection.Remove(family);
        }
    }

    private void OnConfirmClicked()
    {
        onConfirmCallback?.Invoke(activeSelection);
        Close();
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
    }
}