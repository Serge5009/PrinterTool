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
public class PrinterInstance
{
    public string instanceId;
    public string catalogItemId;
    public string customNickname;
    public string customNotes;

    public string typeOverrideId;
    public string dateAddedISO;

    public Vector3 buildVolumeOverride;
    public int maxNozzleTempOverride;
    public int maxBedTempOverride;

    public int enclosedOverride;

    public bool hasMaterialsOverride;
    public List<string> supportedMaterialsOverride;

    public PrinterInstance(string linkedCatalogId)
    {
        instanceId = Guid.NewGuid().ToString();
        catalogItemId = linkedCatalogId;
        customNickname = "";
        customNotes = "";
        typeOverrideId = "";
        dateAddedISO = DateTime.UtcNow.ToString("O");
        buildVolumeOverride = Vector3.zero;
        supportedMaterialsOverride = new List<string>();
    }
}

[Serializable]
public class CustomFilamentData
{
    public string profileId;
    public string customName;
    public string nickname;
    public string hexColor;
    public int nozzleTemp;
    public int bedTemp;
    public float density;
    public string customBrandId;
    public string materialFamilyId;
    public string styleId;

    public CustomFilamentData()
    {
        profileId = "CFIL_" + Guid.NewGuid().ToString();
    }
}

[Serializable]
public class CustomPrinterData
{
    public string profileId;
    public string customName;
    public string customBrandId;
    public Vector3 buildVolume;
    public bool isEnclosed;
    public int maxNozzleTemp;
    public int maxBedTemp;
    public string typeId;

    public List<string> supportedMaterialIds = new List<string>();

    public CustomPrinterData()
    {
        profileId = "CPRINT_" + Guid.NewGuid().ToString();
    }
}

[Serializable]
public class UserSaveData
{
    public List<InventoryList> allInventories = new List<InventoryList>();

    public List<CustomFilamentData> customProfiles = new List<CustomFilamentData>();

    public List<PrinterInstance> ownedPrinters = new List<PrinterInstance>();

    public List<CustomPrinterData> customPrinters = new List<CustomPrinterData>();
}