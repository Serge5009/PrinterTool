using UnityEngine;
using System;

public abstract class CatalogItemSO : ScriptableObject
{
    [Header("Core Identification")]
    [Tooltip("A unique identifier for database lookups. Auto-generates if left blank.")]
    public string uniqueId;

    public BrandSO brand;
    public string itemName;

    [Header("UI Representation")]
    public Sprite icon;

    [TextArea(3, 5)]
    public string description;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}