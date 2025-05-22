// CameraManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage cameraFeedRawImage; // Assign the RawImage from CapturePanel
    public RawImage resultDisplayRawImage; // Assign the RawImage from ResultPanel

    [Header("Managers")]
    public UIManager uiManager;         // Assign in Inspector
    public APIManager apiManager;       // Assign in Inspector

    private WebCamTexture webCamTexture;
    private Texture2D capturedPhoto;
    private bool isCameraInitialized = false;

    public bool IsCameraActive()
    {
        return webCamTexture != null && webCamTexture.isPlaying;
    }

    public void StartCamera()
    {
        if (isCameraInitialized && webCamTexture != null)
        {
            if (!webCamTexture.isPlaying)
            {
                webCamTexture.Play();
                cameraFeedRawImage.texture = webCamTexture;
                // Ensure RawImage is enabled if it was disabled
                cameraFeedRawImage.enabled = true;
                 AdjustAspectRatio(cameraFeedRawImage, webCamTexture.width, webCamTexture.height);
            }
            return;
        }
        StartCoroutine(InitializeCamera());
    }

    private IEnumerator InitializeCamera()
    {
        Debug.Log("Initializing camera...");
#if UNITY_WEBGL && !UNITY_EDITOR // In Editor, RequestUserAuthorization might not work as expected
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogError("Webcam access denied by user.");
            // Optionally, show a message on UI via UIManager
            uiManager.ShowWelcomeScreen(); // Or a specific error message screen
            yield break;
        }
#endif
        yield return null; // Allow a frame for permissions to process if needed

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("No webcam found!");
            // Optionally, show a message on UI
            uiManager.ShowWelcomeScreen();
            yield break;
        }

        // Prefer front camera for mobile, otherwise default
        string cameraName = devices[0].name;
        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing)
            {
                cameraName = devices[i].name;
                break;
            }
        }

        // Request a reasonable resolution
        webCamTexture = new WebCamTexture(cameraName, 640, 480, 30);
        cameraFeedRawImage.texture = webCamTexture;
        webCamTexture.Play();
        isCameraInitialized = true;
        cameraFeedRawImage.enabled = true; // Make sure it's visible

        // Wait for the camera to actually start and get dimensions
        while (webCamTexture.width < 100)
        {
            yield return null;
        }
        Debug.Log($"Webcam started: {webCamTexture.deviceName} {webCamTexture.width}x{webCamTexture.height}");
        AdjustAspectRatio(cameraFeedRawImage, webCamTexture.width, webCamTexture.height);
    }

    public void StopCamera()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
            Debug.Log("Webcam stopped.");
        }
        if (cameraFeedRawImage != null)
        {
            cameraFeedRawImage.texture = null; // Clear the texture
            cameraFeedRawImage.enabled = false; // Hide it
        }
        // isCameraInitialized = false; // Or keep it true if you want to quickly restart
    }

    public void OnCaptureButtonPressed()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            // Create a new Texture2D and grab the current frame
            // Flip an image
            Texture2D tempTexture = new Texture2D(webCamTexture.width, webCamTexture.height);
            tempTexture.SetPixels(webCamTexture.GetPixels());
            tempTexture.Apply();

            // If using front camera, image might be mirrored. Flip it if necessary.
            // This depends on how the webcam presents the image. Test this!
            if (webCamTexture.deviceName.ToLower().Contains("front")) // Basic check
            {
                 capturedPhoto = FlipTexture(tempTexture);
                 Destroy(tempTexture); // Clean up intermediate texture
            } else
            {
                 capturedPhoto = tempTexture;
            }


            Debug.Log("Photo captured!");

            // Optional: Stop webcam after capture for performance, show captured photo briefly
            // StopCamera();
            // cameraFeedRawImage.texture = capturedPhoto; // Show static captured image
            // cameraFeedRawImage.enabled = true;
            // AdjustAspectRatio(cameraFeedRawImage, capturedPhoto.width, capturedPhoto.height);


            // Send to API
            if (apiManager != null)
            {
                apiManager.UploadImageForCartoonEffect(capturedPhoto,
                    (Texture2D cartoonResult) => {
                        Debug.Log("Cartoon effect successful!");
                        DisplayResult(cartoonResult);
                        uiManager.ShowResultScreen();
                    },
                    (string errorMessage) => {
                        Debug.LogError($"Cartoon effect failed: {errorMessage}");
                        // Optionally, show error on UI and return to capture screen or welcome
                        uiManager.ShowCaptureScreen(); // Or an error message and then back
                    }
                );
            }
        }
        else
        {
            Debug.LogWarning("Webcam not active or not found. Attempting to start camera.");
            StartCamera(); // Try to restart if not active
        }
    }

    private void DisplayResult(Texture2D cartoonTexture)
    {
        if (resultDisplayRawImage != null && cartoonTexture != null)
        {
            resultDisplayRawImage.texture = cartoonTexture;
            AdjustAspectRatio(resultDisplayRawImage, cartoonTexture.width, cartoonTexture.height);
            // Store it if needed for download
            // (Handled by DownloadController or direct call)
        }
    }

    // Helper to flip texture if front camera is mirrored
    private Texture2D FlipTexture(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);
        int xN = original.width;
        int yN = original.height;

        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                flipped.SetPixel(xN - 1 - i, j, original.GetPixel(i, j)); // Horizontal flip
                // For vertical flip: flipped.SetPixel(i, yN - 1 - j, original.GetPixel(i, j));
            }
        }
        flipped.Apply();
        return flipped;
    }


    void AdjustAspectRatio(RawImage rawImage, float textureWidth, float textureHeight)
    {
        if (rawImage == null || textureWidth == 0 || textureHeight == 0) return;

        var fitter = rawImage.GetComponent<AspectRatioFitter>();
        if (fitter == null)
        {
            fitter = rawImage.gameObject.AddComponent<AspectRatioFitter>();
        }
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent; // Or HeightControlsWidth, WidthControlsHeight
        fitter.aspectRatio = textureWidth / textureHeight;
    }


    void OnDestroy()
    {
        StopCamera(); // Ensure camera is released when the object is destroyed
    }
}