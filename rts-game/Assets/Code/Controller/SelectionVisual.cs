using UnityEngine;
public sealed class SelectionVisual : MonoBehaviour
{
    [SerializeField] private GameObject indicator;

    private void Awake()
    {
        SetSelected(false);
    }

    public void SetSelected(bool value)
    {
        if (indicator != null)
            indicator.SetActive(value);
    }
}