using UnityEngine;

public class ResourceView : MonoBehaviour
{
    public int resourceId;
    public string resourceType;

    public void Bind(int id, string type)
    {
        resourceId = id;
        resourceType = type;
        name = $"Resource_{id}_{type}";
    }
}