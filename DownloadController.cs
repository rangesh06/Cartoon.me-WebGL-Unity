// DownloadController.cs
using UnityEngine;
using UnityEngine.UI; // For Button
using System.Runtime.InteropServices; // For DllImport

public class DownloadController : MonoBehaviour
{
    public RawImage imageToDownload; // Assign the RawImage on ResultPanel that displays the cartoon

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadFile(byte[] array, int byteLength, string fileName);
#endif

    public void OnDownloadButtonPressed()
    {
        if (imageToDownload == null || imageToDownload.texture == null)
        {
            Debug.LogError("Image to download is not set or has no texture.");
            return;
        }

        Texture2D texture = imageToDownload.texture as Texture2D;
        if (texture == null)
        {
            Debug.LogError("Texture assigned to RawImage is not a Texture2D.");
            return;
        }

        byte[] imageBytes = texture.EncodeToJPG(85); // Encode to JPG with 85% quality
        // byte[] imageBytes = texture.EncodeToPNG(); // Or PNG

        if (imageBytes == null)
        {
            Debug.LogError("Failed to encode texture to bytes.");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        DownloadFile(imageBytes, imageBytes.Length, "cartoon_me_image.jpg");
#else
        Debug.LogWarning("Download functionality is only available in WebGL builds. In Editor, file would be saved to project root if implemented with System.IO.");
        // For Editor testing, you could save it locally:
        // System.IO.File.WriteAllBytes(Application.dataPath + "/../cartoon_me_image.jpg", imageBytes);
        // Debug.Log("Image saved to project root folder for Editor test.");
#endif
    }
}