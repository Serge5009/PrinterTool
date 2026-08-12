using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NavigationManager : MonoBehaviour
{
    [System.Serializable]
    public class TabPair
    {
        public string tabName;
        public Button tabButton;
        public GameObject pagePanel;
    }

    [Header("Navigation Setup")]
    [Tooltip("Add your 3 main tabs here (Model, Filaments, Printers)")]
    public List<TabPair> tabs = new List<TabPair>();

    [Header("Visual Feedback (Optional)")]
    public Color activeTabColor = Color.white;
    public Color inactiveTabColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    private void Start()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            if (tabs[i].tabButton != null)
            {
                tabs[i].tabButton.onClick.AddListener(() => SwitchToTab(index));
            }
        }

        if (tabs.Count > 0)
        {
            SwitchToTab(0);
        }
    }

    public void SwitchToTab(int targetIndex)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            bool isActive = (i == targetIndex);

            if (tabs[i].pagePanel != null)
            {
                tabs[i].pagePanel.SetActive(isActive);
            }

            if (tabs[i].tabButton != null)
            {
                Image btnImage = tabs[i].tabButton.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = isActive ? activeTabColor : inactiveTabColor;
                }
            }
        }
    }
}