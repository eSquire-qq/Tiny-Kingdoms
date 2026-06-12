using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndUI : MonoBehaviour
{
    public static GameEndUI Instance;

    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI resultText;

    private string lastResult = "";

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);
    }

    public void ShowVictory()
    {
        lastResult = "victory";

        if (root != null)
            root.SetActive(true);

        if (resultText != null)
            resultText.text = "VICTORY";
    }

    public void ShowDefeat()
    {
        lastResult = "defeat";

        if (root != null)
            root.SetActive(true);

        if (resultText != null)
            resultText.text = "DEFEAT";
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartMatch()
    {
        LobbyClient lobby = FindFirstObjectByType<LobbyClient>();

        if (lobby == null)
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        string difficulty = PlayerPrefs.GetString("ai_difficulty", "normal");

        SceneManager.LoadScene("SampleScene");

        lobby.CreateAIMatch(difficulty);
    }
}