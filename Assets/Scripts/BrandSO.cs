using UnityEngine;

[CreateAssetMenu(fileName = "NewBrand", menuName = "3DPrintApp/Catalog/Brand")]
public class BrandSO : ScriptableObject
{
    public string brandName;
    public string brandAbbreviation;
    public Sprite brandLogo;
    public string websiteURL;
}