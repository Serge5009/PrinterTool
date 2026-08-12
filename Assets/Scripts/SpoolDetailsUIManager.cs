using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SpoolDetailsUIManager : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject detailsPanel;

    [Header("Header Visuals")]
    public Image brandIcon;
    public Image colorSquare;
    public TextMeshProUGUI itemNameText;

    [Header("Editable Fields")]
    public TMP_InputField remainingWeightInput;
    public TMP_InputField originalWeightInput;
    public TMP_InputField notesInput;

    [Header("Action Buttons")]
    public Button saveButton;
    public Button deleteButton;
    public Button closeButton;
    public TextMeshProUGUI saveButtonText;

    private SpoolInstance activeSpool;
    private FilamentProfileSO activeProfile;

    private bool isEditingMode;
    private string pendingListName;

    private void Start()
    {
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
        if (deleteButton != null) deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    public void OpenForAdd(FilamentProfileSO profile, string targetList)
    {
        isEditingMode = false;
        activeProfile = profile;
        pendingListName = targetList;
        activeSpool = null;

        PopulateHeader(profile);

        if (remainingWeightInput != null) remainingWeightInput.text = "1000";
        if (originalWeightInput != null) originalWeightInput.text = "1000";
        if (notesInput != null) notesInput.text = "";

        if (saveButtonText != null) saveButtonText.text = "Add to Inventory";
        if (deleteButton != null) deleteButton.gameObject.SetActive(false);

        detailsPanel.SetActive(true);
    }

    public void OpenForEdit(SpoolInstance spool)
    {
        isEditingMode = true;
        activeSpool = spool;
        activeProfile = InventoryManager.Instance.GetProfileForSpool(spool);

        PopulateHeader(activeProfile);

        if (remainingWeightInput != null) remainingWeightInput.text = spool.remainingWeightGrams.ToString();
        if (originalWeightInput != null) originalWeightInput.text = spool.originalWeightGrams.ToString();
        if (notesInput != null) notesInput.text = spool.customNotes;

        if (saveButtonText != null) saveButtonText.text = "Save Changes";
        if (deleteButton != null) deleteButton.gameObject.SetActive(true);

        detailsPanel.SetActive(true);
    }

    private void PopulateHeader(FilamentProfileSO profile)
    {
        if (profile == null) return;

        if (brandIcon != null && profile.brand != null)
            brandIcon.sprite = profile.brand.brandLogo;

        if (colorSquare != null)
            colorSquare.color = profile.displayColor;

        string family = profile.materialFamily != null ? profile.materialFamily.familyAbbreviation : "";
        string style = profile.visualStyle != null ? profile.visualStyle.styleName : "";

        if (itemNameText != null)
            itemNameText.text = $"{profile.itemName} {family} ({style})";
    }

    private void OnSaveClicked()
    {
        float remaining = 1000f;
        float original = 1000f;

        if (remainingWeightInput != null) float.TryParse(remainingWeightInput.text, out remaining);
        if (originalWeightInput != null) float.TryParse(originalWeightInput.text, out original);

        if (isEditingMode && activeSpool != null)
        {
            activeSpool.remainingWeightGrams = remaining;
            activeSpool.originalWeightGrams = original;
            if (notesInput != null) activeSpool.customNotes = notesInput.text;

            InventoryManager.Instance.SaveData();
        }
        else if (!isEditingMode && activeProfile != null)
        {
            InventoryManager.Instance.AddSpoolToList(pendingListName, activeProfile, original);

        }

        ClosePanel();
    }

    private void OnDeleteClicked()
    {
        if (isEditingMode && activeSpool != null)
        {
            foreach (var list in InventoryManager.Instance.ActiveData.allInventories)
            {
                if (list.spools.Contains(activeSpool))
                {
                    list.spools.Remove(activeSpool);
                    break;
                }
            }

            InventoryManager.Instance.SaveData();
        }

        ClosePanel();
    }

    public void ClosePanel()
    {
        if (detailsPanel != null) detailsPanel.SetActive(false);
    }
}