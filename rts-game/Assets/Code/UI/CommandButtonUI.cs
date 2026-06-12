using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image icon;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>(true);

        if (icon == null)
            icon = GetComponentInChildren<Image>(true);
    }

    public void Setup(string text, Sprite sprite, UnityEngine.Events.UnityAction action)
    {
        gameObject.SetActive(true);

        if (button == null)
            button = GetComponent<Button>();

        if (label != null)
        {
            label.text = text;
            label.enableWordWrapping = true;
        }

        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            if (action != null)
                button.onClick.AddListener(action);

            button.interactable = true;
        }
    }

    public void Setup(string text, UnityEngine.Events.UnityAction action)
    {
        Setup(text, null, action);
    }

    public void Hide()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();

        gameObject.SetActive(false);
    }
}