using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "MasterCatalogDatabase", menuName = "3DPrintApp/Catalog/Master Database")]
public class CatalogDatabaseSO : ScriptableObject
{
    [Header("Categorized Data")]
    public List<BrandSO> allBrands = new List<BrandSO>();
    public List<MaterialFamilySO> allMaterialFamilies = new List<MaterialFamilySO>();
    public List<FilamentStyleSO> allFilamentStyles = new List<FilamentStyleSO>();
    public List<ColorPresetSO> allColorPresets = new List<ColorPresetSO>();

    [Space(10)]
    public List<PrinterProfileSO> allPrinters = new List<PrinterProfileSO>();
    public List<FilamentProfileSO> allFilaments = new List<FilamentProfileSO>();

    public List<FilamentProfileSO> GetFilamentsByBrand(BrandSO targetBrand)
    {
        return allFilaments.Where(f => f.brand == targetBrand).ToList();
    }

    public List<FilamentProfileSO> GetFilamentsByFamily(MaterialFamilySO targetFamily)
    {
        return allFilaments.Where(f => f.materialFamily == targetFamily).ToList();
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-Populate & Validate Database")]
    private void AutoPopulateAndValidate()
    {
        allBrands = FindAssetsByType<BrandSO>();
        allMaterialFamilies = FindAssetsByType<MaterialFamilySO>();
        allFilamentStyles = FindAssetsByType<FilamentStyleSO>();
        allColorPresets = FindAssetsByType<ColorPresetSO>();
        allPrinters = FindAssetsByType<PrinterProfileSO>();
        allFilaments = FindAssetsByType<FilamentProfileSO>();

        ValidateIDs();
        ValidateReferences();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log("<color=#00FF00><b>[Database Auditor]</b></color> Database successfully auto-populated and validated!");
    }

    private void ValidateIDs()
    {
        HashSet<string> seenIds = new HashSet<string>();

        List<CatalogItemSO> allItems = new List<CatalogItemSO>();
        allItems.AddRange(allPrinters);
        allItems.AddRange(allFilaments);

        foreach (var item in allItems)
        {
            if (string.IsNullOrEmpty(item.uniqueId))
            {
                Debug.LogError($"<color=red><b>[ID Error]</b></color> Missing ID on '{item.name}'. Open it in the inspector to auto-generate.");
                continue;
            }

            if (seenIds.Contains(item.uniqueId))
            {
                Debug.LogError($"<color=red><b>[DUPLICATE ID]</b></color> '{item.name}' shares the ID '{item.uniqueId}' with another item! Clear its ID in the inspector so it can regenerate.");
            }
            else
            {
                seenIds.Add(item.uniqueId);
            }
        }
    }

    private void ValidateReferences()
    {
        foreach (var filament in allFilaments)
        {
            if (string.IsNullOrEmpty(filament.itemName)) Debug.LogWarning($"<color=yellow>[Missing Data]</color> Filament '{filament.name}' is missing an Item/Color Name.");
            if (filament.brand == null) Debug.LogWarning($"<color=yellow>[Missing Data]</color> Filament '{filament.name}' is missing a Brand reference.");
            if (filament.materialFamily == null) Debug.LogWarning($"<color=yellow>[Missing Data]</color> Filament '{filament.name}' is missing a Material Family reference.");
            if (filament.visualStyle == null) Debug.LogWarning($"<color=yellow>[Missing Data]</color> Filament '{filament.name}' is missing a Visual Style reference.");
        }

        foreach (var printer in allPrinters)
        {
            if (string.IsNullOrEmpty(printer.itemName)) Debug.LogWarning($"<color=yellow>[Missing Data]</color> Printer '{printer.name}' is missing an Item Name.");
            if (printer.brand == null) Debug.LogWarning($"<color=yellow>[Missing Data]</color> Printer '{printer.name}' is missing a Brand reference.");
            if (printer.supportedMaterials == null || printer.supportedMaterials.Count == 0)
                Debug.LogWarning($"<color=yellow>[Missing Data]</color> Printer '{printer.name}' has no supported materials defined.");
        }
    }

    private List<T> FindAssetsByType<T>() where T : ScriptableObject
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }
#endif
}