using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitDto
{
    public int id;
    public int owner;
    public string unitType;
    public float x;
    public float y;
    public int hp;
    public int maxHp;
}

[Serializable]
public class PlayerDto
{
    public int playerId;
    public int gold;
    public int lumber;
    public int usedSupply;
    public int maxSupply;
}

[Serializable]
public class BuildingDto
{
    public int id;
    public int owner;
    public string type;
    public float x;
    public float y;
    public int hp;
    public int maxHp;

    public string currentUnit;
    public float trainTime;
    public int queueSize;
}

[Serializable]
public class ResourceDto
{
    public int id;
    public string type;
    public float x;
    public float y;
    public int amount;
}

[Serializable]
public class StateMsg
{
    public string type;
    public int tick;
    public UnitDto[] units;
    public BuildingDto[] buildings;
    public PlayerDto[] players;
    public ResourceDto[] resources;
}

public class UnitsClientWorld : MonoBehaviour
{
    [Header("Optional parent for spawned units")]
    public Transform unitsParent;

    [Header("Prefabs")]
    public GameObject warriorPrefab;
    public GameObject archerPrefab;
    public GameObject workerPrefab;

    private Dictionary<string, GameObject> prefabMap;

    private readonly Dictionary<int, UnitView> _byId = new();
    private readonly Dictionary<int, UnitDto> _dtoById = new();

    private void Awake()
    {
        prefabMap = new Dictionary<string, GameObject>()
        {
            { "swordsman", warriorPrefab },
            { "archer", archerPrefab },
            { "worker", workerPrefab }
        };
    }

    public bool TryGetUnit(int id, out UnitView view)
    {
        return _byId.TryGetValue(id, out view);
    }

    public UnitView TryGetView(int id)
    {
        return _byId.TryGetValue(id, out var view) ? view : null;
    }

    public bool TryGetUnitDto(int id, out UnitDto dto)
    {
        return _dtoById.TryGetValue(id, out dto);
    }

    public IEnumerable<UnitView> AllUnits()
    {
        return _byId.Values;
    }

    public bool TryRaycastUnitUnderMouse(out UnitView unit)
    {
        unit = null;

        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider == null) return false;

        unit = hit.collider.GetComponentInParent<UnitView>();
        return unit != null;
    }

    public void ApplyState(StateMsg state)
    {
        if (state == null || state.units == null) return;

        HashSet<int> aliveIds = new();

        foreach (UnitDto unit in state.units)
        {
            if (unit == null) continue;

            aliveIds.Add(unit.id);
            _dtoById[unit.id] = unit;

            if (!_byId.TryGetValue(unit.id, out UnitView view) || view == null)
            {
                view = Spawn(unit);
                if (view == null) continue;

                _byId[unit.id] = view;
            }

            view.owner = unit.owner;
            view.ApplyServerPos(unit.x, unit.y);
            view.ApplyHp(unit.hp, unit.maxHp);

            SelectionInfoUI.Instance?.UpdateUnit(unit);
        }

        List<int> toRemove = new();

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

    private UnitView Spawn(UnitDto dto)
    {
        if (prefabMap == null || !prefabMap.TryGetValue(dto.unitType, out GameObject prefab))
        {
            Debug.LogError("No prefab for unit type: " + dto.unitType);
            return null;
        }

        if (prefab == null)
        {
            Debug.LogError("Prefab is null for unit type: " + dto.unitType);
            return null;
        }

        Vector3 pos = new Vector3(dto.x, dto.y, 0f);
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity, unitsParent);

        UnitView view = obj.GetComponentInChildren<UnitView>();

        if (view == null)
        {
            Debug.LogError("Prefab " + dto.unitType + " has no UnitView!");
            Destroy(obj);
            return null;
        }

        view.Bind(dto.id);
        view.owner = dto.owner;
        view.ApplyHp(dto.hp, dto.maxHp);

        obj.name = $"Unit_{dto.id}_{dto.unitType}_owner{dto.owner}";

        return view;
    }
}