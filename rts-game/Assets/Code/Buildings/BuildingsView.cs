using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingView : MonoBehaviour
{
    public int owner;
    public int hp;
    public int maxHp;
    public int Id { get; private set; }

    [SerializeField] private EntityId entityId;
    [SerializeField] private HealthBarScript healthBar;

    // ---------------- Training ----------------
    public float TrainingProgress { get; private set; } = 0f; // 0..1
    public List<string> TrainingQueue { get; private set; } = new List<string>();

    [Header("Unit Icons")]
    public Sprite warriorIcon;
    public Sprite archerIcon;
    public Sprite workerIcon;

    private void Awake()
    {
        if (entityId == null) entityId = GetComponent<EntityId>();
        if (entityId == null) entityId = GetComponentInParent<EntityId>();

        if (healthBar == null) healthBar = GetComponentInChildren<HealthBarScript>();
    }

    public void Bind(int id)
    {
        if (entityId == null)
        {
            Debug.LogError("BuildingView: no EntityId on prefab!");
            return;
        }

        entityId.Set(id);
    }

    public void ApplyServerState(float x, float y, int newHp, int newMaxHp, string type, int ownerId)
    {
        transform.position = new Vector3(x, y, 0f);

        hp = newHp;
        maxHp = newMaxHp;
        owner = ownerId;

        if (entityId == null)
            entityId = GetComponent<EntityId>();

        if (entityId == null)
            entityId = GetComponentInParent<EntityId>();

        if (healthBar == null)
            healthBar = GetComponentInChildren<HealthBarScript>();

        if (healthBar != null)
        {
            healthBar.SetHealth(hp, maxHp);
        }

        int safeId = entityId != null ? entityId.Id : Id;

        name = $"Building_{safeId}_{type}_owner{owner}";
    }

    public int GetId()
    {
        if (entityId == null) return -1;
        return entityId.Id;
    }

    // ---------------- Training Methods ----------------
    public void EnqueueUnit(string unitType)
    {
        TrainingQueue.Add(unitType);
        TrainingProgress = 0f;
    }

    public void UpdateTrainingProgress(float dt, float trainingTime)
    {
        if (TrainingQueue.Count == 0) return;

        TrainingProgress += dt / trainingTime;
        if (TrainingProgress >= 1f)
        {
            TrainingProgress = 0f;
            TrainingQueue.RemoveAt(0);
            // Тут можна викликати спавн юніта через LobbyClient або інший менеджер
        }
    }

    public Sprite GetUnitIcon(string unitType)
    {
        return unitType switch
        {
            "warrior" => warriorIcon,
            "archer" => archerIcon,
            "worker" => workerIcon,
            _ => null
        };
    }
}