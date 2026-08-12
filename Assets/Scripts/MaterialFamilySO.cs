using UnityEngine;

[CreateAssetMenu(fileName = "NewMaterialFamily", menuName = "3DPrintApp/Catalog/Material Family")]
public class MaterialFamilySO : ScriptableObject
{
    [Header("Core Info")]
    [Tooltip("The standard abbreviation, e.g., PLA, PETG, TPU")]
    public string familyAbbreviation;

    [Tooltip("The full name, e.g., Polylactic Acid")]
    public string fullName;

    [Header("Physical Properties")]
    [Tooltip("Density in g/cm³. Crucial for converting 3MF volume to spool weight. (PLA is ~1.24)")]
    public float baseDensity = 1.24f;

    [Header("Thermal Baselines")]
    public int defaultNozzleTemperature;
    public int defaultBedTemperature;

    [Header("Hardware Requirements")]
    [Tooltip("Does this material emit toxic fumes or warp heavily, requiring a closed printer?")]
    public bool requiresEnclosure;

    [Tooltip("Is this an abrasive material (like Carbon Fiber) requiring a hardened steel nozzle?")]
    public bool requiresHardenedNozzle;
}