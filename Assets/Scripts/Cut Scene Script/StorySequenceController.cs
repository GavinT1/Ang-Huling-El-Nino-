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
    public class DialogueBlock
    {
        public string blockLabel;
        public List<DialogueLine> lines = new List<DialogueLine>();
    }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text speakerText;
    public TMP_Text bodyText;

    [Header("Player Control")]
    public MonoBehaviour playerMovementScript; // Disabled during cutscenes/dialogue, enabled for item hunt

    [Header("Cutscenes (In Order: 0=Intro, 1=Mid1, 2=Mid2, 3=AfterItems)")]
    public List<PlayableDirector> cutscenes = new List<PlayableDirector>();

    [Header("Dialogue Blocks (0=4 lines, 1=5 lines, 2=4 lines)")]
    public List<DialogueBlock> dialogueBlocks = new List<DialogueBlock>();

    [Header("Item Quest & Next Scene")]
    public int itemsRequired = 2;
    public string nextSceneName = "Level_2";

    private int itemsCollected = 0;
    private int currentDialogueIndex = -1;
    private int activeCutsceneIndex = -1;
    private Queue<DialogueLine> lineQueue = new Queue<DialogueLine>();

    private bool isDialoguePlaying = false;
    private bool isItemHuntActive = false;
    private bool loadingNextSceneAfterCutscene = false;

    void Start()
    {
        dialoguePanel.SetActive(false);
        SetPlayerControl(false);

        // Start Cutscene 0 (Black screen + text + fade + walk) automatically
        PlayCutscene(0, loadSceneWhenFinished: false);
    }

    void Update()
    {
        // First line already showed automatically when the block started;
        // left-click advances through the remaining lines.
        if (isDialoguePlaying && Input.GetMouseButtonDown(0))
        {
            AdvanceDialogue();
        }
    }

    // --- CUTSCENE HANDLING ---
    public void PlayCutscene(int index, bool loadSceneWhenFinished = false)
    {
        if (index < 0 || index >= cutscenes.Count || cutscenes[index] == null) return;

        isItemHuntActive = false;
        isDialoguePlaying = false;
        dialoguePanel.SetActive(false);
        SetPlayerControl(false);

        activeCutsceneIndex = index;
        loadingNextSceneAfterCutscene = loadSceneWhenFinished;

        // Subscribe to finished event cleanly
        cutscenes[index].stopped -= OnCutsceneStopped;
        cutscenes[index].stopped += OnCutsceneStopped;

        cutscenes[index].time = 0;
        cutscenes[index].Play();
    }

    private void OnCutsceneStopped(PlayableDirector director)
    {
        director.stopped -= OnCutsceneStopped;

        if (loadingNextSceneAfterCutscene)
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        // After Cutscene 0 -> Dialogue 0 (4 lines)
        // After Cutscene 1 -> Dialogue 1 (5 lines)
        // After Cutscene 2 -> Dialogue 2 (4 lines)
        if (activeCutsceneIndex == 0) StartDialogueBlock(0);
        else if (activeCutsceneIndex == 1) StartDialogueBlock(1);
        else if (activeCutsceneIndex == 2) StartDialogueBlock(2);
    }

    // --- DIALOGUE HANDLING ---
    public void StartDialogueBlock(int blockIndex)
    {
        if (blockIndex < 0 || blockIndex >= dialogueBlocks.Count) return;

        currentDialogueIndex = blockIndex;
        lineQueue.Clear();

        foreach (var line in dialogueBlocks[blockIndex].lines)
        {
            lineQueue.Enqueue(line);
        }

        dialoguePanel.SetActive(true);
        isDialoguePlaying = true;

        // Automatically display the 1st line right away
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
            FinishCurrentDialogueBlock();
        }
    }

    private void FinishCurrentDialogueBlock()
    {
        isDialoguePlaying = false;
        dialoguePanel.SetActive(false);

        if (currentDialogueIndex == 0)
        {
            // Finished 4-line dialogue -> play Cutscene 2 (index 1)
            PlayCutscene(1, loadSceneWhenFinished: false);
        }
        else if (currentDialogueIndex == 1)
        {
            // Finished 5-line dialogue -> play Cutscene 3 (index 2)
            PlayCutscene(2, loadSceneWhenFinished: false);
        }
        else if (currentDialogueIndex == 2)
        {
            // Finished 4-line dialogue -> unlock player movement for 2 items
            StartItemCollectionPhase();
        }
    }

    // --- ITEM COLLECTION & FINAL SCENE ---
    private void StartItemCollectionPhase()
    {
        isItemHuntActive = true;
        itemsCollected = 0;
        SetPlayerControl(true);
    }

    public void CollectItem()
    {
        if (!isItemHuntActive) return;

        itemsCollected++;
        Debug.Log($"Items collected: {itemsCollected}/{itemsRequired}");

        if (itemsCollected >= itemsRequired)
        {
            // Got both items -> play Cutscene 4 (index 3), then load next scene
            PlayCutscene(3, loadSceneWhenFinished: true);
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