using UnityEngine;
using System.Collections.Generic;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance { get; private set; }

    [Header("Environment Setup")]
    [Tooltip("The generic cube representing the print bed.")]
    public Transform bedCube;

    [Tooltip("How thick the generic bed cube should be (in mm).")]
    public float bedThickness = 5f;

    [Header("Model Management")]
    [Tooltip("A dedicated parent object in the scene to hold all spawned models.")]
    public Transform modelContainer;

    [Header("Camera Configuration")]
    [Tooltip("Reference to our orbital camera so we can tell it to frame objects.")]
    public CameraController mainCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (AppManager.Instance != null)
        {
            AppManager.Instance.OnPrinterChanged += OnPrinterChanged;

            UpdateBedVolume(AppManager.Instance.ActivePrinter);
        }
    }

    private void OnDestroy()
    {
        if (AppManager.Instance != null)
        {
            AppManager.Instance.OnPrinterChanged -= OnPrinterChanged;
        }
    }

    private void OnPrinterChanged(PrinterProfileSO newPrinter)
    {
        UpdateBedVolume(newPrinter);
    }

    private void UpdateBedVolume(PrinterProfileSO printer)
    {
        if (printer == null || bedCube == null) return;


        float width = printer.buildVolume.x;
        float depth = printer.buildVolume.y;

        bedCube.localScale = new Vector3(width, bedThickness, depth);

        bedCube.position = new Vector3(0, -(bedThickness / 2f), 0);

        Debug.Log($"[EnvironmentManager] Bed resized to {width}x{depth}mm.");

        if (mainCamera != null && modelContainer != null && modelContainer.childCount == 0)
        {
            Bounds bedBounds = new Bounds(Vector3.zero, new Vector3(width * 0.8f, 10f, depth * 0.8f));
            mainCamera.FrameBounds(bedBounds, 1.0f);
        }
    }

    public void PlaceModelOnBed(GameObject modelRoot, Bounds combinedBounds)
    {
        if (modelContainer != null)
        {
            modelRoot.transform.SetParent(modelContainer);
        }


        float offsetX = -combinedBounds.center.x;
        float offsetZ = -combinedBounds.center.z;
        float offsetY = -combinedBounds.min.y;

        modelRoot.transform.position = new Vector3(offsetX, offsetY, offsetZ);

        if (mainCamera != null)
        {
            Vector3 worldCenter = new Vector3(0, combinedBounds.extents.y, 0);
            Bounds worldBounds = new Bounds(worldCenter, combinedBounds.size);

            mainCamera.FrameBounds(worldBounds, 1.2f);
        }

        Debug.Log($"[EnvironmentManager] Model placed. Dimensions: {combinedBounds.size.x:F1}x{combinedBounds.size.z:F1}x{combinedBounds.size.y:F1}mm");
    }
}