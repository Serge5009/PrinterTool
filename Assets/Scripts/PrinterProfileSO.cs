using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPrinterProfile", menuName = "3DPrintApp/Catalog/Printer Profile")]
public class PrinterProfileSO : CatalogItemSO
{
    [Header("Physical Specifications")]
    [Tooltip("The printable area in millimeters: X (Width), Y (Depth), Z (Height)")]
    public Vector3 buildVolume;

    [Tooltip("Whether the printer has an active/passive enclosure for high-temp materials.")]
    public bool isEnclosed;

    [Header("Thermal Limits")]
    public int maxNozzleTemperature = 250;
    public int maxBedTemperature = 80;

    [Header("Material Compatibility")]
    [Tooltip("A list of standard material families this printer supports safely")]
    public List<MaterialFamilySO> supportedMaterials = new List<MaterialFamilySO>();

    public bool CanFitModel(Vector3 modelSize)
    {
        return modelSize.x <= buildVolume.x &&
               modelSize.z <= buildVolume.y &&
               modelSize.y <= buildVolume.z;
    }
}