using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialVideoPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private RawImage videoScreen;
    [SerializeField] private Button playButton;
    [SerializeField] private Button closeButton;

    [Header("Video Setup")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip tutorialClip;

    private RenderTexture renderTexture;
    private bool isOpen = false;

    private void Awake()
    {
        // Create a dedicated RenderTexture so the video renders cleanly inside the UI card
        renderTexture = new RenderTexture(640, 360, 0);
        renderTexture.Create();

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            // CRITICAL: Allows the video to play while Time.timeScale = 0
            videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;

            if (tutorialClip != null)
            {
                videoPlayer.clip = tutorialClip;
            }
        }

        if (videoScreen != null)
        {
            videoScreen.texture = renderTexture;
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Allow closing with Escape or Space/Enter once the player has watched it
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePopup();
        }
    }

    public void OpenPopup(VideoClip overrideClip = null)
    {
        if (isOpen) return;
        isOpen = true;

        if (overrideClip != null && videoPlayer != null)
        {
            videoPlayer.clip = overrideClip;
        }

        // Pause gameplay
        Time.timeScale = 0f;

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }

        if (playButton != null)
        {
            playButton.gameObject.SetActive(true);
        }

        // Prepare frame 0 so the thumbnail preview shows immediately before clicking Play
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += ShowFirstFrame;
        }
    }

    private void ShowFirstFrame(VideoPlayer vp)
    {
        vp.prepareCompleted -= ShowFirstFrame;
        vp.frame = 0;
        vp.Pause();
    }

    public void OnPlayClicked()
    {
        if (playButton != null)
        {
            playButton.gameObject.SetActive(false);
        }

        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }

    public void ClosePopup()
    {
        if (!isOpen) return;
        isOpen = false;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        // Resume gameplay
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}