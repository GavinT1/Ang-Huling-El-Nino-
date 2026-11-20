using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
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
        public PlayableDirector cutscene;
        public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text speakerText;
    public TMP_Text bodyText;

    [Header("Player Control")]
    public MonoBehaviour playerMovementScript;

    [Header("Phase 1: Cutscene + Dialogue BEFORE Item Pickup (e.g., 2 steps)")]
    public List<StoryStep> beforeItemsSteps = new List<StoryStep>();

    [Header("Phase 2: Item Pickup")]
    public int itemsRequired = 2;

    [Header("Phase 3: Cutscene + Dialogue AFTER Item Pickup (e.g., 5 steps)")]
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

    void Start()
    {
        dialoguePanel.SetActive(false);
        SetPlayerControl(false);

        isInAfterItemsPhase = false;
        activePhaseList = beforeItemsSteps;
        PlayStep(0);
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

        PlayableDirector director = activePhaseList[index].cutscene;

        if (director != null)
        {
            director.stopped -= OnCutsceneStopped;
            director.stopped += OnCutsceneStopped;
            director.time = 0;
            director.Play();
        }
        else
        {
            StartDialogueForCurrentStep();
        }
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
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = enabled;
        }
    }
}