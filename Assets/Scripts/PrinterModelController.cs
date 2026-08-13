using UnityEngine;

public class PrinterModelController : MonoBehaviour
{
    [Header("Hardware References")]
    [Tooltip("The object representing the physical print bed.")]
    public Transform bedTransform;

    [Tooltip("An empty GameObject placed exactly where the bottom-center of a printed object should spawn (usually resting on top of the bed).")]
    public Transform printOrigin;

    [Header("Original Prefab Dimensions")]
    [Tooltip("The unscaled size of the print bed in this specific 3D model. (X = Width, Z = Depth).")]
    public Vector2 originalBedSize = new Vector2(200f, 200f);

    [Tooltip("The unscaled maximum Z-height of this generic model.")]
    public float originalMaxHeight = 200f;

    public void ApplyBuildVolumeScale(Vector3 targetBuildVolume)
    {
        if (originalBedSize.x <= 0 || originalBedSize.y <= 0 || originalMaxHeight <= 0)
        {
            Debug.LogError($"[PrinterModel] Invalid original dimensions on {gameObject.name}. Cannot scale.");
            return;
        }

        float scaleX = targetBuildVolume.x / originalBedSize.x;

        float scaleZ = targetBuildVolume.y / originalBedSize.y;

        float scaleY = targetBuildVolume.z / originalMaxHeight;

        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }
}