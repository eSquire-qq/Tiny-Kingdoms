using UnityEngine;

public interface ICommand
{
    int EntityId { get; }
}

public readonly struct MoveCommand : ICommand
{
    public int EntityId { get; }
    public Vector2 Target { get; }

    public MoveCommand(int entityId, Vector2 target)
    {
        EntityId = entityId;
        Target = target;
    }
}

public readonly struct AttackCommand : ICommand
{
    public int EntityId { get; }
    public int TargetId { get; }

    public AttackCommand(int entityId, int targetId)
    {
        EntityId = entityId;
        TargetId = targetId;
    }
}