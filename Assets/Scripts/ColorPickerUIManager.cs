using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ColorPickerUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pickerPanel;

    [Header("Previews")]
    public Image colorPreview;
    public TMP_InputField hexInput;

    [Header("Mode Toggle")]
    public Button toggleModeButton;
    public TextMeshProUGUI toggleModeText;

    [Header("Sliders")]
    public Slider slider1;
    public Slider slider2;
    public Slider slider3;

    public TextMeshProUGUI label1;
    public TextMeshProUGUI label2;
    public TextMeshProUGUI label3;

    public TMP_InputField value1Input;
    public TMP_InputField value2Input;
    public TMP_InputField value3Input;

    [Header("Presets")]
    public Transform presetsContainer;
    public GameObject presetButtonPrefab;

    [Header("Actions")]
    public Button applyButton;
    public Button closeButton;

    private Color currentColor = Color.white;
    private bool isRGBMode = true;
    private bool isUpdatingUI = false;
    private Action<Color, string, string> onApplyCallback;
    private string activePresetName = "";

    private void Start()
    {
        if (toggleModeButton != null) toggleModeButton.onClick.AddListener(ToggleMode);
        if (applyButton != null) applyButton.onClick.AddListener(ApplyColor);
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        if (slider1 != null) slider1.onValueChanged.AddListener(_ => OnSliderChanged());
        if (slider2 != null) slider2.onValueChanged.AddListener(_ => OnSliderChanged());
        if (slider3 != null) slider3.onValueChanged.AddListener(_ => OnSliderChanged());

        if (hexInput != null) hexInput.onEndEdit.AddListener(OnHexInputChanged);
    }

    public void Open(Color startingColor, Action<Color, string, string> onApply)
    {
        currentColor = startingColor;
        onApplyCallback = onApply;
        activePresetName = "";

        isRGBMode = true;

        PopulatePresets();
        ConfigureSliderRanges();
        RefreshUI();

        pickerPanel.SetActive(true);
        pickerPanel.transform.SetAsLastSibling();
    }

    private void PopulatePresets()
    {
        foreach (Transform child in presetsContainer) Destroy(child.gameObject);

        var presets = AppManager.Instance.masterDatabase.allColorPresets;
        if (presets == null || presetButtonPrefab == null) return;

        foreach (var preset in presets)
        {
            GameObject btnObj = Instantiate(presetButtonPrefab, presetsContainer);
            Image btnImg = btnObj.GetComponent<Image>();
            Button btn = btnObj.GetComponent<Button>();

            if (btnImg != null) btnImg.color = preset.presetColor;

            Color capturedColor = preset.presetColor;
            string capturedName = preset.presetName;
            if (btn != null) btn.onClick.AddListener(() => SetColorFromPreset(capturedColor, capturedName));
        }
    }

    private void ToggleMode()
    {
        isRGBMode = !isRGBMode;
        ConfigureSliderRanges();
        RefreshUI();
    }

    private void ConfigureSliderRanges()
    {
        isUpdatingUI = true;

        if (toggleModeText != null) toggleModeText.text = isRGBMode ? "Switch to HSV" : "Switch to RGB";

        if (isRGBMode)
        {
            label1.text = "R"; slider1.maxValue = 255;
            label2.text = "G"; slider2.maxValue = 255;
            label3.text = "B"; slider3.maxValue = 255;
        }
        else
        {
            label1.text = "H"; slider1.maxValue = 360;
            label2.text = "S"; slider2.maxValue = 100;
            label3.text = "V"; slider3.maxValue = 100;
        }

        isUpdatingUI = false;
    }

    private void OnSliderChanged()
    {
        if (isUpdatingUI) return;
        activePresetName = "";

        if (isRGBMode)
        {
            currentColor = new Color(slider1.value / 255f, slider2.value / 255f, slider3.value / 255f);
        }
        else
        {
            currentColor = Color.HSVToRGB(slider1.value / 360f, slider2.value / 100f, slider3.value / 100f);
        }

        RefreshUI(updateSliders: false);
    }

    private void OnHexInputChanged(string hex)
    {
        if (isUpdatingUI) return;
        activePresetName = "";

        if (!hex.StartsWith("#")) hex = "#" + hex;

        if (ColorUtility.TryParseHtmlString(hex, out Color parsedColor))
        {
            currentColor = parsedColor;
            RefreshUI();
        }
    }

    private void SetColorFromPreset(Color color, string presetName)
    {
        currentColor = color;
        activePresetName = presetName;
        RefreshUI();
    }

    private void RefreshUI(bool updateSliders = true)
    {
        isUpdatingUI = true;

        if (colorPreview != null) colorPreview.color = currentColor;

        string hexString = "#" + ColorUtility.ToHtmlStringRGB(currentColor);
        if (hexInput != null) hexInput.text = hexString;

        if (updateSliders)
        {
            if (isRGBMode)
            {
                slider1.value = Mathf.RoundToInt(currentColor.r * 255);
                slider2.value = Mathf.RoundToInt(currentColor.g * 255);
                slider3.value = Mathf.RoundToInt(currentColor.b * 255);
            }
            else
            {
                Color.RGBToHSV(currentColor, out float h, out float s, out float v);
                slider1.value = Mathf.RoundToInt(h * 360);
                slider2.value = Mathf.RoundToInt(s * 100);
                slider3.value = Mathf.RoundToInt(v * 100);
            }
        }

        if (value1Input != null) value1Input.text = slider1.value.ToString();
        if (value2Input != null) value2Input.text = slider2.value.ToString();
        if (value3Input != null) value3Input.text = slider3.value.ToString();

        isUpdatingUI = false;
    }

    private void ApplyColor()
    {
        string hex = "#" + ColorUtility.ToHtmlStringRGB(currentColor);
        onApplyCallback?.Invoke(currentColor, hex, activePresetName);
        Close();
    }

    public void Close()
    {
        if (pickerPanel != null) pickerPanel.SetActive(false);
    }
}