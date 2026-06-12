using System.Collections.Generic;
public sealed class CommandQueue
{
    private readonly Queue<ICommand> queue = new();
    
    public void Enqueue(ICommand cmd) => queue.Enqueue(cmd);
    
    public bool TryDequeue(out ICommand cmd)
    {
        if (queue.Count > 0)
        {
            cmd = queue.Dequeue();
            return true;
        }

        cmd = null;
        return false;
    }
}