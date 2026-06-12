using UnityEngine;
using UnityEngine.InputSystem;

public class RTSCameraController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float panZoomBase = 10f;
    [SerializeField] private float panSmooth = 20f;
    [SerializeField] private bool edgeScroll = true;
    [SerializeField] private int edgePx = 30;
    [SerializeField] private float edgeScrollPower = 1.2f;
    [SerializeField] private float edgeScrollGrace = 0.25f;
    [SerializeField] private bool disableEdgeScrollWhenMouseIsZero = true;
    [SerializeField] private float zoomStepPerTick = 1.5f;
    [SerializeField] private float zoomSmooth = 20f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 100f;
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 boundsMin = new(-50, -50);
    [SerializeField] private Vector2 boundsMax = new(50, 50);

    private Vector3 targetPos;
    private float targetZoom;
    private Vector2 lastMousePos;
    private float lastMouseMoveTime;

    private void Awake()
    {
        if (!cam) cam = Camera.main;
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        targetPos = transform.position;
        targetZoom = cam.orthographicSize;
        if (Mouse.current != null)
        {
            lastMousePos = Mouse.current.position.ReadValue();
            lastMouseMoveTime = Time.unscaledTime;
        }
    }

    private void LateUpdate()
    {
        if (!Application.isFocused) return;
        float dt = Time.unscaledDeltaTime;

        UpdateMouseMovementStamp();

        Vector2 move = ReadMove();
        if (edgeScroll && CanUseEdgeScroll()) move += ReadEdgeScroll();
        if (move.sqrMagnitude > 1f) move.Normalize();

        float zoomFactor = targetZoom / panZoomBase;
        targetPos += (Vector3)(move * panSpeed * zoomFactor * dt);

        HandleZoom();
        if (useBounds) targetPos = ClampToBounds(targetPos, targetZoom);

        transform.position = Smooth(transform.position, targetPos, panSmooth, dt);
        cam.orthographicSize = Smooth(cam.orthographicSize, targetZoom, zoomSmooth, dt);
    }

    private void UpdateMouseMovementStamp()
    {
        var m = Mouse.current; if (m == null) return;
        Vector2 p = m.position.ReadValue();
        if ((p - lastMousePos).sqrMagnitude > 0.01f) { lastMousePos = p; lastMouseMoveTime = Time.unscaledTime; }
    }

    private bool CanUseEdgeScroll()
    {
        var m = Mouse.current; if (m == null) return false;
        Vector2 p = m.position.ReadValue();
        if (disableEdgeScrollWhenMouseIsZero && p == Vector2.zero) return false;
        if (Time.unscaledTime - lastMouseMoveTime > edgeScrollGrace) return false;
        if (p.x < 0 || p.x > Screen.width || p.y < 0 || p.y > Screen.height) return false;
        return true;
    }

    private Vector2 ReadMove()
    {
        var k = Keyboard.current; if (k == null) return Vector2.zero;
        return new Vector2(
            (k.rightArrowKey.isPressed ? 1f : 0f) - (k.leftArrowKey.isPressed ? 1f : 0f),
            (k.upArrowKey.isPressed ? 1f : 0f) - (k.downArrowKey.isPressed ? 1f : 0f)
        );
    }

    private Vector2 ReadEdgeScroll()
    {
        Vector2 p = Mouse.current.position.ReadValue();
        float x = 0f, y = 0f;
        if (p.x <= edgePx) x = -Mathf.Pow(1f - p.x / edgePx, edgeScrollPower);
        if (p.x >= Screen.width - edgePx) x = Mathf.Pow(1f - (Screen.width - p.x) / edgePx, edgeScrollPower);
        if (p.y <= edgePx) y = -Mathf.Pow(1f - p.y / edgePx, edgeScrollPower);
        if (p.y >= Screen.height - edgePx) y = Mathf.Pow(1f - (Screen.height - p.y) / edgePx, edgeScrollPower);
        return new Vector2(x, y);
    }

    private void HandleZoom()
    {
        var m = Mouse.current; if (m == null) return;
        float raw = m.scroll.ReadValue().y; if (Mathf.Abs(raw) < 0.01f) return;
        float oldTarget = targetZoom;
        float ticks = raw / 120f;
        targetZoom = Mathf.Clamp(targetZoom - ticks * zoomStepPerTick, minZoom, maxZoom);
        if (Mathf.Abs(targetZoom - oldTarget) > 0.01f)
        {
            Vector2 mousePos = m.position.ReadValue();
            Vector2 normMouse = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
            Vector2 sizeDelta = new Vector2((oldTarget - targetZoom) * cam.aspect, oldTarget - targetZoom);
            targetPos.x -= sizeDelta.x * (normMouse.x - 0.5f) * 2f;
            targetPos.y -= sizeDelta.y * (normMouse.y - 0.5f) * 2f;
        }
    }

    private Vector3 ClampToBounds(Vector3 pos, float ortho)
    {
        float halfH = ortho;
        float halfW = ortho * cam.aspect;
        float minX = boundsMin.x + halfW;
        float maxX = boundsMax.x - halfW;
        float minY = boundsMin.y + halfH;
        float maxY = boundsMax.y - halfH;
        pos.x = (minX > maxX) ? (boundsMin.x + boundsMax.x) * 0.5f : Mathf.Clamp(pos.x, minX, maxX);
        pos.y = (minY > maxY) ? (boundsMin.y + boundsMax.y) * 0.5f : Mathf.Clamp(pos.y, minY, maxY);
        pos.z = 0f;
        return pos;
    }

    private static Vector3 Smooth(Vector3 from, Vector3 to, float sharpness, float dt) =>
        Vector3.Lerp(from, to, 1f - Mathf.Exp(-sharpness * dt));

    private static float Smooth(float from, float to, float sharpness, float dt) =>
        Mathf.Lerp(from, to, 1f - Mathf.Exp(-sharpness * dt));

    public void SetBounds(Vector2 min, Vector2 max)
    {
        boundsMin = min;
        boundsMax = max;
        targetPos = ClampToBounds(targetPos, targetZoom);
    }

    public void CenterOn(Vector3 worldPos)
    {
        targetPos = new Vector3(worldPos.x, worldPos.y, 0f);
    }
}