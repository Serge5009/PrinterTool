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
public class UserSaveData
{
    public List<InventoryList> allInventories = new List<InventoryList>();
}