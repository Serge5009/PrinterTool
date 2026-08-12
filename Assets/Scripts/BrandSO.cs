using UnityEngine;

[CreateAssetMenu(fileName = "NewBrand", menuName = "3DPrintApp/Catalog/Brand")]
public class BrandSO : ScriptableObject
{
    [Tooltip("The official name of the brand.")]
    public string brandName;

    [Tooltip("The logo to display in the UI dropdowns.")]
    public Sprite brandLogo;

    [Tooltip("Optional link to their official store.")]
    public string websiteURL;
}