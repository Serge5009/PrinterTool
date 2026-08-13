using System.IO;
using UnityEngine;
using TMPro;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using SFB;
#endif

public class FileImportManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI fileNameText;

    [Header("Model Rendering")]
    public Material defaultMaterial;
    private GameObject currentModel;

    public void OpenFilePicker()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        var extensions = new[] {
            new ExtensionFilter("3D Printing Files", "stl", "3mf")
        };

        var paths = StandaloneFileBrowser.OpenFilePanel("Select a 3D Model", "", extensions, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            ProcessSelectedFile(paths[0]);
        }

#elif UNITY_ANDROID

        string[] fileTypes = new string[] { 
            NativeFilePicker.ConvertExtensionToFileType("stl"), 
            NativeFilePicker.ConvertExtensionToFileType("3mf") 
        };

        NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
            {
                Debug.Log("File selection cancelled by user.");
            }
            else
            {
                ProcessSelectedFile(path);
            }
        }, fileTypes);

#endif
    }

    private void ProcessSelectedFile(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        string extension = Path.GetExtension(filePath).ToLower();

        if (fileNameText != null)
        {
            fileNameText.text = fileName;
        }

        Debug.Log("Successfully loaded path: " + filePath);

        if (extension == ".stl")
        {
            try
            {
                Mesh generatedMesh = STLParser.LoadBinarySTL(filePath);

                DisplayModel(new Mesh[] { generatedMesh }, fileName);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to parse STL: " + e.Message);
            }
        }
        else if (extension == ".3mf")
        {
            try
            {
                Mesh[] generatedMeshes = ThreeMFParser.Load3MF(filePath);
                DisplayModel(generatedMeshes, fileName);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to parse 3MF: " + e.Message);
            }
        }
    }

    private void DisplayModel(Mesh[] meshes, string modelName)
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
        }

        currentModel = new GameObject(modelName);

        Bounds combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool boundsInitialized = false;

        for (int i = 0; i < meshes.Length; i++)
        {
            GameObject partObj = new GameObject($"Part_{i}");
            partObj.transform.SetParent(currentModel.transform, false);

            MeshFilter meshFilter = partObj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = partObj.AddComponent<MeshRenderer>();

            meshFilter.mesh = meshes[i];

            if (defaultMaterial != null)
            {
                meshRenderer.material = defaultMaterial;
            }
            else
            {
                meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }

            if (!boundsInitialized)
            {
                combinedBounds = meshes[i].bounds;
                boundsInitialized = true;
            }
            else
            {
                combinedBounds.Encapsulate(meshes[i].bounds);
            }
        }

        if (EnvironmentManager.Instance != null)
        {
            EnvironmentManager.Instance.PlaceModelOnBed(currentModel, combinedBounds);
        }
        else
        {
            currentModel.transform.position = new Vector3(-combinedBounds.center.x, -combinedBounds.min.y, -combinedBounds.center.z);
        }
    }
}