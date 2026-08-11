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

        if (fileNameText != null)
        {
            fileNameText.text = fileName;
        }

        Debug.Log("Successfully loaded path: " + filePath);

    }
}