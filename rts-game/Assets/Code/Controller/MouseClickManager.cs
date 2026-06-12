using UnityEngine;
using UnityEngine.EventSystems;

public class MouseClickManager : MonoBehaviour
{
    private Camera cam;
    private UnitsClientWorld unitsWorld;
    private BuildingsClientWorld buildingsWorld;

    private void Awake()
    {
        cam = Camera.main;
        unitsWorld = FindFirstObjectByType<UnitsClientWorld>();
        buildingsWorld = FindFirstObjectByType<BuildingsClientWorld>();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (cam == null)
            cam = Camera.main;

        if (cam == null) return;

        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);

        if (CommandPanelUI.Instance != null && CommandPanelUI.Instance.HasCommandMode())
        {
            bool executed = CommandPanelUI.Instance.ExecuteCommand(mousePos, hits);
            if (executed) return;
        }

        if (hits == null || hits.Length == 0)
        {
            SelectionInfoUI.Instance?.Hide();
            return;
        }

        foreach (var hit in hits)
        {
            var unit = hit.GetComponentInParent<UnitView>();
            if (unit != null)
            {
                if (unitsWorld != null && unitsWorld.TryGetUnitDto(unit.GetId(), out var unitDto))
                {
                    SelectionInfoUI.Instance?.ShowUnit(unitDto);
                    return;
                }
            }
        }

        foreach (var hit in hits)
        {
            var buildingClick = hit.GetComponentInParent<BuildingClickHandler>();
            if (buildingClick != null)
            {
                if (buildingsWorld != null && buildingsWorld.TryGetBuildingDto(buildingClick.buildingId, out var buildingDto))
                {
                    SelectionInfoUI.Instance?.ShowBuilding(buildingDto);
                    return;
                }
            }
        }

        SelectionInfoUI.Instance?.Hide();
    }
}