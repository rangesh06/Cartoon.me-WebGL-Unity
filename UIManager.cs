// UIManager.cs
using UnityEngine;
using UnityEngine.UI; // If you need to directly interact with UI elements like text

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject welcomePanel;
    public GameObject capturePanel;
    public GameObject resultPanel;
    public GameObject loadingIndicator;

    // Optional: Reference if CameraManager needs to be triggered on screen change
    public CameraManager cameraManager;

    void Start()
    {
        ShowWelcomeScreen();
    }

    public void ShowWelcomeScreen()
    {
        welcomePanel.SetActive(true);
        capturePanel.SetActive(false);
        resultPanel.SetActive(false);
        if (loadingIndicator) loadingIndicator.SetActive(false);
        // Optional: Stop camera if returning to welcome screen
        if (cameraManager != null && cameraManager.IsCameraActive())
        {
            cameraManager.StopCamera();
        }
    }

    public void ShowCaptureScreen()
    {
        welcomePanel.SetActive(false);
        capturePanel.SetActive(true);
        resultPanel.SetActive(false);
        if (loadingIndicator) loadingIndicator.SetActive(false);

        if (cameraManager != null)
        {
            cameraManager.StartCamera();
        }
    }

    public void ShowResultScreen()
    {
        welcomePanel.SetActive(false);
        capturePanel.SetActive(false);
        resultPanel.SetActive(true);
        if (loadingIndicator) loadingIndicator.SetActive(false);

        // Stop camera when showing results
        if (cameraManager != null && cameraManager.IsCameraActive())
        {
            cameraManager.StopCamera();
        }
    }

    public void ShowLoading(bool isLoading)
    {
        if (loadingIndicator)
        {
            loadingIndicator.SetActive(isLoading);
            // Optionally disable other panels when loading to prevent interaction
            if (isLoading)
            {
                welcomePanel.SetActive(false);
                capturePanel.SetActive(false);
                resultPanel.SetActive(false);
            }
        }
    }
}
