using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;

public class StorySequenceController : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string speaker;
        [TextArea(2, 4)] public string text;
    }

    [System.Serializable]
    public class StoryStep
    {
        public string stepLabel;
        public GameObject videoPanel;     // Drag UI video panel here (freezes game while active)
        public PlayableDirector cutscene; // Drag Timeline PlayableDirector here
        public Transform cameraAnchor;    // Drag BEDROOM CAM / KITCHEN CAM here
        public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    }

    [Header("Opening Video (Optional)")]
    public GameObject introVideoPanel;

    [Header("Camera")]
    public Camera mainCamera;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text speakerText;
    public TMP_Text bodyText;

    [Header("Player Control (Just Drag PLAYER Here!)")]
    public GameObject playerObject;

    [Header("Phase 1: Cutscene + Dialogue BEFORE Item Pickup")]
    public List<StoryStep> beforeItemsSteps = new List<StoryStep>();

    [Header("Phase 2: Item Pickup")]
    public int itemsRequired = 2;

    [Header("Phase 3: Cutscene + Dialogue AFTER Item Pickup")]
    public List<StoryStep> afterItemsSteps = new List<StoryStep>();

    [Header("Main Game Scene")]
    public string nextSceneName = "SampleScene";

    private List<StoryStep> activePhaseList;
    private int currentStepIndex = 0;
    private int itemsCollected = 0;

    private Queue<DialogueLine> lineQueue = new Queue<DialogueLine>();
    private bool isDialoguePlaying = false;
    private bool isItemHuntActive = false;
    private bool isInAfterItemsPhase = false;
    private Coroutine stepCoroutine;
    private RuntimeAnimatorController savedPlayerAnimatorController;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Save PLAYER's Animator Controller so we can mute normal Idle/Walk during cutscenes
        // while keeping Animator enabled for Timeline Animation Tracks!
        if (playerObject != null)
        {
            Animator anim = playerObject.GetComponent<Animator>();
            if (anim != null)
            {
                savedPlayerAnimatorController = anim.runtimeAnimatorController;
            }
        }

        dialoguePanel.SetActive(false);
        SetPlayerControl(false);

        isInAfterItemsPhase = false;
        activePhaseList = beforeItemsSteps;

        if (introVideoPanel != null && introVideoPanel.activeInHierarchy)
        {
            StartCoroutine(WaitForVideoPanelThenRun(introVideoPanel, () => PlayStep(0)));
        }
        else
        {
            PlayStep(0);
        }
    }

    void Update()
    {
        if (isDialoguePlaying && Input.GetMouseButtonDown(0))
        {
            AdvanceDialogue();
        }
    }

    private void PlayStep(int index)
    {
        if (activePhaseList == null || index >= activePhaseList.Count)
        {
            OnPhaseFinished();
            return;
        }

        currentStepIndex = index;
        isItemHuntActive = false;
        isDialoguePlaying = false;
        dialoguePanel.SetActive(false);
        SetPlayerControl(false);

        StoryStep step = activePhaseList[index];

        if (mainCamera != null && step.cameraAnchor != null)
        {
            Vector3 pos = step.cameraAnchor.position;
            pos.z = mainCamera.transform.position.z;
            mainCamera.transform.position = pos;
        }

        if (step.videoPanel != null)
        {
            step.videoPanel.SetActive(true);
            if (stepCoroutine != null) StopCoroutine(stepCoroutine);
            stepCoroutine = StartCoroutine(WaitForVideoPanelThenRun(step.videoPanel, () => PlayStepCutsceneOrDialogue(step)));
        }
        else
        {
            PlayStepCutsceneOrDialogue(step);
        }
    }

    private void PlayStepCutsceneOrDialogue(StoryStep step)
    {
        if (step.cutscene != null)
        {
            Time.timeScale = 1f;
            step.cutscene.stopped -= OnCutsceneStopped;
            step.cutscene.stopped += OnCutsceneStopped;
            step.cutscene.time = 0;
            step.cutscene.Play();
        }
        else
        {
            StartDialogueForCurrentStep();
        }
    }

    private IEnumerator WaitForVideoPanelThenRun(GameObject panel, System.Action onComplete)
    {
        VideoPlayer vp = panel.GetComponentInChildren<VideoPlayer>(true);
        if (vp != null)
        {
            vp.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
        }

        Time.timeScale = 0f;

        while (panel != null && panel.activeInHierarchy)
        {
            yield return null;
        }

        onComplete?.Invoke();
    }

    private void OnCutsceneStopped(PlayableDirector director)
    {
        director.stopped -= OnCutsceneStopped;
        StartDialogueForCurrentStep();
    }

    private void StartDialogueForCurrentStep()
    {
        List<DialogueLine> lines = activePhaseList[currentStepIndex].dialogueLines;

        if (lines == null || lines.Count == 0)
        {
            PlayStep(currentStepIndex + 1);
            return;
        }

        Time.timeScale = 0f;

        lineQueue.Clear();
        foreach (var line in lines)
        {
            lineQueue.Enqueue(line);
        }

        dialoguePanel.SetActive(true);
        isDialoguePlaying = true;
        AdvanceDialogue();
    }

    public void AdvanceDialogue()
    {
        if (lineQueue.Count > 0)
        {
            DialogueLine next = lineQueue.Dequeue();
            speakerText.text = next.speaker;
            bodyText.text = next.text;
        }
        else
        {
            isDialoguePlaying = false;
            dialoguePanel.SetActive(false);
            PlayStep(currentStepIndex + 1);
        }
    }

    private void OnPhaseFinished()
    {
        Time.timeScale = 1f;

        if (!isInAfterItemsPhase)
        {
            isItemHuntActive = true;
            itemsCollected = 0;
            SetPlayerControl(true);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void CollectItem()
    {
        if (!isItemHuntActive) return;

        itemsCollected++;

        if (itemsCollected >= itemsRequired)
        {
            isItemHuntActive = false;
            isInAfterItemsPhase = true;
            activePhaseList = afterItemsSteps;
            PlayStep(0);
        }
    }

    private void SetPlayerControl(bool enabled)
    {
        if (playerObject == null) return;

        // Keep Animator enabled so Timeline clips work, but detach the Controller during cutscenes
        // so normal Idle/Walk loops never bleed in!
        Animator anim = playerObject.GetComponent<Animator>();
        if (anim != null)
        {
            anim.enabled = true;
            anim.runtimeAnimatorController = enabled ? savedPlayerAnimatorController : null;
        }

        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (!enabled) rb.linearVelocity = Vector2.zero;
            rb.simulated = enabled;
        }

        foreach (MonoBehaviour script in playerObject.GetComponents<MonoBehaviour>())
        {
            if (script != null) script.enabled = enabled;
        }
    }
}