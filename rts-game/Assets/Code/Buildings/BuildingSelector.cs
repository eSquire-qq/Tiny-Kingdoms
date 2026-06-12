using UnityEngine;

public class BuildingSelector : MonoBehaviour
{
    public Camera mainCamera;
    public SelectedBuildingUI selectedUI;

    private BuildingView selectedBuilding;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 wp = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            wp.z = 0f;

            RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);
            if (hit.collider != null)
            {
                BuildingView building = hit.collider.GetComponentInParent<BuildingView>();
                if (building != null)
                {
                    selectedBuilding = building;
                    selectedUI.Show(building);
                    return;
                }
            }

            // Якщо клікнули по порожньому місцю
            selectedBuilding = null;
            selectedUI.Hide();
        }
    }
}