using System.Collections.Generic;
using UnityEngine;

public sealed class GameSimulation : MonoBehaviour
{
    private readonly CommandQueue commandQueue = new();
    
    private readonly Dictionary<int, UnitMotor2D> unitsById = new();
    public CommandQueue Commands => commandQueue;

    private void Awake()
    {
        foreach (var motor in FindObjectsOfType<UnitMotor2D>())
        {
            var id = motor.GetComponent<EntityId>();
            if (id == null)
            {
                Debug.LogError($"Unit {motor.name} has no EntityId");
                continue;
            }
            
            unitsById[id.Id] = motor;
        }
    }

    private void Update()
    {
        while (commandQueue.TryDequeue(out var cmd))
        {
            Apply(cmd);
        }
    }
    private void Apply(ICommand cmd)
    {
        if (cmd is MoveCommand move)
        {
            if (unitsById.TryGetValue(move.EntityId, out var motor))
            {
                // Передає ціль
                motor.SetMoveTarget(move.Target);
            }
        }
    }
}
