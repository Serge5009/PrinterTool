using System.IO;
using UnityEngine;
using TMPro;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using SFB; // StandaloneFileBrowser namespace
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
                DisplayMesh(generatedMesh, fileName);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to parse STL: " + e.Message);
            }
        }
        else if (extension == ".3mf")
        {
            Debug.LogWarning("3MF parsing is not yet implemented!");
        }
    }

    private void DisplayMesh(Mesh mesh, string modelName)
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
        }

        currentModel = new GameObject(modelName);

        MeshFilter meshFilter = currentModel.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = currentModel.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;

        if (defaultMaterial != null)
        {
            meshRenderer.material = defaultMaterial;
        }
        else
        {
            meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }

        currentModel.transform.position = -mesh.bounds.center;
    }
}