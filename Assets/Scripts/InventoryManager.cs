using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public UserSaveData ActiveData { get; private set; }

    private Dictionary<string, FilamentProfileSO> runtimeCustomProfiles = new Dictionary<string, FilamentProfileSO>();

    private string SaveFilePath => Application.persistentDataPath + "/user_inventory.json";

    public event Action OnInventoryChanged;

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

    private void LoadData()
    {
        if (File.Exists(SaveFilePath))
        {
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                ActiveData = JsonUtility.FromJson<UserSaveData>(json);
                Debug.Log("[InventoryManager] Loaded existing inventory data.");

                RebuildRuntimeProfiles();

                OnInventoryChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[InventoryManager] Failed to load data: {e.Message}");
                InitializeFreshData();
            }
        }
        else
        {
            InitializeFreshData();
        }
    }

    private void InitializeFreshData()
    {
        ActiveData = new UserSaveData();

        ActiveData.allInventories.Add(new InventoryList("Owned Spools"));
        ActiveData.allInventories.Add(new InventoryList("Wishlist"));
        ActiveData.allInventories.Add(new InventoryList("Archived/Empty"));

        SaveData();
        RebuildRuntimeProfiles();
        Debug.Log("[InventoryManager] Created new fresh inventory profile.");
    }

    private void RebuildRuntimeProfiles()
    {
        runtimeCustomProfiles.Clear();
        foreach (var customData in ActiveData.customProfiles)
        {
            GenerateRuntimeSO(customData);
        }
    }

    private FilamentProfileSO GenerateRuntimeSO(CustomFilamentData data)
    {
        FilamentProfileSO newProfile = ScriptableObject.CreateInstance<FilamentProfileSO>();
        newProfile.uniqueId = data.profileId;
        newProfile.itemName = data.customName;
        newProfile.customNozzleTemp = data.nozzleTemp;
        newProfile.customBedTemp = data.bedTemp;
        newProfile.customDensity = data.density;

        if (!string.IsNullOrEmpty(data.customBrandId))
        {
            newProfile.brand = AppManager.Instance.masterDatabase.allBrands.FirstOrDefault(b => b.brandName == data.customBrandId);
        }

        if (!string.IsNullOrEmpty(data.materialFamilyId))
        {
            newProfile.materialFamily = AppManager.Instance.masterDatabase.allMaterialFamilies.FirstOrDefault(f => f.familyAbbreviation == data.materialFamilyId);
        }

        if (!string.IsNullOrEmpty(data.styleId))
        {
            newProfile.visualStyle = AppManager.Instance.masterDatabase.allFilamentStyles.FirstOrDefault(s => s.styleName == data.styleId);
        }

        newProfile.nickname = data.nickname;

        if (ColorUtility.TryParseHtmlString(data.hexColor, out Color parsedColor))
        {
            newProfile.displayColor = parsedColor;
        }

        runtimeCustomProfiles[data.profileId] = newProfile;
        return newProfile;
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(ActiveData, true);
        File.WriteAllText(SaveFilePath, json);

        OnInventoryChanged?.Invoke();
    }

    public SpoolInstance AddSpoolToList(string listName, FilamentProfileSO profile, float startingWeight = 1000f)
    {
        InventoryList targetList = ActiveData.allInventories.FirstOrDefault(l => l.listName == listName);

        if (targetList != null)
        {
            SpoolInstance newSpool = new SpoolInstance(profile.uniqueId, startingWeight);
            targetList.spools.Add(newSpool);

            SaveData();
            Debug.Log($"[InventoryManager] Added new spool to {listName}.");
            return newSpool;
        }

        Debug.LogError($"[InventoryManager] List '{listName}' does not exist!");
        return null;
    }

    public FilamentProfileSO GetProfileForSpool(SpoolInstance spool)
    {
        if (runtimeCustomProfiles.TryGetValue(spool.catalogItemId, out FilamentProfileSO customProfile))
        {
            return customProfile;
        }

        return AppManager.Instance.masterDatabase.allFilaments
            .FirstOrDefault(f => f.uniqueId == spool.catalogItemId);
    }

    public SpoolInstance CreateAndAddCustomFilament(string targetList, string nickname, string colorName, string hex, int nozzle, int bed, float density, string brandId, string familyId, string styleId)
    {
        CustomFilamentData newData = new CustomFilamentData
        {
            nickname = nickname,
            customName = colorName,
            hexColor = hex,
            nozzleTemp = nozzle,
            bedTemp = bed,
            density = density,
            customBrandId = brandId,
            materialFamilyId = familyId,
            styleId = styleId
        };

        ActiveData.customProfiles.Add(newData);

        FilamentProfileSO runtimeSO = GenerateRuntimeSO(newData);

        return AddSpoolToList(targetList, runtimeSO);
    }


    public string GetSpoolDisplayName(SpoolInstance spool)
    {
        if (!string.IsNullOrEmpty(spool.spoolNameOverride)) return spool.spoolNameOverride;

        FilamentProfileSO profile = GetProfileForSpool(spool);
        if (profile != null)
        {
            string b = !string.IsNullOrEmpty(spool.brandOverrideId) ? spool.brandOverrideId : (profile.brand != null ? profile.brand.brandName : "");
            string f = !string.IsNullOrEmpty(spool.familyOverrideId) ? spool.familyOverrideId : (profile.materialFamily != null ? profile.materialFamily.familyAbbreviation : "");
            string c = !string.IsNullOrEmpty(spool.colorNameOverride) ? spool.colorNameOverride : profile.itemName;

            string s = "";
            if (!string.IsNullOrEmpty(spool.styleOverrideId)) s = spool.styleOverrideId;
            else if (profile.visualStyle != null) s = profile.visualStyle.styleName;

            string rawName = $"{b} {f} {c} {s}";
            return System.Text.RegularExpressions.Regex.Replace(rawName, @"\s+", " ").Trim();
        }
        return "Unknown Spool";
    }

    public Color GetSpoolColor(SpoolInstance spool)
    {
        if (!string.IsNullOrEmpty(spool.colorHexOverride))
        {
            if (ColorUtility.TryParseHtmlString(spool.colorHexOverride, out Color c)) return c;
        }
        FilamentProfileSO profile = GetProfileForSpool(spool);
        if (profile != null) return profile.displayColor;
        return Color.white;
    }
}