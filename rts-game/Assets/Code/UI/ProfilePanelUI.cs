using System.Text;
using TMPro;
using UnityEngine;

public class ProfilePanelUI : MonoBehaviour
{
    [SerializeField] private LobbyClient lobby;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI emailText;
    [SerializeField] private TextMeshProUGUI ratingText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI historyText;

    private void Awake()
    {
        if (lobby == null)
            lobby = FindFirstObjectByType<LobbyClient>();
    }

    private void OnEnable()
    {
        if (lobby != null)
        {
            lobby.OnProfileReceived += ShowProfile;
            lobby.RequestProfile();
        }
    }

    private void OnDisable()
    {
        if (lobby != null)
            lobby.OnProfileReceived -= ShowProfile;
    }

    private void ShowProfile(ProfileMsg profile)
    {
        if (profile == null)
            return;

        if (nicknameText != null)
            nicknameText.text = "Нік: " + profile.username;

        if (emailText != null)
            emailText.text = "Email: " + profile.email;

        if (ratingText != null)
            ratingText.text = "Рейтинг: " + profile.rating;

        if (statsText != null)
        {
            statsText.text =
                "Матчів: " + profile.totalMatches + "\n" +
                "Перемог: " + profile.wins + "\n" +
                "Поразок: " + profile.losses + "\n" +
                "Нічиїх: " + profile.draws + "\n" +
                "Win Rate: " + profile.winRate + "%\n" +
                "Золота зібрано: " + profile.totalGoldCollected + "\n" +
                "Дерева зібрано: " + profile.totalLumberCollected + "\n" +
                "Юнітів створено: " + profile.totalUnitsCreated + "\n" +
                "Будівель збудовано: " + profile.totalBuildingsBuilt;
        }

        if (historyText != null)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Історія матчів:");

            if (profile.matches == null || profile.matches.Length == 0)
            {
                sb.AppendLine("Історія матчів порожня");
            }
            else
            {
                foreach (var match in profile.matches)
                {
                    sb.AppendLine(
                        match.startedAt + " | " +
                        match.sessionType + " | " +
                        match.mode + " | " +
                        match.difficulty + " | " +
                        match.result
                    );
                }
            }

            historyText.text = sb.ToString();
        }
    }
}