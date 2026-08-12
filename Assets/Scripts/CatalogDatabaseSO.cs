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
    [ContextMenu("Auto-Populate Database")]
    private void AutoPopulate()
    {
        allBrands = FindAssetsByType<BrandSO>();
        allMaterialFamilies = FindAssetsByType<MaterialFamilySO>();
        allFilamentStyles = FindAssetsByType<FilamentStyleSO>();
        allColorPresets = FindAssetsByType<ColorPresetSO>();
        allPrinters = FindAssetsByType<PrinterProfileSO>();
        allFilaments = FindAssetsByType<FilamentProfileSO>();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log("Database successfully auto-populated!");
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