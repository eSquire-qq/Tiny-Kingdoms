using UnityEngine;

public sealed class SelectionBoxView : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform boxRect;

    private Vector2 startLocal;

    private void Awake()
    {
        boxRect.anchorMin = Vector2.zero;
        boxRect.anchorMax = Vector2.zero;
        boxRect.pivot = Vector2.zero;
        boxRect.gameObject.SetActive(false);
    }

    public void Begin(Vector2 startScreenPos)
    {
        boxRect.gameObject.SetActive(true);
        startLocal = ScreenToCanvasLocal(startScreenPos);
        UpdateBox(startScreenPos);
    }

    public void UpdateBox(Vector2 currentScreenPos)
    {
        Vector2 currentLocal = ScreenToCanvasLocal(currentScreenPos);
        Vector2 min = Vector2.Min(startLocal, currentLocal);
        Vector2 max = Vector2.Max(startLocal, currentLocal);
        boxRect.anchoredPosition = min;
        boxRect.sizeDelta = max - min;
    }

    public void End()
    {
        boxRect.gameObject.SetActive(false);
    }

    private Vector2 ScreenToCanvasLocal(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out var localPoint);
        return localPoint + canvasRect.rect.size * 0.5f;
    }
}