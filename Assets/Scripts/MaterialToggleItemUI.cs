using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MaterialToggleItemUI : MonoBehaviour
{
    public TextMeshProUGUI familyNameText;
    public Toggle toggleComponent;

    private MaterialFamilySO currentFamily;
    private Action<MaterialFamilySO, bool> onToggleCallback;

    public void Setup(MaterialFamilySO family, bool isSelected, Action<MaterialFamilySO, bool> onToggle)
    {
        currentFamily = family;
        onToggleCallback = onToggle;

        if (familyNameText != null)
            familyNameText.text = $"{family.fullName} ({family.familyAbbreviation})";

        if (toggleComponent != null)
        {
            toggleComponent.onValueChanged.RemoveAllListeners();
            toggleComponent.isOn = isSelected;
            toggleComponent.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        onToggleCallback?.Invoke(currentFamily, isOn);
    }
}