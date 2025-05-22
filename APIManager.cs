// APIManager.cs
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class APIManager : MonoBehaviour
{
    // --- IMPORTANT: REPLACE WITH YOUR ACTUAL API KEY ---
    [SerializeField] private string rapidAPIKey = "YOUR_RAPIDAPI_KEY";
    // Verify this endpoint with your RapidAPI subscription for "Cartoon Yourself"
    private string apiURL = "https://cartoon-yourself.p.rapidapi.com/facebody/api/portrait-animation";
    private string rapidAPIHost = "cartoon-yourself.p.rapidapi.com";

    public UIManager uiManager; // Assign in Inspector

    public void UploadImageForCartoonEffect(Texture2D photoTexture, Action<Texture2D> onSuccess, Action<string> onError)
    {
        StartCoroutine(UploadImageCoroutine(photoTexture, onSuccess, onError));
    }

    private IEnumerator UploadImageCoroutine(Texture2D photoTexture, Action<Texture2D> onSuccess, Action<string> onError)
    {
        if (uiManager) uiManager.ShowLoading(true);

        // Encode texture to JPG (or PNG) bytes
        byte[] imageData = photoTexture.EncodeToJPG(75); // 75 is quality, adjust as needed
        // byte[] imageData = photoTexture.EncodeToPNG();
        if (imageData == null)
        {
            onError?.Invoke("Failed to encode image.");
            if (uiManager) uiManager.ShowLoading(false);
            yield break;
        }

        WWWForm form = new WWWForm();
        // The API documentation must confirm the field name for the image. "image" is common.
        form.AddBinaryData("image", imageData, "photo.jpg", "image/jpeg");

        // The "Grok AI" plan mentioned a "type" parameter for "comic".
        // Verify if the "Cartoon Yourself" API endpoint you're using requires this.
        // If it's a specific endpoint for comic style, this might not be needed.
        // form.AddField("type", "comic"); // Uncomment and set if required by the API

        using (UnityWebRequest www = UnityWebRequest.Post(apiURL, form))
        {
            www.SetRequestHeader("X-RapidAPI-Key", rapidAPIKey);
            www.SetRequestHeader("X-RapidAPI-Host", rapidAPIHost);
            // UnityWebRequest.Post with WWWForm usually sets Content-Type automatically.

            Debug.Log("Sending image to API...");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Success! Processing response...");
                // Assuming the API returns the image directly
                Texture2D resultTexture = new Texture2D(2, 2); // Temporary size
                if (resultTexture.LoadImage(www.downloadHandler.data))
                {
                    onSuccess?.Invoke(resultTexture);
                }
                else
                {
                    onError?.Invoke("Failed to load texture from API response. Response might not be an image.");
                    Debug.LogError("Raw API Response: " + www.downloadHandler.text);
                }
            }
            else
            {
                Debug.LogError($"API Error: {www.error}");
                Debug.LogError($"Response Code: {www.responseCode}");
                Debug.LogError($"Response Text: {www.downloadHandler.text}");
                onError?.Invoke($"API Error: {www.error} (Code: {www.responseCode})");
            }
        }
        if (uiManager) uiManager.ShowLoading(false);
    }
}