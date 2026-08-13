using UnityEngine;
using System;

public class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }

    [Header("Global References")]
    [Tooltip("The master catalog for easy global access.")]
    public CatalogDatabaseSO masterDatabase;

    [Tooltip("The generic printer to use if the user hasn't selected an active one.")]
    public PrinterProfileSO fallbackPrinter;

    private PrinterProfileSO _activePrinter;
    public PrinterProfileSO ActivePrinter
    {
        get { return _activePrinter != null ? _activePrinter : fallbackPrinter; }
    }

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
        _activePrinter = newPrinter;

        Debug.Log($"[AppManager] Active Printer set to: {newPrinter.itemName}");

        OnPrinterChanged?.Invoke(newPrinter);
    }
}