using UnityEngine;

public class BuildUI : MonoBehaviour
{
    public LobbyClient client;

    private bool isPlacing = false;
    private string buildingType;

    public void BuildBarracks()
    {
        StartPlacing("barracks");
    }

    public void BuildArchery()
    {
        StartPlacing("archery");
    }

    public void BuildHouse()
    {
        StartPlacing("house");
    }

    void StartPlacing(string type)
    {
        isPlacing = true;
        buildingType = type;
    }

    void Update()
    {
        if (!isPlacing) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = GetMouseWorldPosition();

            client.SendBuildCommand(buildingType, pos.x, pos.z);

            isPlacing = false;
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }
}