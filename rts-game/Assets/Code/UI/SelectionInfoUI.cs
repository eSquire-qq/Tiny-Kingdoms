using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionInfoUI : MonoBehaviour
{
    public static SelectionInfoUI Instance;

    [Header("Refs")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Unit Icons")]
    [SerializeField] private Sprite swordsmanIcon;
    [SerializeField] private Sprite archerIcon;
    [SerializeField] private Sprite workerIcon;

    [Header("Building Icons")]
    [SerializeField] private Sprite barracksIcon;
    [SerializeField] private Sprite archeryIcon;
    [SerializeField] private Sprite houseIcon;

    private int selectedId = -1;
    private bool selectedIsBuilding;

    private void Awake()
    {
        Instance = this;

        if (panel == null)
            panel = gameObject;

        panel.SetActive(false);
    }

    public void ShowUnit(UnitDto unit)
    {
        selectedId = unit.id;
        selectedIsBuilding = false;

        panel.SetActive(true);

        if (nameText != null)
            nameText.text = $"{GetUnitName(unit.unitType)} #{unit.id}";

        if (iconImage != null)
        {
            iconImage.sprite = GetUnitIcon(unit.unitType);
            iconImage.enabled = iconImage.sprite != null;
        }

        UpdateHp(unit.hp, unit.maxHp);

        BuildingUI.Instance?.HideActions();
        CommandPanelUI.Instance?.ShowForUnit(unit);
    }

    public void ShowBuilding(BuildingDto building)
    {
        selectedId = building.id;
        selectedIsBuilding = true;

        panel.SetActive(true);

        if (nameText != null)
            nameText.text = $"{GetBuildingName(building.type)} #{building.id}";

        if (iconImage != null)
        {
            iconImage.sprite = GetBuildingIcon(building.type);
            iconImage.enabled = iconImage.sprite != null;
        }

        UpdateHp(building.hp, building.maxHp);

        BuildingUI.Instance?.ShowActions(building);
        CommandPanelUI.Instance?.ShowForBuilding(building);
    }

    public void Hide()
    {
        selectedId = -1;

        if (panel != null)
            panel.SetActive(false);

        BuildingUI.Instance?.HideActions();
        CommandPanelUI.Instance?.HideAll();
    }

    public void UpdateUnit(UnitDto unit)
    {
        if (selectedIsBuilding) return;
        if (unit.id != selectedId) return;

        UpdateHp(unit.hp, unit.maxHp);
    }

    public void UpdateBuilding(BuildingDto building)
    {
        if (!selectedIsBuilding) return;
        if (building.id != selectedId) return;

        UpdateHp(building.hp, building.maxHp);
        BuildingUI.Instance?.UpdateBuildingInfo(building);
    }

    private void UpdateHp(int hp, int maxHp)
    {
        if (maxHp <= 0) return;

        if (hpSlider != null)
        {
            hpSlider.gameObject.SetActive(true);
            hpSlider.minValue = 0;
            hpSlider.maxValue = maxHp;
            hpSlider.value = hp;
        }

        if (hpText != null)
            hpText.text = $"{hp} / {maxHp}";
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

    private string GetBuildingName(string type)
    {
        return type switch
        {
            "barracks" => "Barracks",
            "archery" => "Archery",
            "house" => "House",
            _ => type
        };
    }

    private Sprite GetUnitIcon(string type)
    {
        return type switch
        {
            "swordsman" => swordsmanIcon,
            "archer" => archerIcon,
            "worker" => workerIcon,
            _ => null
        };
    }

    private Sprite GetBuildingIcon(string type)
    {
        return type switch
        {
            "barracks" => barracksIcon,
            "archery" => archeryIcon,
            "house" => houseIcon,
            _ => null
        };
    }
}