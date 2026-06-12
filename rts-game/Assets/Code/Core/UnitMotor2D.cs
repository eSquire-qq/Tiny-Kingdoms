using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class UnitMotor2D : MonoBehaviour
{
    
    [SerializeField] private float moveSpeed = 3.5f;
    private Rigidbody2D rb;
    private Vector2 target;
    private bool hasTarget;
    public Vector2 Target => target;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }
    public void SetMoveTarget(Vector2 worldPoint)
    {
        target = worldPoint;
        hasTarget = true;
    }
    public void Stop()
    {
        hasTarget = false;
        rb.linearVelocity = Vector2.zero;
    }
    
    private void FixedUpdate()
    {
        if (!hasTarget) return;

        Vector2 pos = rb.position;
        Vector2 to = target - pos;

        if (to.sqrMagnitude < 0.02f)
        {
            rb.MovePosition(target);
            Stop();
            return;
        }
        Vector2 step = to.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(pos + step);
    }
}
