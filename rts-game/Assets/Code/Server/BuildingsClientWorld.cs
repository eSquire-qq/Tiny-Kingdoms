using System.Collections.Generic;
using UnityEngine;

public class BuildingsClientWorld : MonoBehaviour
{
    public Transform buildingsParent;

    [Header("Building Prefabs")]
    public GameObject castlePrefab;
    public GameObject monasteryPrefab;
    public GameObject barrackPrefab;
    public GameObject archerHousePrefab;
    public GameObject housePrefab;

    private Dictionary<string, GameObject> buildingsPrefabMap;

    private readonly Dictionary<int, BuildingView> _byId = new();
    private readonly Dictionary<int, BuildingDto> _dtoById = new();

    private void Awake()
    {
        buildingsPrefabMap = new Dictionary<string, GameObject>()
        {
            { "castle", castlePrefab },
            { "monastery", monasteryPrefab },
            { "barracks", barrackPrefab },
            { "archery", archerHousePrefab },
            { "house", housePrefab },
        };
    }

    public bool TryGetBuildingDto(int id, out BuildingDto dto)
    {
        return _dtoById.TryGetValue(id, out dto);
    }

    public void ApplyState(StateMsg state)
    {
        if (state == null || state.buildings == null) return;

        var aliveIds = new HashSet<int>();

        foreach (var b in state.buildings)
        {
            if (b == null) continue;

            aliveIds.Add(b.id);
            _dtoById[b.id] = b;

            if (!_byId.TryGetValue(b.id, out var view) || view == null)
            {
                view = Spawn(b);
                if (view == null) continue;

                _byId[b.id] = view;
            }

            view.ApplyServerState(b.x, b.y, b.hp, b.maxHp, b.type, b.owner);
            SelectionInfoUI.Instance?.UpdateBuilding(b);
        }

        var toRemove = new List<int>();

        foreach (var kv in _byId)
        {
            if (!aliveIds.Contains(kv.Key))
            {
                if (kv.Value != null)
                    Destroy(kv.Value.gameObject);

                toRemove.Add(kv.Key);
            }
        }

        foreach (int id in toRemove)
        {
            _byId.Remove(id);
            _dtoById.Remove(id);
        }
    }

    private BuildingView Spawn(BuildingDto dto)
    {
        if (buildingsPrefabMap == null || !buildingsPrefabMap.TryGetValue(dto.type, out var prefabToSpawn))
        {
            Debug.LogError("No prefab for building type: " + dto.type);
            return null;
        }

        if (prefabToSpawn == null)
        {
            Debug.LogError("Prefab is NULL for building type: " + dto.type);
            return null;
        }

        var pos = new Vector3(dto.x, dto.y, 0f);
        var obj = Instantiate(prefabToSpawn, pos, Quaternion.identity, buildingsParent);

        var view = obj.GetComponentInChildren<BuildingView>();

        if (view == null)
        {
            Debug.LogError("BuildingView NOT FOUND on prefab: " + dto.type);
            Destroy(obj);
            return null;
        }

        view.Bind(dto.id);
        view.ApplyServerState(dto.x, dto.y, dto.hp, dto.maxHp, dto.type, dto.owner);

        var clickHandler = obj.GetComponentInChildren<BuildingClickHandler>();

        if (clickHandler != null)
            clickHandler.buildingId = dto.id;
        else
            Debug.LogError("BuildingClickHandler NOT FOUND on prefab: " + dto.type);

        obj.name = $"Building_{dto.id}_{dto.type}_owner{dto.owner}";

        return view;
    }
}