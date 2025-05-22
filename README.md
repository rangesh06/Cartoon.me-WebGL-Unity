# Cartoon.me-WebGL-Unity
Turn your photo into a fun cartoon!

![Screenshot (1108)](https://github.com/user-attachments/assets/6b11be5e-225d-4cee-9a4a-fed58807d47e)
Overview
Cartoon.me is a fun and interactive web application built with Unity and deployed for WebGL. It allows users to capture a photo using their webcam, send it to an AI-powered API ("Cartoon Yourself" on RapidAPI), and receive a cartoon-style version of their image. The application is designed with a mobile-first approach, providing a seamless user experience on web browsers across various devices.

Features
Intuitive Welcome Screen: Greets users and provides a clear "START" button to begin the cartoonization process.
Live Camera Preview: Accesses the user's webcam and displays a live feed, allowing them to position themselves for a photo.
In-App Photo Capture: Enables users to easily take a snapshot directly from the live webcam feed.
AI-Powered Cartoonization: Sends the captured photograph to the "Cartoon Yourself" RapidAPI, which processes the image and returns a comic-style cartoon version.
Result Display: Clearly presents the generated cartoon image to the user within the application.
Image Download: Allows users to download their unique cartoonized image to their device.
Retake Photo Option: Provides a simple way for users to go back to the camera screen and capture a new photo if they wish.
User-Friendly Navigation: Smooth transitions between welcome, camera capture, and result display screens, managed by the UIManager.
Loading Feedback: Displays a loading indicator while the API is processing the image, keeping the user informed.
Automatic Image Adjustment: Includes logic for flipping images from front-facing cameras for correct orientation and uses AspectRatioFitter for proper display.
Technology Stack
Game Engine: Unity (Developed with Unity 2020.3.37f1, adaptable to other versions)
Platform: WebGL (for browser-based deployment, with a focus on mobile compatibility)
Programming Language: C#
External API: Cartoon Yourself API on RapidAPI (specifically using the https://cartoon-yourself.p.rapidapi.com/facebody/api/portrait-animation endpoint)
UI System: Unity UI (Canvas, RawImage, Button, TextMeshPro)
Core Unity Features/Packages:
UnityEngine.Networking for handling HTTP requests to the API.
UnityEngine.UI for all user interface elements.
WebCamTexture for camera access.
JavaScript Interoperability (DllImport("__Internal")) for WebGL-specific features like file download.
Core Scripts Overview
The project's functionality is primarily driven by the following C# scripts:

UIManager.cs:
Manages the state of the UI, controlling which panel (Welcome, Capture, Result, or Loading) is active.
Handles the flow of navigation between different screens based on user actions.
Interacts with CameraManager to initiate or stop camera previews when switching screens.

CameraManager.cs:
Manages all aspects of webcam interaction: requesting user permission, initializing the WebCamTexture, and displaying the feed on a RawImage.
Captures the current frame from the WebCamTexture into a Texture2D.
Includes a helper function to flip the texture horizontally, which is often necessary for front-facing cameras.
Passes the captured photo to APIManager for processing.
Displays the final cartoonized image on the result screen's RawImage.
Dynamically adds and configures an AspectRatioFitter to UI elements to maintain correct image proportions.

APIManager.cs:
Interfaces with the "Cartoon Yourself" RapidAPI.
Prepares the captured Texture2D by encoding it into JPG format.
Constructs an HTTP POST request using UnityWebRequest, attaching the image data and required headers (including the RapidAPI key).
Sends the request and awaits the asynchronous response.
Processes the API's response: if successful, it loads the returned image data into a new Texture2D.
Uses C# Action delegates for callbacks to handle success (passing the result texture) or failure (passing an error message).

DownloadController.cs:
Enables the image download functionality on the result screen.
Retrieves the Texture2D of the cartoonized image from its RawImage display.
Encodes the texture into JPG format.
For WebGL builds, it calls a JavaScript function (via [DllImport("__Internal")]) to trigger the browser's file download mechanism.
