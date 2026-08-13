using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewPrinterType", menuName = "3DPrintApp/Catalog/Printer Type")]
public class PrinterTypeSO : ScriptableObject
{
    [Tooltip("Unique ID for saving and overrides.")]
    public string typeId;

    [Tooltip("Display name (e.g., 'Bed Slinger (Open)', 'CoreXY (Enclosed)').")]
    public string typeName;

    [Tooltip("The generic 3D model prefab representing this style.")]
    public GameObject printerPrefab;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(typeId))
        {
            typeId = "PTYPE_" + Guid.NewGuid().ToString().Substring(0, 8);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}