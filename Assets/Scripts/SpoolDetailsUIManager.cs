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

    [Header("All Stats & Overrides")]
    public TMP_InputField nicknameInput;
    public TMP_InputField colorNameInput;
    public TMP_Dropdown materialFamilyDropdown;
    public TMP_Dropdown styleDropdown;
    public TMP_InputField customNozzleInput;
    public TMP_InputField customBedInput;
    public TMP_InputField densityInput;

    [Header("Custom Color Controls")]
    public ColorPickerUIManager colorPicker;
    public Button customColorButton;
    public Image customColorPreview;
    private Color activeCustomColor = Color.white;
    private string activeCustomHex = "#FFFFFF";

    [Header("Custom Brand Controls")]
    public CatalogUIManager catalogManager;
    public Button customBrandButton;
    public Image customBrandIcon;
    public TextMeshProUGUI customBrandText;

    [Header("Action Buttons")]
    public Button saveButton;
    public Button deleteButton;
    public Button closeButton;
    public TextMeshProUGUI saveButtonText;

    private SpoolInstance activeSpool;
    private FilamentProfileSO activeProfile;

    private int currentMode;
    private string pendingListName;
    private BrandSO activeCustomBrand;
    private System.Collections.Generic.List<MaterialFamilySO> availableFamilies;
    private System.Collections.Generic.List<FilamentStyleSO> availableStyles;

    private bool isUpdatingUI = false;
    private bool isColorNameManuallyEdited = false;

    private void Start()
    {
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
        if (deleteButton != null) deleteButton.onClick.AddListener(OnDeleteClicked);

        if (customBrandButton != null) customBrandButton.onClick.AddListener(OnCustomBrandButtonClicked);
        if (customColorButton != null) customColorButton.onClick.AddListener(OnCustomColorButtonClicked);

        if (colorNameInput != null) colorNameInput.onValueChanged.AddListener(OnColorNameManuallyChanged);
        if (materialFamilyDropdown != null) materialFamilyDropdown.onValueChanged.AddListener(OnFamilyDropdownChanged);
    }

    private void OnFamilyDropdownChanged(int index)
    {
        if (isUpdatingUI || availableFamilies == null || availableFamilies.Count <= index) return;

        if (currentMode == 2)
        {
            var fam = availableFamilies[index];
            if (customNozzleInput != null) customNozzleInput.text = fam.defaultNozzleTemperature.ToString();
            if (customBedInput != null) customBedInput.text = fam.defaultBedTemperature.ToString();
            if (densityInput != null) densityInput.text = fam.baseDensity.ToString();
        }
    }

    private void OnColorNameManuallyChanged(string val)
    {
        if (!isUpdatingUI) isColorNameManuallyEdited = true;
    }

    public void OpenForAdd(FilamentProfileSO profile, string targetList)
    {
        currentMode = 1;
        activeProfile = profile;
        pendingListName = targetList;
        activeSpool = null;

        PopulateFamilyDropdown();
        PopulateStyleDropdown();
        PopulateHeader(profile);
        PopulateFieldsFromProfile();

        if (saveButtonText != null) saveButtonText.text = "Add to Inventory";
        if (deleteButton != null) deleteButton.gameObject.SetActive(false);

        detailsPanel.SetActive(true);
        detailsPanel.transform.SetAsLastSibling();
    }

    public void OpenForCustomAdd(string targetList, BrandSO preselectedBrand = null)
    {
        currentMode = 2;
        pendingListName = targetList;
        activeSpool = null;
        activeProfile = null;
        activeCustomBrand = preselectedBrand;

        PopulateFamilyDropdown();
        PopulateStyleDropdown();

        if (brandIcon != null) brandIcon.gameObject.SetActive(false);
        if (colorSquare != null) colorSquare.color = Color.white;
        if (itemNameText != null) itemNameText.text = "New Custom Filament";

        isUpdatingUI = true;
        if (nicknameInput != null) nicknameInput.text = "";
        if (colorNameInput != null) colorNameInput.text = "";

        if (availableFamilies != null && availableFamilies.Count > 0)
        {
            var fam = availableFamilies[0];
            if (customNozzleInput != null) customNozzleInput.text = fam.defaultNozzleTemperature.ToString();
            if (customBedInput != null) customBedInput.text = fam.defaultBedTemperature.ToString();
            if (densityInput != null) densityInput.text = fam.baseDensity.ToString();
        }
        else
        {
            if (customNozzleInput != null) customNozzleInput.text = "200";
            if (customBedInput != null) customBedInput.text = "60";
            if (densityInput != null) densityInput.text = "1.24";
        }

        if (remainingWeightInput != null) remainingWeightInput.text = "1000";
        if (originalWeightInput != null) originalWeightInput.text = "1000";
        if (notesInput != null) notesInput.text = "";
        isUpdatingUI = false;

        isColorNameManuallyEdited = false;
        SetCustomColor(Color.white, "#FFFFFF", "");

        if (saveButtonText != null) saveButtonText.text = "Create & Add to Inventory";
        if (deleteButton != null) deleteButton.gameObject.SetActive(false);

        UpdateCustomBrandUI();

        detailsPanel.SetActive(true);
        detailsPanel.transform.SetAsLastSibling();
    }

    public void OpenForEdit(SpoolInstance spool)
    {
        currentMode = 0;
        activeSpool = spool;
        activeProfile = InventoryManager.Instance.GetProfileForSpool(spool);

        PopulateFamilyDropdown();
        PopulateStyleDropdown();
        PopulateHeader(activeProfile);
        PopulateFieldsFromSpool();

        if (saveButtonText != null) saveButtonText.text = "Save Changes";
        if (deleteButton != null) deleteButton.gameObject.SetActive(true);

        detailsPanel.SetActive(true);
        detailsPanel.transform.SetAsLastSibling();
    }

    private void UpdateCustomBrandUI()
    {
        if (activeCustomBrand != null)
        {
            if (customBrandText != null) customBrandText.text = activeCustomBrand.brandName;
            if (customBrandIcon != null)
            {
                customBrandIcon.sprite = activeCustomBrand.brandLogo;
                customBrandIcon.gameObject.SetActive(true);
            }
        }
        else
        {
            if (customBrandText != null) customBrandText.text = "Select Brand (Optional)";
            if (customBrandIcon != null) customBrandIcon.gameObject.SetActive(false);
        }
    }

    private void OnCustomBrandButtonClicked()
    {
        if (catalogManager != null)
        {
            catalogManager.OpenBrandSelector(OnBrandSelected);
        }
    }

    private void OnBrandSelected(BrandSO selectedBrand)
    {
        activeCustomBrand = selectedBrand;
        UpdateCustomBrandUI();
    }

    private void PopulateFamilyDropdown()
    {
        if (materialFamilyDropdown == null) return;
        materialFamilyDropdown.ClearOptions();
        availableFamilies = AppManager.Instance.masterDatabase.allMaterialFamilies;

        System.Collections.Generic.List<string> options = new System.Collections.Generic.List<string>();
        foreach (var family in availableFamilies) options.Add(family.familyAbbreviation);
        materialFamilyDropdown.AddOptions(options);
    }

    private void PopulateStyleDropdown()
    {
        if (styleDropdown == null) return;
        styleDropdown.ClearOptions();
        availableStyles = AppManager.Instance.masterDatabase.allFilamentStyles;

        System.Collections.Generic.List<string> options = new System.Collections.Generic.List<string>();
        foreach (var style in availableStyles) options.Add(style.styleName);
        styleDropdown.AddOptions(options);
    }

    private void OnCustomColorButtonClicked()
    {
        if (colorPicker != null) colorPicker.Open(activeCustomColor, SetCustomColor);
    }

    private void SetCustomColor(Color newColor, string hexCode, string presetName)
    {
        activeCustomColor = newColor;
        activeCustomHex = hexCode;

        if (customColorPreview != null) customColorPreview.color = newColor;
        if (colorSquare != null) colorSquare.color = newColor;

        if (!isColorNameManuallyEdited && !string.IsNullOrEmpty(presetName) && colorNameInput != null)
        {
            isUpdatingUI = true;
            colorNameInput.text = presetName;
            isUpdatingUI = false;
        }
    }

    private void PopulateHeader(FilamentProfileSO profile)
    {
        if (profile == null) return;

        if (brandIcon != null && profile.brand != null) brandIcon.sprite = profile.brand.brandLogo;
        if (colorSquare != null) colorSquare.color = profile.displayColor;
        if (itemNameText != null) itemNameText.text = profile.GetDisplayName();
    }

    private void PopulateFieldsFromProfile()
    {
        isUpdatingUI = true;

        activeCustomBrand = activeProfile.brand;
        UpdateCustomBrandUI();

        if (nicknameInput != null) nicknameInput.text = activeProfile.GetDisplayName();
        if (colorNameInput != null) colorNameInput.text = activeProfile.itemName;
        if (customNozzleInput != null) customNozzleInput.text = activeProfile.GetActiveNozzleTemp().ToString();
        if (customBedInput != null) customBedInput.text = activeProfile.GetActiveBedTemp().ToString();
        if (densityInput != null) densityInput.text = activeProfile.GetActiveDensity().ToString();

        if (activeProfile.materialFamily != null && availableFamilies != null)
            materialFamilyDropdown.value = availableFamilies.FindIndex(f => f == activeProfile.materialFamily);

        if (activeProfile.visualStyle != null && availableStyles != null)
            styleDropdown.value = availableStyles.FindIndex(s => s == activeProfile.visualStyle);

        SetCustomColor(activeProfile.displayColor, "#" + ColorUtility.ToHtmlStringRGB(activeProfile.displayColor), "");

        isUpdatingUI = false;
        isColorNameManuallyEdited = false;
    }

    private void PopulateFieldsFromSpool()
    {
        isUpdatingUI = true;

        string bId = !string.IsNullOrEmpty(activeSpool.brandOverrideId) ? activeSpool.brandOverrideId : (activeProfile.brand != null ? activeProfile.brand.brandName : "");
        activeCustomBrand = AppManager.Instance.masterDatabase.allBrands.Find(b => b.brandName == bId);
        UpdateCustomBrandUI();

        if (nicknameInput != null) nicknameInput.text = InventoryManager.Instance.GetSpoolDisplayName(activeSpool);
        if (colorNameInput != null) colorNameInput.text = !string.IsNullOrEmpty(activeSpool.colorNameOverride) ? activeSpool.colorNameOverride : activeProfile.itemName;

        if (customNozzleInput != null) customNozzleInput.text = activeSpool.nozzleTempOverride > 0 ? activeSpool.nozzleTempOverride.ToString() : activeProfile.GetActiveNozzleTemp().ToString();
        if (customBedInput != null) customBedInput.text = activeSpool.bedTempOverride > 0 ? activeSpool.bedTempOverride.ToString() : activeProfile.GetActiveBedTemp().ToString();
        if (densityInput != null) densityInput.text = activeSpool.densityOverride > 0 ? activeSpool.densityOverride.ToString() : activeProfile.GetActiveDensity().ToString();

        string fId = !string.IsNullOrEmpty(activeSpool.familyOverrideId) ? activeSpool.familyOverrideId : (activeProfile.materialFamily != null ? activeProfile.materialFamily.familyAbbreviation : "");
        if (availableFamilies != null) materialFamilyDropdown.value = Mathf.Max(0, availableFamilies.FindIndex(f => f.familyAbbreviation == fId));

        string sId = !string.IsNullOrEmpty(activeSpool.styleOverrideId) ? activeSpool.styleOverrideId : (activeProfile.visualStyle != null ? activeProfile.visualStyle.styleName : "");
        if (availableStyles != null) styleDropdown.value = Mathf.Max(0, availableStyles.FindIndex(s => s.styleName == sId));

        Color c = InventoryManager.Instance.GetSpoolColor(activeSpool);
        SetCustomColor(c, "#" + ColorUtility.ToHtmlStringRGB(c), "");

        if (remainingWeightInput != null) remainingWeightInput.text = activeSpool.remainingWeightGrams.ToString();
        if (originalWeightInput != null) originalWeightInput.text = activeSpool.originalWeightGrams.ToString();
        if (notesInput != null) notesInput.text = activeSpool.customNotes;

        isUpdatingUI = false;
        isColorNameManuallyEdited = false;
    }

    private void OnSaveClicked()
    {
        float remaining = ParseFloat(remainingWeightInput != null ? remainingWeightInput.text : "1000", 1000f);
        float original = ParseFloat(originalWeightInput != null ? originalWeightInput.text : "1000", 1000f);

        if (currentMode == 0 && activeSpool != null)
        {
            ApplyOverridesToSpool(activeSpool);
            InventoryManager.Instance.SaveData();
        }
        else if (currentMode == 1 && activeProfile != null)
        {
            SpoolInstance newSpool = InventoryManager.Instance.AddSpoolToList(pendingListName, activeProfile, original);
            ApplyOverridesToSpool(newSpool);
            InventoryManager.Instance.SaveData();
        }
        else if (currentMode == 2)
        {
            string nick = nicknameInput != null ? nicknameInput.text : "";
            string color = colorNameInput != null ? colorNameInput.text : "Custom";
            int nozzle = ParseInt(customNozzleInput != null ? customNozzleInput.text : "", 200);
            int bed = ParseInt(customBedInput != null ? customBedInput.text : "", 60);
            float density = ParseFloat(densityInput != null ? densityInput.text : "", 1.24f);

            string brandId = activeCustomBrand != null ? activeCustomBrand.brandName : "";
            string familyId = "";

            if (materialFamilyDropdown != null && availableFamilies != null && availableFamilies.Count > 0)
            {
                var fam = availableFamilies[materialFamilyDropdown.value];
                familyId = fam.familyAbbreviation;

                if (nozzle == fam.defaultNozzleTemperature) nozzle = 0;
                if (bed == fam.defaultBedTemperature) bed = 0;
                if (Mathf.Approximately(density, fam.baseDensity)) density = 0f;
            }

            string styleId = "";
            if (styleDropdown != null && availableStyles != null && availableStyles.Count > 0)
                styleId = availableStyles[styleDropdown.value].styleName;

            SpoolInstance newSpool = InventoryManager.Instance.CreateAndAddCustomFilament(
                pendingListName, nick, color, activeCustomHex, nozzle, bed, density, brandId, familyId, styleId);

            newSpool.remainingWeightGrams = remaining;
            newSpool.originalWeightGrams = original;
            if (notesInput != null) newSpool.customNotes = notesInput.text;

            InventoryManager.Instance.SaveData();
        }

        ClosePanel();
    }

    private void ApplyOverridesToSpool(SpoolInstance spool)
    {
        spool.remainingWeightGrams = remainingWeightInput != null ? ParseFloat(remainingWeightInput.text, 1000f) : 1000f;
        spool.originalWeightGrams = originalWeightInput != null ? ParseFloat(originalWeightInput.text, 1000f) : 1000f;
        if (notesInput != null) spool.customNotes = notesInput.text;

        string enteredNickname = nicknameInput != null ? nicknameInput.text : "";
        string generatedName = activeProfile != null ? activeProfile.GetDisplayName() : "";
        spool.spoolNameOverride = (enteredNickname != generatedName) ? enteredNickname : "";

        if (activeProfile != null)
        {
            string cName = colorNameInput != null ? colorNameInput.text : "";
            spool.colorNameOverride = cName != activeProfile.itemName ? cName : "";

            string hexCheck = "#" + ColorUtility.ToHtmlStringRGB(activeProfile.displayColor);
            spool.colorHexOverride = activeCustomHex != hexCheck ? activeCustomHex : "";

            int nT = ParseInt(customNozzleInput.text, 0);
            spool.nozzleTempOverride = nT != activeProfile.GetActiveNozzleTemp() ? nT : 0;

            int bT = ParseInt(customBedInput.text, 0);
            spool.bedTempOverride = bT != activeProfile.GetActiveBedTemp() ? bT : 0;

            float d = ParseFloat(densityInput.text, 0f);
            spool.densityOverride = d != activeProfile.GetActiveDensity() ? d : 0f;

            spool.brandOverrideId = (activeCustomBrand != null && activeCustomBrand != activeProfile.brand) ? activeCustomBrand.brandName : "";

            if (availableFamilies != null && availableFamilies.Count > 0)
            {
                string fam = availableFamilies[materialFamilyDropdown.value].familyAbbreviation;
                spool.familyOverrideId = (activeProfile.materialFamily == null || fam != activeProfile.materialFamily.familyAbbreviation) ? fam : "";
            }

            if (availableStyles != null && availableStyles.Count > 0)
            {
                string sty = availableStyles[styleDropdown.value].styleName;
                spool.styleOverrideId = (activeProfile.visualStyle == null || sty != activeProfile.visualStyle.styleName) ? sty : "";
            }
        }
    }

    private float ParseFloat(string val, float fallback)
    {
        if (float.TryParse(val, out float res)) return res;
        return fallback;
    }

    private int ParseInt(string val, int fallback)
    {
        if (int.TryParse(val, out int res)) return res;
        return fallback;
    }

    private void OnDeleteClicked()
    {
        if (currentMode == 0 && activeSpool != null)
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