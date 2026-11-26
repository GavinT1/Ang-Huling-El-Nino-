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

    [Header("Player & NPC Control (Drag PLAYER and family memeber Here!)")]
    public GameObject playerObject;
    public List<GameObject> npcObjects = new List<GameObject>();

    [Header("Phase 1: Cutscene + Dialogue BEFORE Item Pickup")]
    public List<StoryStep> beforeItemsSteps = new List<StoryStep>();

    [Header("Phase 2: Item Pickup")]
    public int itemsRequired = 2;

    [Header("Phase 3: Cutscene + Dialogue AFTER Item Pickup")]
    public List<StoryStep> afterItemsSteps = new List<StoryStep>();

    [Header("Main Game Scene")]
    public string nextSceneName = "Area1";

    private List<StoryStep> activePhaseList;
    private int currentStepIndex = 0;
    private int itemsCollected = 0;

    private Queue<DialogueLine> lineQueue = new Queue<DialogueLine>();
    private bool isDialoguePlaying = false;
    private bool isItemHuntActive = false;
    private bool isInAfterItemsPhase = false;
    private Coroutine stepCoroutine;

    private Dictionary<GameObject, RuntimeAnimatorController> savedControllers = new Dictionary<GameObject, RuntimeAnimatorController>();

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        SetHorizontalCameraFollow(false);

        SaveAnimatorController(playerObject);
        foreach (GameObject npc in npcObjects)
        {
            SaveAnimatorController(npc);
        }

        dialoguePanel.SetActive(false);
        SetAllCharactersControl(false, false);

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

    private void SaveAnimatorController(GameObject obj)
    {
        if (obj == null) return;
        Animator anim = obj.GetComponent<Animator>();
        if (anim != null && !savedControllers.ContainsKey(obj))
        {
            savedControllers[obj] = anim.runtimeAnimatorController;
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
        SetHorizontalCameraFollow(false);

        // Lock movement AND mute AnimatorController for PLAYER + family memeber during cutscene
        SetAllCharactersControl(false, false);

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

        // Keep movement locked, and enable animations for PLAYER + family memeber during dialogue
        Time.timeScale = 1f;
        SetAllCharactersControl(false, true);

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
            SetAllCharactersControl(true, true);
            SetHorizontalCameraFollow(true);
        }
        else
        {
            SetHorizontalCameraFollow(false);
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(nextSceneName);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
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
            SetHorizontalCameraFollow(false);
            activePhaseList = afterItemsSteps;
            PlayStep(0);
        }
    }

    private void SetHorizontalCameraFollow(bool enabledState)
    {
        if (mainCamera == null) return;
        HorizontalCameraFollow camFollow = mainCamera.GetComponent<HorizontalCameraFollow>();
        if (camFollow != null)
        {
            camFollow.enabled = enabledState;
        }
    }

    private void SetAllCharactersControl(bool canMove, bool allowIdleAnimator)
    {
        ApplyCharacterControl(playerObject, canMove, allowIdleAnimator);
        foreach (GameObject npc in npcObjects)
        {
            // NPCs never get player movement scripts enabled, only their Animator toggles
            ApplyCharacterControl(npc, false, allowIdleAnimator);
        }
    }

    private void ApplyCharacterControl(GameObject target, bool canMove, bool allowIdleAnimator)
    {
        if (target == null) return;

        Animator anim = target.GetComponent<Animator>();
        if (anim != null)
        {
            anim.enabled = true;
            savedControllers.TryGetValue(target, out RuntimeAnimatorController savedCtrl);

            if (allowIdleAnimator)
            {
                anim.runtimeAnimatorController = savedCtrl;
            }
            else
            {
                if (savedCtrl != null)
                {
                    anim.runtimeAnimatorController = savedCtrl;
                    anim.Rebind();
                    anim.Update(0f);
                }
                anim.runtimeAnimatorController = null;
            }
        }

        if (target == playerObject)
        {
            Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (!canMove) rb.linearVelocity = Vector2.zero;
                rb.simulated = canMove;
            }

            foreach (MonoBehaviour script in target.GetComponents<MonoBehaviour>())
            {
                if (script != null) script.enabled = canMove;
            }
        }
    }
}