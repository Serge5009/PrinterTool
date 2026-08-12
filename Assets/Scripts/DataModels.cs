using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpoolInstance
{
    public string spoolId;

    [Tooltip("The unique ID of the FilamentProfileSO this spool belongs to.")]
    public string catalogItemId;
    public float remainingWeightGrams;
    public float originalWeightGrams;
    public string customNotes;
    public string dateAddedISO;
    public string spoolNameOverride;
    public string colorNameOverride;
    public string colorHexOverride;
    public int nozzleTempOverride;
    public int bedTempOverride;
    public float densityOverride;
    public string styleOverrideId;
    public string brandOverrideId;
    public string familyOverrideId;

    public SpoolInstance(string linkedCatalogId, float startingWeight = 1000f)
    {
        spoolId = Guid.NewGuid().ToString();
        catalogItemId = linkedCatalogId;
        originalWeightGrams = startingWeight;
        remainingWeightGrams = startingWeight;
        dateAddedISO = DateTime.UtcNow.ToString("O");
        customNotes = "";
    }
}

[Serializable]
public class InventoryList
{
    public string listName;
    public List<SpoolInstance> spools = new List<SpoolInstance>();

    public InventoryList(string name)
    {
        listName = name;
    }
}

[Serializable]
public class CustomFilamentData
{
    public string profileId;
    public string customName;
    public string hexColor;
    public int nozzleTemp;
    public int bedTemp;
    public float density;
    public string nickname;
    public string styleId;
    public string customBrandId;
    public string materialFamilyId;

    public CustomFilamentData()
    {
        profileId = "CUSTOM_" + Guid.NewGuid().ToString();
    }
}

[Serializable]
public class UserSaveData
{
    public List<InventoryList> allInventories = new List<InventoryList>();

    public List<CustomFilamentData> customProfiles = new List<CustomFilamentData>();
}