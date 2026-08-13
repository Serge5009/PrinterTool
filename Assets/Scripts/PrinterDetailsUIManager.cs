using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class PrinterDetailsUIManager : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject detailsPanel;

    [Header("Header Visuals")]
    public Image brandIcon;
    public Image printerIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI statsText;

    [Header("Editable Fields")]
    public TMP_InputField nicknameInput;
    public TMP_InputField notesInput;
    public TMP_Dropdown modelTypeDropdown;

    [Header("All Stats & Overrides")]
    public TMP_InputField widthInput;
    public TMP_InputField depthInput;
    public TMP_InputField heightInput;
    public TMP_InputField maxNozzleInput;
    public TMP_InputField maxBedInput;
    public Toggle enclosedToggle;

    [Header("Material Selector")]
    public MaterialSelectorUIManager materialSelector;
    public Button selectMaterialsButton;
    public TextMeshProUGUI materialAbbreviationsText;

    [Header("Custom Brand Controls")]
    public CatalogUIManager catalogManager;
    public Button customBrandButton;
    public Image customBrandIcon;
    public TextMeshProUGUI customBrandText;

    [Header("Action Buttons")]
    public Button saveButton;
    public Button deleteButton;
    public Button closeButton;
    public Button setActiveButton;
    public TextMeshProUGUI saveButtonText;

    private PrinterInstance activePrinter;
    private PrinterProfileSO activeProfile;

    private int currentMode;
    private List<PrinterTypeSO> availableTypes;
    private BrandSO activeCustomBrand;
    private List<MaterialFamilySO> activeSupportedMaterials = new List<MaterialFamilySO>();

    private void Start()
    {
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
        if (deleteButton != null) deleteButton.onClick.AddListener(OnDeleteClicked);
        if (setActiveButton != null) setActiveButton.onClick.AddListener(OnSetActiveClicked);

        if (customBrandButton != null) customBrandButton.onClick.AddListener(OnCustomBrandButtonClicked);
        if (selectMaterialsButton != null) selectMaterialsButton.onClick.AddListener(OnSelectMaterialsClicked);
    }

    private void OnSelectMaterialsClicked()
    {
        if (materialSelector != null)
        {
            materialSelector.Open(activeSupportedMaterials, OnMaterialsConfirmed);
        }
    }

    private void OnMaterialsConfirmed(List<MaterialFamilySO> updatedMaterials)
    {
        activeSupportedMaterials = updatedMaterials;
        UpdateMaterialAbbreviations();
    }

    private void UpdateMaterialAbbreviations()
    {
        if (materialAbbreviationsText == null) return;

        if (activeSupportedMaterials == null || activeSupportedMaterials.Count == 0)
        {
            materialAbbreviationsText.text = "None Selected";
            return;
        }

        List<string> abbrs = activeSupportedMaterials.Select(m => m.familyAbbreviation).ToList();
        materialAbbreviationsText.text = string.Join(", ", abbrs);
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
        if (catalogManager != null) catalogManager.OpenBrandSelector(OnBrandSelected);
    }

    private void OnBrandSelected(BrandSO selectedBrand)
    {
        activeCustomBrand = selectedBrand;
        UpdateCustomBrandUI();
    }

    private void PopulateTypeDropdown()
    {
        if (modelTypeDropdown == null) return;

        modelTypeDropdown.ClearOptions();

        if (AppManager.Instance != null && AppManager.Instance.masterDatabase != null)
            availableTypes = AppManager.Instance.masterDatabase.allPrinterTypes;
        else
            availableTypes = new List<PrinterTypeSO>();

        List<string> options = new List<string>();
        foreach (var pType in availableTypes) options.Add(pType.typeName);

        modelTypeDropdown.AddOptions(options);
    }

    private void PopulateHeader(PrinterProfileSO profile)
    {
        if (profile == null) return;

        if (itemNameText != null) itemNameText.text = profile.itemName;
        if (statsText != null) statsText.text = $"{profile.buildVolume.x}x{profile.buildVolume.y}x{profile.buildVolume.z} mm";

        if (brandIcon != null)
        {
            if (profile.brand != null)
            {
                brandIcon.sprite = profile.brand.brandLogo;
                brandIcon.gameObject.SetActive(true);
            }
            else
            {
                brandIcon.gameObject.SetActive(false);
            }
        }

        if (printerIcon != null)
        {
            if (profile.icon != null)
            {
                printerIcon.sprite = profile.icon;
                printerIcon.gameObject.SetActive(true);
            }
            else
            {
                printerIcon.gameObject.SetActive(false);
            }
        }
    }

    private void PopulateFieldsFromProfile()
    {
        activeCustomBrand = activeProfile.brand;
        UpdateCustomBrandUI();

        if (nicknameInput != null) nicknameInput.text = activeProfile.itemName;
        if (notesInput != null) notesInput.text = "";

        if (widthInput != null) widthInput.text = activeProfile.buildVolume.x.ToString();
        if (depthInput != null) depthInput.text = activeProfile.buildVolume.y.ToString();
        if (heightInput != null) heightInput.text = activeProfile.buildVolume.z.ToString();

        if (maxNozzleInput != null) maxNozzleInput.text = activeProfile.maxNozzleTemperature.ToString();
        if (maxBedInput != null) maxBedInput.text = activeProfile.maxBedTemperature.ToString();
        if (enclosedToggle != null) enclosedToggle.isOn = activeProfile.isEnclosed;

        activeSupportedMaterials = new List<MaterialFamilySO>(activeProfile.supportedMaterials ?? new List<MaterialFamilySO>());
        UpdateMaterialAbbreviations();

        if (activeProfile.defaultPrinterType != null && availableTypes != null)
        {
            modelTypeDropdown.value = Mathf.Max(0, availableTypes.FindIndex(t => t == activeProfile.defaultPrinterType));
        }
    }

    private void PopulateFieldsFromPrinter()
    {
        string bId = activeProfile.brand != null ? activeProfile.brand.brandName : "";
        activeCustomBrand = AppManager.Instance.masterDatabase.allBrands.Find(b => b.brandName == bId);
        UpdateCustomBrandUI();

        if (nicknameInput != null) nicknameInput.text = !string.IsNullOrEmpty(activePrinter.customNickname) ? activePrinter.customNickname : activeProfile.itemName;
        if (notesInput != null) notesInput.text = activePrinter.customNotes;

        Vector3 v = activePrinter.buildVolumeOverride != Vector3.zero ? activePrinter.buildVolumeOverride : activeProfile.buildVolume;
        if (widthInput != null) widthInput.text = v.x.ToString();
        if (depthInput != null) depthInput.text = v.y.ToString();
        if (heightInput != null) heightInput.text = v.z.ToString();

        if (maxNozzleInput != null) maxNozzleInput.text = activePrinter.maxNozzleTempOverride > 0 ? activePrinter.maxNozzleTempOverride.ToString() : activeProfile.maxNozzleTemperature.ToString();
        if (maxBedInput != null) maxBedInput.text = activePrinter.maxBedTempOverride > 0 ? activePrinter.maxBedTempOverride.ToString() : activeProfile.maxBedTemperature.ToString();

        if (enclosedToggle != null)
        {
            if (activePrinter.enclosedOverride == 0) enclosedToggle.isOn = activeProfile.isEnclosed;
            else enclosedToggle.isOn = (activePrinter.enclosedOverride == 1);
        }

        activeSupportedMaterials.Clear();
        if (activePrinter.hasMaterialsOverride)
        {
            foreach (var mId in activePrinter.supportedMaterialsOverride)
            {
                var fam = AppManager.Instance.masterDatabase.allMaterialFamilies.FirstOrDefault(f => f.familyAbbreviation == mId);
                if (fam != null) activeSupportedMaterials.Add(fam);
            }
        }
        else
        {
            activeSupportedMaterials = new List<MaterialFamilySO>(activeProfile.supportedMaterials ?? new List<MaterialFamilySO>());
        }
        UpdateMaterialAbbreviations();

        string targetTypeId = !string.IsNullOrEmpty(activePrinter.typeOverrideId) ? activePrinter.typeOverrideId :
            (activeProfile.defaultPrinterType != null ? activeProfile.defaultPrinterType.typeId : "");

        if (availableTypes != null)
            modelTypeDropdown.value = Mathf.Max(0, availableTypes.FindIndex(t => t.typeId == targetTypeId));
    }

    public void OpenForCustomAdd(BrandSO preselectedBrand = null)
    {
        currentMode = 2;
        activePrinter = null;
        activeProfile = null;
        activeCustomBrand = preselectedBrand;

        PopulateTypeDropdown();
        UpdateCustomBrandUI();

        if (itemNameText != null) itemNameText.text = "New Custom Printer";
        if (statsText != null) statsText.text = "Custom Specifications";
        if (brandIcon != null) brandIcon.gameObject.SetActive(false);
        if (printerIcon != null) printerIcon.gameObject.SetActive(false);

        if (nicknameInput != null) nicknameInput.text = "";
        if (notesInput != null) notesInput.text = "";

        if (widthInput != null) widthInput.text = "200";
        if (depthInput != null) depthInput.text = "200";
        if (heightInput != null) heightInput.text = "200";
        if (maxNozzleInput != null) maxNozzleInput.text = "250";
        if (maxBedInput != null) maxBedInput.text = "80";
        if (enclosedToggle != null) enclosedToggle.isOn = false;

        activeSupportedMaterials.Clear();
        UpdateMaterialAbbreviations();

        if (saveButtonText != null) saveButtonText.text = "Create & Add to Inventory";
        if (deleteButton != null) deleteButton.gameObject.SetActive(false);
        if (setActiveButton != null) setActiveButton.gameObject.SetActive(false);

        if (detailsPanel != null)
        {
            detailsPanel.SetActive(true);
            detailsPanel.transform.SetAsLastSibling();
        }
    }

    public void OpenForAdd(PrinterProfileSO profile)
    {
        currentMode = 1;
        activeProfile = profile;
        activePrinter = null;

        PopulateTypeDropdown();
        PopulateHeader(profile);
        PopulateFieldsFromProfile();

        if (saveButtonText != null) saveButtonText.text = "Add to Inventory";
        if (deleteButton != null) deleteButton.gameObject.SetActive(false);
        if (setActiveButton != null) setActiveButton.gameObject.SetActive(false);

        if (detailsPanel != null)
        {
            detailsPanel.SetActive(true);
            detailsPanel.transform.SetAsLastSibling();
        }
    }

    public void OpenForEdit(PrinterInstance printer)
    {
        currentMode = 0;
        activePrinter = printer;
        activeProfile = InventoryManager.Instance.GetProfileForPrinter(printer);

        PopulateTypeDropdown();
        PopulateHeader(activeProfile);
        PopulateFieldsFromPrinter();

        if (saveButtonText != null) saveButtonText.text = "Save Changes";
        if (deleteButton != null) deleteButton.gameObject.SetActive(true);
        if (setActiveButton != null) setActiveButton.gameObject.SetActive(true);

        if (detailsPanel != null)
        {
            detailsPanel.SetActive(true);
            detailsPanel.transform.SetAsLastSibling();
        }
    }

    private void OnSaveClicked()
    {
        if (currentMode == 0 && activePrinter != null)
        {
            ApplyOverrides(activePrinter);
            InventoryManager.Instance.SaveData();
        }
        else if (currentMode == 1 && activeProfile != null)
        {
            PrinterInstance newPrinter = InventoryManager.Instance.AddPrinter(activeProfile);
            ApplyOverrides(newPrinter);
            InventoryManager.Instance.SaveData();

            if (InventoryManager.Instance.ActiveData.ownedPrinters.Count == 1)
                AppManager.Instance.SetActivePrinter(activeProfile);
        }
        else if (currentMode == 2)
        {
            string name = nicknameInput != null && !string.IsNullOrEmpty(nicknameInput.text) ? nicknameInput.text : "Custom Printer";
            string brandId = activeCustomBrand != null ? activeCustomBrand.brandName : "";

            float w = ParseFloat(widthInput != null ? widthInput.text : "200", 200f);
            float d = ParseFloat(depthInput != null ? depthInput.text : "200", 200f);
            float h = ParseFloat(heightInput != null ? heightInput.text : "200", 200f);
            Vector3 volume = new Vector3(w, d, h);

            int nozzle = ParseInt(maxNozzleInput != null ? maxNozzleInput.text : "250", 250);
            int bed = ParseInt(maxBedInput != null ? maxBedInput.text : "80", 80);
            bool enclosed = enclosedToggle != null && enclosedToggle.isOn;

            string typeId = "";
            if (availableTypes != null && availableTypes.Count > 0 && modelTypeDropdown != null)
                typeId = availableTypes[modelTypeDropdown.value].typeId;

            List<string> materials = activeSupportedMaterials.Select(m => m.familyAbbreviation).ToList();

            PrinterInstance newPrinter = InventoryManager.Instance.CreateAndAddCustomPrinter(name, brandId, volume, enclosed, nozzle, bed, typeId, materials);
            ApplyOverrides(newPrinter);
            InventoryManager.Instance.SaveData();

            if (InventoryManager.Instance.ActiveData.ownedPrinters.Count == 1)
            {
                var profile = InventoryManager.Instance.GetProfileForPrinter(newPrinter);
                if (profile != null) AppManager.Instance.SetActivePrinter(profile);
            }
        }
        ClosePanel();
    }

    private void ApplyOverrides(PrinterInstance printer)
    {
        if (notesInput != null) printer.customNotes = notesInput.text;

        string enteredNick = nicknameInput != null ? nicknameInput.text : "";
        string itemName = activeProfile != null ? activeProfile.itemName : "";
        printer.customNickname = (enteredNick != itemName) ? enteredNick : "";

        if (activeProfile != null)
        {
            float w = ParseFloat(widthInput.text, 0f);
            float d = ParseFloat(depthInput.text, 0f);
            float h = ParseFloat(heightInput.text, 0f);
            Vector3 v = new Vector3(w, d, h);
            printer.buildVolumeOverride = (v != activeProfile.buildVolume) ? v : Vector3.zero;

            int noz = ParseInt(maxNozzleInput.text, 0);
            printer.maxNozzleTempOverride = (noz != activeProfile.maxNozzleTemperature) ? noz : 0;

            int bed = ParseInt(maxBedInput.text, 0);
            printer.maxBedTempOverride = (bed != activeProfile.maxBedTemperature) ? bed : 0;

            bool enc = enclosedToggle.isOn;
            if (enc != activeProfile.isEnclosed) printer.enclosedOverride = enc ? 1 : 2;
            else printer.enclosedOverride = 0;

            bool materialsChanged = false;
            if (activeProfile.supportedMaterials == null || activeSupportedMaterials.Count != activeProfile.supportedMaterials.Count)
            {
                materialsChanged = true;
            }
            else
            {
                HashSet<string> original = new HashSet<string>(activeProfile.supportedMaterials.Select(m => m.familyAbbreviation));
                foreach (var m in activeSupportedMaterials)
                {
                    if (!original.Contains(m.familyAbbreviation)) materialsChanged = true;
                }
            }

            printer.hasMaterialsOverride = materialsChanged;
            printer.supportedMaterialsOverride = materialsChanged ? activeSupportedMaterials.Select(m => m.familyAbbreviation).ToList() : new List<string>();
        }

        if (availableTypes != null && availableTypes.Count > 0)
        {
            string selectedTypeId = availableTypes[modelTypeDropdown.value].typeId;
            string defaultId = (activeProfile != null && activeProfile.defaultPrinterType != null) ? activeProfile.defaultPrinterType.typeId : "";
            printer.typeOverrideId = (selectedTypeId != defaultId) ? selectedTypeId : "";
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
        if (currentMode == 0 && activePrinter != null)
        {
            InventoryManager.Instance.ActiveData.ownedPrinters.Remove(activePrinter);
            InventoryManager.Instance.SaveData();
        }
        ClosePanel();
    }

    private void OnSetActiveClicked()
    {
        if (activeProfile != null)
        {
            AppManager.Instance.SetActivePrinter(activeProfile);
            ClosePanel();
        }
    }

    public void ClosePanel()
    {
        if (detailsPanel != null) detailsPanel.SetActive(false);
    }
}