using UnityEngine;
using System;

public class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }

    [Header("Global References")]
    [Tooltip("The master catalog for easy global access.")]
    public CatalogDatabaseSO masterDatabase;
    public PrinterProfileSO ActivePrinter { get; private set; }
    public event Action<PrinterProfileSO> OnPrinterChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetActivePrinter(PrinterProfileSO newPrinter)
    {
        ActivePrinter = newPrinter;

        Debug.Log($"[AppManager] Active Printer set to: {newPrinter.itemName}");

        OnPrinterChanged?.Invoke(newPrinter);
    }
}