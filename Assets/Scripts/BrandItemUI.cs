using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BrandItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Button itemButton;
    public Image brandIcon;
    public TextMeshProUGUI brandNameText;

    private BrandSO currentBrand;
    private Action<BrandSO> onClickCallback;

    public void Setup(BrandSO brand, Action<BrandSO> onClick)
    {
        currentBrand = brand;
        onClickCallback = onClick;

        if (brand != null)
        {
            if (brandIcon != null) brandIcon.sprite = brand.brandLogo;
            if (brandNameText != null) brandNameText.text = brand.brandName;
        }

        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(OnItemClicked);
        }
    }

    private void OnItemClicked()
    {
        onClickCallback?.Invoke(currentBrand);
    }
}