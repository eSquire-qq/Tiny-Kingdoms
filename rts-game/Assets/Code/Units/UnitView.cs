using UnityEngine;

public class UnitView : MonoBehaviour
{
    public int owner;
    public int hp;
    public int maxHp;
    public int Id { get; private set; }

    [Header("Refs")]
    [SerializeField] private EntityId entityId;
    [SerializeField] private HealthBarScript healthBar;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animation")]
    [SerializeField] private float moveThreshold = 0.0001f;

    private Vector3 lastPosition;

    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private static readonly int AttackHash = Animator.StringToHash("attack");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (entityId == null)
            entityId = GetComponent<EntityId>();

        if (entityId == null)
            entityId = GetComponentInParent<EntityId>();

        if (healthBar == null)
            healthBar = GetComponentInChildren<HealthBarScript>();

        lastPosition = transform.position;
    }

    private void Update()
    {
        Vector3 delta = transform.position - lastPosition;
        bool isMoving = delta.sqrMagnitude > moveThreshold;

        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetBool(IsMovingHash, isMoving);

        if (spriteRenderer != null && Mathf.Abs(delta.x) > 0.001f)
            spriteRenderer.flipX = delta.x < 0f;

        lastPosition = transform.position;
    }

    public void Bind(int id)
    {
        Id = id;

        if (entityId != null)
            entityId.Set(id);
    }

    public void ApplyServerPos(float x, float y)
    {
        transform.position = new Vector3(x, y, 0f);
    }

    public void ApplyHp(int newHp, int newMaxHp)
    {
        hp = newHp;
        maxHp = newMaxHp;

        if (healthBar != null)
            healthBar.SetHealth(hp, maxHp);
    }

    public void PlayAttack()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetTrigger(AttackHash);
    }

    public int GetId()
    {
        if (entityId == null) return -1;
        return entityId.Id;
    }
}