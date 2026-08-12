#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class FilamentExporterWindow : EditorWindow
{
    private UserSaveData activeSaveData;
    private CatalogDatabaseSO masterDB;
    private string targetRootFolder = "Assets/Data/Filaments";
    private const string PREF_KEY = "FilamentExporter_Folder";
    private Vector2 scrollPos;

    private Dictionary<string, bool> exportSelection = new Dictionary<string, bool>();

    [MenuItem("3DPrintApp/Custom Filament Exporter")]
    public static void ShowWindow()
    {
        GetWindow<FilamentExporterWindow>("Filament Exporter");
    }

    private void OnEnable()
    {
        targetRootFolder = EditorPrefs.GetString(PREF_KEY, "Assets/Data/Filaments");
        LoadUserData();
        FindMasterDatabase();
    }

    private void LoadUserData()
    {
        string savePath = Application.persistentDataPath + "/user_inventory.json";
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            activeSaveData = JsonUtility.FromJson<UserSaveData>(json);

            exportSelection.Clear();
            if (activeSaveData != null && activeSaveData.customProfiles != null)
            {
                foreach (var profile in activeSaveData.customProfiles)
                {
                    exportSelection[profile.profileId] = true;
                }
            }
        }
        else
        {
            activeSaveData = null;
        }
    }

    private void FindMasterDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("t:CatalogDatabaseSO");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            masterDB = AssetDatabase.LoadAssetAtPath<CatalogDatabaseSO>(path);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Export Custom Filaments to Database", EditorStyles.boldLabel);

        if (masterDB == null)
        {
            EditorGUILayout.HelpBox("Master Database not found! Please create one first.", MessageType.Error);
            return;
        }

        if (activeSaveData == null || activeSaveData.customProfiles == null || activeSaveData.customProfiles.Count == 0)
        {
            EditorGUILayout.HelpBox("No custom filaments found in your local save file.", MessageType.Info);
            if (GUILayout.Button("Reload Save Data")) LoadUserData();
            return;
        }

        EditorGUILayout.Space();
        GUILayout.Label("Target Export Folder (Must be inside Assets/):");
        GUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        targetRootFolder = EditorGUILayout.TextField(targetRootFolder);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString(PREF_KEY, targetRootFolder);
        }

        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Export Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path) && path.Contains("Assets"))
            {
                targetRootFolder = "Assets" + path.Substring(Application.dataPath.Length);
                EditorPrefs.SetString(PREF_KEY, targetRootFolder);
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUILayout.Label($"Found {activeSaveData.customProfiles.Count} Custom Profiles:", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box, GUILayout.Height(200));
        foreach (var profile in activeSaveData.customProfiles)
        {
            GUILayout.BeginHorizontal();
            exportSelection[profile.profileId] = EditorGUILayout.Toggle(exportSelection[profile.profileId], GUILayout.Width(20));
            GUILayout.Label($"{profile.customBrandId} {profile.materialFamilyId} {profile.customName} ({profile.styleId})");
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (GUILayout.Button("Export Selected to ScriptableObjects", GUILayout.Height(30)))
        {
            ExportSelected();
        }

        if (GUILayout.Button("Reload Save Data"))
        {
            LoadUserData();
        }
    }

    private void ExportSelected()
    {
        if (string.IsNullOrEmpty(targetRootFolder) || !targetRootFolder.StartsWith("Assets"))
        {
            Debug.LogError("[Exporter] Invalid target folder. Must start with 'Assets'.");
            return;
        }

        int exportCount = 0;
        int skipCount = 0;
        List<CustomFilamentData> exportedProfiles = new List<CustomFilamentData>();

        foreach (var data in activeSaveData.customProfiles)
        {
            if (!exportSelection.ContainsKey(data.profileId) || !exportSelection[data.profileId]) continue;

            BrandSO brand = masterDB.allBrands.FirstOrDefault(b => b.brandName == data.customBrandId);
            MaterialFamilySO family = masterDB.allMaterialFamilies.FirstOrDefault(f => f.familyAbbreviation == data.materialFamilyId);
            FilamentStyleSO style = masterDB.allFilamentStyles.FirstOrDefault(s => s.styleName == data.styleId);

            string brandAbbr = brand != null && !string.IsNullOrEmpty(brand.brandAbbreviation) ? brand.brandAbbreviation : "UNK";
            string famAbbr = family != null ? family.familyAbbreviation : "UNK";
            string colorName = SanitizeString(data.customName);
            string styleName = style != null ? SanitizeString(style.styleName) : "UNK";

            string uniqueId = $"{brandAbbr}_{famAbbr}_{colorName}_{styleName}";

            if (masterDB.allFilaments.Any(f => f.uniqueId == uniqueId))
            {
                Debug.LogWarning($"[Exporter] Skipped '{uniqueId}': A filament with this ID already exists in the master database.");
                skipCount++;
                continue;
            }

            string brandFolder = brand != null ? SanitizeString(brand.brandName) : "Unknown_Brand";
            string famFolder = family != null ? SanitizeString(family.familyAbbreviation) : "Unknown_Family";
            string styleFolder = style != null ? SanitizeString(style.styleName) : "Unknown_Style";

            string fullFolderPath = $"{targetRootFolder}/{brandFolder}/{famFolder}/{styleFolder}";

            if (!AssetDatabase.IsValidFolder(fullFolderPath))
            {
                Directory.CreateDirectory(Application.dataPath + fullFolderPath.Substring(6));
                AssetDatabase.Refresh();
            }

            string assetPath = $"{fullFolderPath}/{uniqueId}.asset";
            if (File.Exists(assetPath))
            {
                Debug.LogWarning($"[Exporter] Skipped '{uniqueId}': File already exists at path.");
                skipCount++;
                continue;
            }

            FilamentProfileSO newProfile = ScriptableObject.CreateInstance<FilamentProfileSO>();

            newProfile.uniqueId = uniqueId;
            newProfile.brand = brand;
            newProfile.materialFamily = family;
            newProfile.visualStyle = style;
            newProfile.itemName = data.customName;
            newProfile.nickname = data.nickname;

            if (ColorUtility.TryParseHtmlString(data.hexColor, out Color parsedColor))
            {
                newProfile.displayColor = parsedColor;
            }

            newProfile.customNozzleTemp = data.nozzleTemp;
            newProfile.customBedTemp = data.bedTemp;
            newProfile.customDensity = data.density;

            AssetDatabase.CreateAsset(newProfile, assetPath);
            exportCount++;

            foreach (var list in activeSaveData.allInventories)
            {
                foreach (var spool in list.spools)
                {
                    if (spool.catalogItemId == data.profileId)
                    {
                        spool.catalogItemId = uniqueId;
                    }
                }
            }
            exportedProfiles.Add(data);
        }

        if (exportedProfiles.Count > 0)
        {
            foreach (var p in exportedProfiles)
            {
                activeSaveData.customProfiles.Remove(p);
                exportSelection.Remove(p.profileId);
            }

            string savePath = Application.persistentDataPath + "/user_inventory.json";
            File.WriteAllText(savePath, JsonUtility.ToJson(activeSaveData, true));
            Debug.Log("[Exporter] Migrated user_inventory.json to link spools to the new static database.");

            if (Application.isPlaying && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.LoadData();
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=#00FF00><b>[Exporter Complete]</b></color> Successfully exported {exportCount} filaments. Skipped {skipCount}.");
    }

    private string SanitizeString(string input)
    {
        if (string.IsNullOrEmpty(input)) return "Unknown";
        string noSpaces = input.Replace(" ", "");
        return string.Join("_", noSpaces.Split(Path.GetInvalidFileNameChars()));
    }
}
#endif