using UnityEngine;

[CreateAssetMenu(fileName = "NewFilamentProfile", menuName = "3DPrintApp/Catalog/Filament Profile")]
public class FilamentProfileSO : CatalogItemSO
{
    [Header("Material Link")]
    [Tooltip("The base plastic type (e.g., PLA, PETG).")]
    public MaterialFamilySO materialFamily;

    [Header("Visual Properties")]
    public Color displayColor = Color.white;

    [Tooltip("Defines how this filament looks in the 3D visualizer (e.g., Matte, Silk).")]
    public FilamentStyleSO visualStyle;

    [Header("Manufacturer Overrides")]
    [Tooltip("Leave as 0 to use the Family default. Use if a specific brand needs higher temps.")]
    public int customNozzleTemp = 0;

    [Tooltip("Leave as 0 to use the Family default.")]
    public int customBedTemp = 0;

    [Tooltip("Leave as 0 to use the Family default density. Useful for lightweight/foaming PLA.")]
    public float customDensity = 0f;

    public int GetActiveNozzleTemp()
    {
        if (customNozzleTemp > 0) return customNozzleTemp;
        if (materialFamily != null) return materialFamily.defaultNozzleTemperature;
        return 0;
    }

    public int GetActiveBedTemp()
    {
        if (customBedTemp > 0) return customBedTemp;
        if (materialFamily != null) return materialFamily.defaultBedTemperature;
        return 0;
    }

    public float GetActiveDensity()
    {
        if (customDensity > 0f) return customDensity;
        if (materialFamily != null) return materialFamily.baseDensity;
        return 1.24f;
    }
}