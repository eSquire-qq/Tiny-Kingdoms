using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    public static BuildingUI Instance;

    [Header("Refs")]
    [SerializeField] private GameObject queueSection;
    [SerializeField] private GameObject progressSection;
    [SerializeField] private TextMeshProUGUI queueText;
    [SerializeField] private Slider progressBar;

    private int selectedBuildingId = -1;

    private const float TRAIN_TIME = 3f;

    private void Awake()
    {
        Instance = this;
        HideActions();
    }

    public void ShowActions(BuildingDto building)
    {
        selectedBuildingId = building.id;

        bool canTrain = building.type == "barracks" || building.type == "archery";

        if (queueSection != null)
            queueSection.SetActive(canTrain);

        if (progressSection != null)
            progressSection.SetActive(canTrain);

        if (!canTrain)
            return;

        UpdateBuildingInfo(building);
    }

    public void HideActions()
    {
        selectedBuildingId = -1;

        if (queueSection != null)
            queueSection.SetActive(false);

        if (progressSection != null)
            progressSection.SetActive(false);

        if (queueText != null)
            queueText.text = "";

        if (progressBar != null)
        {
            progressBar.value = 0f;
            progressBar.gameObject.SetActive(false);
        }
    }

    public bool IsShowingBuilding(int id)
    {
        return selectedBuildingId == id;
    }

    public void UpdateBuildingInfo(BuildingDto building)
    {
        if (!IsShowingBuilding(building.id)) return;

        bool hasTraining =
            !string.IsNullOrEmpty(building.currentUnit) &&
            building.currentUnit != "null" &&
            building.currentUnit != "None";

        if (queueText != null)
        {
            if (hasTraining)
            {
                float timeLeft = Mathf.Max(0f, building.trainTime);
                queueText.text =
                    $"Training: {GetUnitName(building.currentUnit)}\n" +
                    $"Ready in: {timeLeft:0.0}s\n" +
                    $"Queue: {building.queueSize}";
            }
            else
            {
                queueText.text =
                    "Training: None\n" +
                    $"Queue: {building.queueSize}";
            }
        }

        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(hasTraining);

            if (hasTraining)
            {
                progressBar.minValue = 0f;
                progressBar.maxValue = 1f;

                float progress = 1f - Mathf.Clamp01(building.trainTime / TRAIN_TIME);
                progressBar.value = progress;
            }
        }
    }

    private string GetUnitName(string type)
    {
        return type switch
        {
            "swordsman" => "Swordsman",
            "archer" => "Archer",
            "worker" => "Worker",
            _ => type
        };
    }
}