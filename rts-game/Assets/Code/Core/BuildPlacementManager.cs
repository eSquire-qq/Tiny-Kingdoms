using UnityEngine;
using UnityEngine.EventSystems;

public class BuildPlacementManager : MonoBehaviour
{
    public static BuildPlacementManager Instance;

    [Header("Refs")]
    [SerializeField] private LobbyClient lobby;
    [SerializeField] private Camera cam;

    [Header("Ghost prefabs")]
    [SerializeField] private GameObject houseGhostPrefab;
    [SerializeField] private GameObject barracksGhostPrefab;
    [SerializeField] private GameObject archeryGhostPrefab;

    [Header("Settings")]
    [SerializeField] private LayerMask groundMask;

    private GameObject ghost;
    private string buildingType;
    private bool isPlacing;

    private void Awake()
    {
        Instance = this;

        if (lobby == null)
            lobby = FindFirstObjectByType<LobbyClient>();

        if (cam == null)
            cam = Camera.main;
    }

    private void Update()
    {
        if (!isPlacing) return;

        if (cam == null)
            cam = Camera.main;

        Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (ghost != null)
            ghost.transform.position = new Vector3(worldPos.x, worldPos.y, 0f);

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (!CanPlace(worldPos))
                return;

            lobby.SendBuildCommand(buildingType, worldPos.x, worldPos.y);
            CancelPlacement();
        }
    }

    public void StartPlacement(string type)
    {
        CancelPlacement();

        buildingType = type;
        isPlacing = true;

        GameObject prefab = GetGhostPrefab(type);

        if (prefab != null)
        {
            ghost = Instantiate(prefab);
            SetGhostVisual(ghost, true);
        }
    }

    private void CancelPlacement()
    {
        isPlacing = false;
        buildingType = "";

        if (ghost != null)
        {
            Destroy(ghost);
            ghost = null;
        }
    }

    private bool CanPlace(Vector2 pos)
    {
        if (groundMask.value == 0) return true;

        Collider2D ground = Physics2D.OverlapPoint(pos, groundMask);
        return ground != null;
    }

    private GameObject GetGhostPrefab(string type)
    {
        return type switch
        {
            "house" => houseGhostPrefab,
            "barracks" => barracksGhostPrefab,
            "archery" => archeryGhostPrefab,
            _ => null
        };
    }

    private void SetGhostVisual(GameObject obj, bool ghostMode)
    {
        var renderers = obj.GetComponentsInChildren<SpriteRenderer>();

        foreach (var r in renderers)
        {
            Color c = r.color;
            c.a = ghostMode ? 0.5f : 1f;
            r.color = c;
        }

        var colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders)
        {
            c.enabled = false;
        }
    }
    
    public void SetLobby(LobbyClient newLobby)
    {
        lobby = newLobby;
    }
    
}