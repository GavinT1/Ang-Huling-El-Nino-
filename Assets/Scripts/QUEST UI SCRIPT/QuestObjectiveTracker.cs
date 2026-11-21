using UnityEngine;
using TMPro;

public class QuestObjectiveTracker : MonoBehaviour
{
    public static QuestObjectiveTracker Instance;

    public enum QuestStage
    {
        ReadBoard,
        CollectJars,
        GoToExit
    }

    public QuestStage currentStage { get; private set; } = QuestStage.ReadBoard;

    [Header("HUD Elements (Genshin Style)")]
    public TextMeshProUGUI objectiveTitleText; 
    public TextMeshProUGUI objectiveSubText;  

    [Header("World Targets for Distance Tracking")]
    public Transform playerTransform;
    public Transform noticeBoardTarget;
    public Transform exitTarget;

    [Header("Optional World Waypoint Marker")]
    public Transform waypointDiamond; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SetStage(QuestStage.ReadBoard);
    }

    void Update()
    {
        UpdateSubText();
        UpdateWaypointMarker();
    }

    public void SetStage(QuestStage newStage)
    {
        currentStage = newStage;

        switch (currentStage)
        {
            case QuestStage.ReadBoard:
                if (objectiveTitleText != null)
                    objectiveTitleText.text = "Inspect the notice board";
                break;

            case QuestStage.CollectJars:
                if (objectiveTitleText != null)
                    objectiveTitleText.text = "Collect water jars (" +
                        JarGoalManager.Instance.collectedJars + "/" +
                        JarGoalManager.Instance.requiredJars + ")";
                break;

            case QuestStage.GoToExit:
                if (objectiveTitleText != null)
                    objectiveTitleText.text = "Proceed to the Area 2 exit";
                break;
        }

        UpdateSubText();
    }

    public void RefreshJarCount(int current, int required)
    {
        if (currentStage != QuestStage.CollectJars) return;

        if (objectiveTitleText != null)
            objectiveTitleText.text = "Collect water jars (" + current + "/" + required + ")";
    }

    private void UpdateSubText()
    {
        if (objectiveSubText == null || playerTransform == null) return;

        Transform activeTarget = GetActiveTarget();
        if (activeTarget != null)
        {
            float dist = Vector2.Distance(playerTransform.position, activeTarget.position);
            float heightDiff = activeTarget.position.y - playerTransform.position.y;

            // Genshin-style directional prefix
            string dirArrow = "";
            if (heightDiff > 2.5f) dirArrow = "▲ Up ";
            else if (heightDiff < -2.5f) dirArrow = "▼ Down ";

            objectiveSubText.text = dirArrow + Mathf.RoundToInt(dist) + "m";
        }
        else if (currentStage == QuestStage.CollectJars && JarGoalManager.Instance != null)
        {
            int remaining = JarGoalManager.Instance.requiredJars - JarGoalManager.Instance.collectedJars;
            objectiveSubText.text = remaining + " jars remaining in Area 1";
        }
    }

    private Transform GetActiveTarget()
    {
        if (currentStage == QuestStage.ReadBoard) return noticeBoardTarget;
        if (currentStage == QuestStage.GoToExit) return exitTarget;
        return null; // During CollectJars, jars are scattered across the map
    }

    private void UpdateWaypointMarker()
    {
        if (waypointDiamond == null) return;

        Transform target = GetActiveTarget();
        if (target != null)
        {
            waypointDiamond.gameObject.SetActive(true);
            float bob = Mathf.Sin(Time.time * 3f) * 0.1f;
            waypointDiamond.position = target.position + new Vector3(0f, 1.4f + bob, 0f);
        }
        else
        {
            waypointDiamond.gameObject.SetActive(false);
        }
    }
}