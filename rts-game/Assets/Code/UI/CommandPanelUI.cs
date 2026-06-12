using UnityEngine;

public enum CommandMode
{
    None,
    Move,
    Attack,
    Gather
}

public class CommandPanelUI : MonoBehaviour
{
    public static CommandPanelUI Instance;

    [Header("Refs")]
    [SerializeField] private LobbyClient lobby;
    [SerializeField] private CommandButtonUI[] buttons;

    [Header("Command Icons")]
    [SerializeField] private Sprite moveIcon;
    [SerializeField] private Sprite stopIcon;
    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite gatherIcon;
    [SerializeField] private Sprite buildIcon;

    [Header("Building Icons")]
    [SerializeField] private Sprite castleIcon;
    [SerializeField] private Sprite monasteryIcon;
    [SerializeField] private Sprite houseIcon;
    [SerializeField] private Sprite barracksIcon;
    [SerializeField] private Sprite archeryIcon;
    [SerializeField] private Sprite backIcon;

    [Header("Train Icons")]
    [SerializeField] private Sprite workerIcon;
    [SerializeField] private Sprite swordIcon;
    [SerializeField] private Sprite archerIcon;

    private int selectedUnitId = -1;
    private string selectedUnitType = "";

    private int selectedBuildingId = -1;
    private string selectedBuildingType = "";

    private CommandMode mode = CommandMode.None;

    private void Awake()
    {
        Instance = this;

        if (lobby == null)
            lobby = FindFirstObjectByType<LobbyClient>();

        HideAll();
    }

    public bool HasCommandMode()
    {
        return mode != CommandMode.None && selectedUnitId > 0;
    }

    public void ShowForUnit(UnitDto unit)
    {
        selectedUnitId = unit.id;
        selectedUnitType = unit.unitType;

        selectedBuildingId = -1;
        selectedBuildingType = "";
        mode = CommandMode.None;

        ShowUnitMainCommands();
    }

    private void ShowUnitMainCommands()
    {
        HideButtonsOnly();

        int i = 0;

        Add(ref i, "Move", moveIcon, () =>
        {
            mode = CommandMode.Move;
        });

        Add(ref i, "Stop", stopIcon, () =>
        {
            mode = CommandMode.None;
            lobby?.CmdStop(selectedUnitId);
        });

        Add(ref i, "Attack", attackIcon, () =>
        {
            mode = CommandMode.Attack;
        });

        if (selectedUnitType == "worker")
        {
            Add(ref i, "Gather", gatherIcon, () =>
            {
                mode = CommandMode.Gather;
            });

            Add(ref i, "Build", buildIcon, () =>
            {
                mode = CommandMode.None;
                ShowBuildMenu();
            });
        }
    }

    private void ShowBuildMenu()
    {
        HideButtonsOnly();

        int i = 0;

        Add(ref i, "Castle\n300G 150L", castleIcon, () =>
        {
            BuildPlacementManager.Instance?.StartPlacement("castle");
            ShowUnitMainCommands();
        });

        Add(ref i, "Monastery\n150G 75L", monasteryIcon, () =>
        {
            BuildPlacementManager.Instance?.StartPlacement("monastery");
            ShowUnitMainCommands();
        });

        Add(ref i, "House\n75G 25L", houseIcon, () =>
        {
            BuildPlacementManager.Instance?.StartPlacement("house");
            ShowUnitMainCommands();
        });

        Add(ref i, "Barracks\n200G 50L", barracksIcon, () =>
        {
            BuildPlacementManager.Instance?.StartPlacement("barracks");
            ShowUnitMainCommands();
        });

        Add(ref i, "Archery\n150G 100L", archeryIcon, () =>
        {
            BuildPlacementManager.Instance?.StartPlacement("archery");
            ShowUnitMainCommands();
        });

        Add(ref i, "Back", backIcon, () =>
        {
            ShowUnitMainCommands();
        });
    }

    public void ShowForBuilding(BuildingDto building)
    {
        selectedBuildingId = building.id;
        selectedBuildingType = building.type;

        selectedUnitId = -1;
        selectedUnitType = "";
        mode = CommandMode.None;

        HideButtonsOnly();

        int i = 0;

        if (selectedBuildingType == "monastery")
        {
            Add(ref i, "Worker\n50G", workerIcon, () =>
            {
                lobby?.CmdTrainUnit(selectedBuildingId, "worker");
            });
        }
        else if (selectedBuildingType == "barracks")
        {
            Add(ref i, "Sword\n100G", swordIcon, () =>
            {
                lobby?.CmdTrainUnit(selectedBuildingId, "swordsman");
            });
        }
        else if (selectedBuildingType == "archery")
        {
            Add(ref i, "Archer\n125G 25L", archerIcon, () =>
            {
                lobby?.CmdTrainUnit(selectedBuildingId, "archer");
            });
        }
    }

    public bool ExecuteCommand(Vector2 worldPos, Collider2D[] hits)
    {
        if (lobby == null || !lobby.inMatch)
            return false;

        if (selectedUnitId <= 0)
            return false;

        if (mode == CommandMode.Move)
        {
            lobby.CmdMove(selectedUnitId, worldPos.x, worldPos.y);
            mode = CommandMode.None;
            return true;
        }

        if (mode == CommandMode.Attack)
        {
            foreach (Collider2D hit in hits)
            {
                UnitView targetUnit = hit.GetComponentInParent<UnitView>();

                if (targetUnit != null && targetUnit.GetId() != selectedUnitId)
                {
                    UnitView selectedView = FindSelectedUnitView();

                    if (selectedView != null)
                        selectedView.PlayAttack();

                    lobby.CmdAttack(selectedUnitId, targetUnit.GetId());

                    mode = CommandMode.None;

                    return true;
                }

                BuildingClickHandler targetBuilding = hit.GetComponentInParent<BuildingClickHandler>();

                if (targetBuilding != null)
                {
                    UnitView selectedView = FindSelectedUnitView();

                    if (selectedView != null)
                        selectedView.PlayAttack();

                    lobby.CmdAttackBuilding(selectedUnitId, targetBuilding.buildingId);

                    mode = CommandMode.None;

                    return true;
                }

                BuildingView buildingView = hit.GetComponentInParent<BuildingView>();
                EntityId buildingEntity = hit.GetComponentInParent<EntityId>();

                if (buildingView != null && buildingEntity != null)
                {
                    UnitView selectedView = FindSelectedUnitView();

                    if (selectedView != null)
                        selectedView.PlayAttack();

                    lobby.CmdAttackBuilding(selectedUnitId, buildingEntity.Id);

                    mode = CommandMode.None;

                    return true;
                }
            }

            return false;
        }

        if (mode == CommandMode.Gather)
        {
            foreach (Collider2D hit in hits)
            {
                ResourceView resource = hit.GetComponentInParent<ResourceView>();

                if (resource != null)
                {
                    lobby.CmdGather(selectedUnitId, resource.resourceId);

                    mode = CommandMode.None;

                    return true;
                }
            }

            return false;
        }

        return false;
    }

    private UnitView FindSelectedUnitView()
    {
        UnitView[] units = FindObjectsByType<UnitView>(FindObjectsSortMode.None);

        foreach (UnitView unit in units)
        {
            if (unit != null && unit.GetId() == selectedUnitId)
                return unit;
        }

        return null;
    }

    public void HideAll()
    {
        mode = CommandMode.None;

        selectedUnitId = -1;
        selectedUnitType = "";

        selectedBuildingId = -1;
        selectedBuildingType = "";

        HideButtonsOnly();
    }

    private void HideButtonsOnly()
    {
        if (buttons == null)
            return;

        foreach (CommandButtonUI btn in buttons)
        {
            if (btn != null)
                btn.Hide();
        }
    }

    private void Add(ref int index, string text, Sprite icon, UnityEngine.Events.UnityAction action)
    {
        if (buttons == null || index >= buttons.Length)
            return;

        if (buttons[index] == null)
            return;

        buttons[index].Setup(text, icon, action);

        index++;
    }

    public void SetLobby(LobbyClient newLobby)
    {
        lobby = newLobby;
    }
}