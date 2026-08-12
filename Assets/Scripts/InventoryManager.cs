using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public UserSaveData ActiveData { get; private set; }
    public event Action OnInventoryChanged;

    private Dictionary<string, FilamentProfileSO> runtimeCustomProfiles = new Dictionary<string, FilamentProfileSO>();

    private string SaveFilePath => Application.persistentDataPath + "/user_inventory.json";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadData();
    }

    public void LoadData()
    {
        if (File.Exists(SaveFilePath))
        {
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                ActiveData = JsonUtility.FromJson<UserSaveData>(json);

                if (ActiveData == null) InitializeDefaultData();
            }
            catch (Exception e)
            {
                Debug.LogError($"[InventoryManager] Failed to load data: {e.Message}");
                InitializeDefaultData();
            }
        }
        else
        {
            InitializeDefaultData();
        }

        GenerateRuntimeCustomProfiles();
        OnInventoryChanged?.Invoke();
    }

    private void InitializeDefaultData()
    {
        ActiveData = new UserSaveData();
        ActiveData.allInventories.Add(new InventoryList("Owned Spools"));
        ActiveData.allInventories.Add(new InventoryList("Wishlist"));
        ActiveData.allInventories.Add(new InventoryList("Archived"));
    }

    public void SaveData()
    {
        try
        {
            string json = JsonUtility.ToJson(ActiveData, true);
            File.WriteAllText(SaveFilePath, json);
            OnInventoryChanged?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[InventoryManager] Failed to save data: {e.Message}");
        }
    }

    private void GenerateRuntimeCustomProfiles()
    {
        runtimeCustomProfiles.Clear();
        if (ActiveData.customProfiles == null) return;

        foreach (var customData in ActiveData.customProfiles)
        {
            FilamentProfileSO tempSO = ScriptableObject.CreateInstance<FilamentProfileSO>();
            tempSO.uniqueId = customData.profileId;
            tempSO.itemName = customData.customName;
            tempSO.nickname = customData.nickname;

            if (ColorUtility.TryParseHtmlString(customData.hexColor, out Color parsedColor))
            {
                tempSO.displayColor = parsedColor;
            }

            tempSO.customNozzleTemp = customData.nozzleTemp;
            tempSO.customBedTemp = customData.bedTemp;
            tempSO.customDensity = customData.density;

            if (AppManager.Instance != null && AppManager.Instance.masterDatabase != null)
            {
                tempSO.brand = AppManager.Instance.masterDatabase.allBrands.FirstOrDefault(b => b.brandName == customData.customBrandId);
                tempSO.materialFamily = AppManager.Instance.masterDatabase.allMaterialFamilies.FirstOrDefault(f => f.familyAbbreviation == customData.materialFamilyId);
                tempSO.visualStyle = AppManager.Instance.masterDatabase.allFilamentStyles.FirstOrDefault(s => s.styleName == customData.styleId);
            }

            runtimeCustomProfiles[customData.profileId] = tempSO;
        }
    }

    public SpoolInstance AddSpoolToList(string listName, FilamentProfileSO profile, float startingWeight = 1000f)
    {
        InventoryList list = ActiveData.allInventories.FirstOrDefault(l => l.listName == listName);
        if (list == null)
        {
            list = new InventoryList(listName);
            ActiveData.allInventories.Add(list);
        }

        SpoolInstance newSpool = new SpoolInstance(profile.uniqueId, startingWeight);
        list.spools.Add(newSpool);

        SaveData();
        return newSpool;
    }

    public SpoolInstance CreateAndAddCustomFilament(string listName, string nickname, string colorName, string hexColor, int nozzle, int bed, float density, string brandId, string familyId, string styleId)
    {
        CustomFilamentData newCustom = new CustomFilamentData
        {
            customName = colorName,
            nickname = nickname,
            hexColor = hexColor,
            nozzleTemp = nozzle,
            bedTemp = bed,
            density = density,
            customBrandId = brandId,
            materialFamilyId = familyId,
            styleId = styleId
        };

        ActiveData.customProfiles.Add(newCustom);

        GenerateRuntimeCustomProfiles();

        FilamentProfileSO profile = runtimeCustomProfiles[newCustom.profileId];
        return AddSpoolToList(listName, profile);
    }

    public FilamentProfileSO GetProfileForSpool(SpoolInstance spool)
    {
        if (AppManager.Instance != null && AppManager.Instance.masterDatabase != null)
        {
            var profile = AppManager.Instance.masterDatabase.allFilaments.FirstOrDefault(f => f.uniqueId == spool.catalogItemId);
            if (profile != null) return profile;
        }

        if (runtimeCustomProfiles.TryGetValue(spool.catalogItemId, out FilamentProfileSO customProfile))
        {
            return customProfile;
        }

        return null;
    }

    public string GetSpoolDisplayName(SpoolInstance spool)
    {
        if (!string.IsNullOrEmpty(spool.spoolNameOverride)) return spool.spoolNameOverride;

        var profile = GetProfileForSpool(spool);
        if (profile != null) return profile.GetDisplayName();

        return "Unknown Spool";
    }

    public Color GetSpoolColor(SpoolInstance spool)
    {
        if (!string.IsNullOrEmpty(spool.colorHexOverride) && ColorUtility.TryParseHtmlString(spool.colorHexOverride, out Color c))
        {
            return c;
        }

        var profile = GetProfileForSpool(spool);
        if (profile != null) return profile.displayColor;

        return Color.white;
    }
}