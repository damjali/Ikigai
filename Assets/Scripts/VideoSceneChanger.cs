using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoSceneChanger : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Drag your VideoPlayer object here, or leave blank if on the same object.")]
    public VideoPlayer videoPlayer;
    
    [Tooltip("The exact name of the scene you want to load.")]
    public string nextSceneName;

    void Start()
    {
        // If no video player is assigned, try to find one on this GameObject
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // Subscribe to the loopPointReached event
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // Unsubscribe to prevent memory leaks/multiple triggers
        videoPlayer.loopPointReached -= OnVideoFinished;

        // Load the target scene
        SceneManager.LoadScene("MainMenu");
    }
}