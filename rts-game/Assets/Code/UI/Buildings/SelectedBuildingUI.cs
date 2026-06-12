using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectedBuildingUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public Slider progressBar;
    public Transform queueParent;
    public GameObject unitIconPrefab;

    private BuildingView currentBuilding;

    private readonly List<GameObject> spawnedIcons = new();

    private void Update()
    {
        if (currentBuilding == null || !panel.activeSelf) return;

        // Оновлюємо прогрес тренування
        progressBar.value = currentBuilding.TrainingProgress;
    }

    public void Show(BuildingView building)
    {
        currentBuilding = building;
        panel.SetActive(true);
        RefreshQueue();
    }

    public void Hide()
    {
        currentBuilding = null;
        panel.SetActive(false);
        ClearQueue();
    }

    public void RefreshQueue()
    {
        ClearQueue();

        if (currentBuilding == null) return;

        foreach (var unit in currentBuilding.TrainingQueue)
        {
            GameObject icon = Instantiate(unitIconPrefab, queueParent);
            // Можна поставити іконку відповідно до типу unit
            icon.GetComponent<Image>().sprite = currentBuilding.GetUnitIcon(unit);
            spawnedIcons.Add(icon);
        }
    }

    private void ClearQueue()
    {
        foreach (var icon in spawnedIcons)
            Destroy(icon);
        spawnedIcons.Clear();
    }
}