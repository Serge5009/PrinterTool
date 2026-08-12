using UnityEngine;

[CreateAssetMenu(fileName = "NewColorPreset", menuName = "3DPrintApp/Catalog/Color Preset")]
public class ColorPresetSO : ScriptableObject
{
    public string presetName;
    public Color presetColor = Color.white;
}