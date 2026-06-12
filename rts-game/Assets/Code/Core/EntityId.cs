using UnityEngine;

public sealed class EntityId : MonoBehaviour
{
    [SerializeField] private int id;
    public int Id => id;
    public void Set(int newId) => id = newId;
}