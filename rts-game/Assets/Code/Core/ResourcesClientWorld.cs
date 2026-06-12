using System.Collections.Generic;
using UnityEngine;

public class ResourcesClientWorld : MonoBehaviour
{
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private Transform resourcesParent;

    private readonly Dictionary<int, ResourceView> byId = new();

    public void ApplyState(StateMsg state)
    {
        if (state == null || state.resources == null) return;

        var alive = new HashSet<int>();

        foreach (var r in state.resources)
        {
            if (r == null) continue;

            alive.Add(r.id);

            if (!byId.TryGetValue(r.id, out var view) || view == null)
            {
                view = Spawn(r);

                if (view == null) 
                    continue;

                byId[r.id] = view;
            }

            view.transform.position = new Vector3(r.x, r.y, 0f);
            view.Bind(r.id, r.type);
        }

        var remove = new List<int>();

        foreach (var kv in byId)
        {
            if (!alive.Contains(kv.Key))
            {
                if (kv.Value != null)
                    Destroy(kv.Value.gameObject);

                remove.Add(kv.Key);
            }
        }

        foreach (var id in remove)
        {
            byId.Remove(id);
        }
    }

    private ResourceView Spawn(ResourceDto r)
    {
        GameObject prefab = GetPrefab(r.type);

        if (prefab == null)
        {
            Debug.LogError("No prefab for resource type: " + r.type);
            return null;
        }

        var obj = Instantiate(
            prefab,
            new Vector3(r.x, r.y, 0f),
            Quaternion.identity,
            resourcesParent
        );

        obj.name = $"Resource_{r.id}_{r.type}";

        var view = obj.GetComponent<ResourceView>();

        if (view == null)
        {
            Debug.LogError("Resource prefab has no ResourceView: " + r.type);
            Destroy(obj);
            return null;
        }

        view.Bind(r.id, r.type);

        return view;
    }

    private GameObject GetPrefab(string type)
    {
        return type switch
        {
            "gold" => goldPrefab,
            "lumber" => treePrefab,
            _ => null
        };
    }
}