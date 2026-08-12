using UnityEngine;

[CreateAssetMenu(fileName = "NewFilamentStyle", menuName = "3DPrintApp/Catalog/Filament Style")]
public class FilamentStyleSO : ScriptableObject
{
    [Tooltip("The name of the style (e.g., 'Silk', 'Matte', 'Carbon Fiber').")]
    public string styleName;

    [Header("Rendering Blueprint")]
    [Tooltip("The base URP material to instantiate when rendering this style.")]
    public Material baseMaterialTemplate;

    [Tooltip("Default smoothness override for this specific style (0 to 1).")]
    [Range(0f, 1f)]
    public float defaultSmoothness = 0.5f;

    [Tooltip("Default metallic override for this specific style (0 to 1).")]
    [Range(0f, 1f)]
    public float defaultMetallic = 0.0f;
}