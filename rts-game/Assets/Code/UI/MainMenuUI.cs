using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LobbyClient lobby;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject aiPanel;
    [SerializeField] private GameObject multiplayerPanel;
    [SerializeField] private GameObject authPanel;

    [Header("Multiplayer")]
    [SerializeField] private TMP_InputField lobbyIdInput;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Auth")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField usernameInput;

    [SerializeField] private GameObject loginButton;
    [SerializeField] private GameObject registerButton;

    [SerializeField] private TextMeshProUGUI authTitleText;
    [SerializeField] private TextMeshProUGUI authStatusText;
    [SerializeField] private TextMeshProUGUI switchModeText;
    
    [SerializeField] private GameObject profilePanel;
    
    private bool isRegisterMode = false;

    private void Awake()
    {
        if (lobby == null)
            lobby = FindFirstObjectByType<LobbyClient>();
    }

    private void OnEnable()
    {
        if (lobby == null) return;

        lobby.OnAuthSuccess += HandleAuthSuccess;
        lobby.OnAuthError += HandleAuthError;
        lobby.OnLobbyStatus += HandleLobbyStatus;
    }

    private void OnDisable()
    {
        if (lobby == null) return;

        lobby.OnAuthSuccess -= HandleAuthSuccess;
        lobby.OnAuthError -= HandleAuthError;
        lobby.OnLobbyStatus -= HandleLobbyStatus;
    }

    private void Start()
    {
        ShowOnly(mainPanel);

        if (authStatusText != null)
        {
            authStatusText.text = lobby != null && lobby.isAuthenticated
                ? "Logged in as " + lobby.username
                : "Not authenticated";
        }

        SetStatus(lobby != null && lobby.isAuthenticated
            ? "Logged in as " + lobby.username
            : "Please login first");
    }

    public void OpenAiPanel()
    {
        if (!IsLoggedIn()) return;
        ShowOnly(aiPanel);
    }

    public void OpenMultiplayerPanel()
    {
        if (!IsLoggedIn()) return;

        ShowOnly(multiplayerPanel);
        SetStatus("Multiplayer menu opened");
    }

    public void OpenAuthPanel()
    {
        ShowOnly(authPanel);
        SetLoginMode();
    }

    public void BackToMain()
    {
        ShowOnly(mainPanel);
    }

    public void SwitchAuthMode()
    {
        isRegisterMode = !isRegisterMode;

        if (isRegisterMode)
            SetRegisterMode();
        else
            SetLoginMode();
    }

    private void SetLoginMode()
    {
        isRegisterMode = false;

        if (authTitleText != null)
            authTitleText.text = "LOGIN";

        if (usernameInput != null)
            usernameInput.gameObject.SetActive(false);

        if (loginButton != null)
            loginButton.SetActive(true);

        if (registerButton != null)
            registerButton.SetActive(false);

        if (switchModeText != null)
            switchModeText.text = "Create account";

        if (authStatusText != null)
            authStatusText.text = "Enter email and password";
    }

    private void SetRegisterMode()
    {
        isRegisterMode = true;

        if (authTitleText != null)
            authTitleText.text = "REGISTER";

        if (usernameInput != null)
            usernameInput.gameObject.SetActive(true);

        if (loginButton != null)
            loginButton.SetActive(false);

        if (registerButton != null)
            registerButton.SetActive(true);

        if (switchModeText != null)
            switchModeText.text = "Already have account";

        if (authStatusText != null)
            authStatusText.text = "Create new account";
    }

    public void Login()
    {
        if (lobby == null)
        {
            SetAuthStatus("LobbyClient not found");
            return;
        }

        string email = emailInput != null ? emailInput.text.Trim() : "";
        string password = passwordInput != null ? passwordInput.text : "";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            SetAuthStatus("Email and password are required");
            return;
        }

        SetAuthStatus("Logging in...");
        lobby.Login(email, password);
    }

    public void Register()
    {
        if (lobby == null)
        {
            SetAuthStatus("LobbyClient not found");
            return;
        }

        string username = usernameInput != null ? usernameInput.text.Trim() : "";
        string email = emailInput != null ? emailInput.text.Trim() : "";
        string password = passwordInput != null ? passwordInput.text : "";

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            SetAuthStatus("Username, email and password are required");
            return;
        }

        if (password.Length < 6)
        {
            SetAuthStatus("Password must be at least 6 characters");
            return;
        }

        SetAuthStatus("Registering...");
        lobby.Register(username, email, password);
    }

    public void Logout()
    {
        if (lobby != null)
            lobby.Logout();

        SetStatus("Logged out");
        SetAuthStatus("Not authenticated");
        ShowOnly(mainPanel);
    }

    public void CreateLobby()
    {
        if (!IsLoggedIn()) return;

        SetStatus("Creating lobby...");
        lobby.CreateLobby();
    }

    public void JoinLobby()
    {
        if (!IsLoggedIn()) return;

        string lobbyId = lobbyIdInput != null ? lobbyIdInput.text.Trim() : "";

        if (string.IsNullOrWhiteSpace(lobbyId))
        {
            SetStatus("Enter Lobby ID");
            return;
        }

        SetStatus("Joining lobby: " + lobbyId);
        lobby.JoinLobby(lobbyId);
    }

    public void FindMatch()
    {
        if (!IsLoggedIn()) return;

        SetStatus("Searching match...");
        
    }

    private void StartAiGame(string difficulty)
    {
        if (!IsLoggedIn()) return;

        PlayerPrefs.SetString("game_mode", "ai");
        PlayerPrefs.SetString("ai_difficulty", difficulty);
        PlayerPrefs.Save();

        SceneManager.LoadScene("SampleScene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private bool IsLoggedIn()
    {
        if (lobby != null && lobby.isAuthenticated)
            return true;

        SetStatus("Please login first");
        ShowOnly(authPanel);
        SetLoginMode();
        return false;
    }

    private void HandleAuthSuccess(AuthSuccessMsg msg)
    {
        SetAuthStatus("Logged in as " + msg.username);
        SetStatus("Authenticated as " + msg.username);
        ShowOnly(mainPanel);
    }

    private void HandleAuthError(string message)
    {
        SetAuthStatus(message);
    }

    private void HandleLobbyStatus(string message)
    {
        SetStatus(message);
    }

    private void ShowOnly(GameObject panel)
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (aiPanel != null) aiPanel.SetActive(false);
        if (multiplayerPanel != null) multiplayerPanel.SetActive(false);
        if (authPanel != null) authPanel.SetActive(false);

        if (panel != null) panel.SetActive(true);
        if (profilePanel != null) profilePanel.SetActive(false);
    }

    public void OpenProfilePanel()
    {
        if (!IsLoggedIn()) return;

        ShowOnly(profilePanel);
        SetStatus("Profile opened");
    }
    
    private void SetStatus(string text)
    {
        if (statusText != null)
            statusText.text = text;
    }

    private void SetAuthStatus(string text)
    {
        if (authStatusText != null)
            authStatusText.text = text;
    }
    
    public void SetReady()
    {
        if (!IsLoggedIn()) return;

        SetStatus("Ready");
        lobby.SetReady(true);
    }
    
    public void StartAiEasy()
    {
        StartAIMatch("easy");
    }

    public void StartAiNormal()
    {
        StartAIMatch("normal");
    }

    public void StartAiHard()
    {
        StartAIMatch("hard");
    }

    private void StartAIMatch(string difficulty)
    {
        if (!IsLoggedIn()) return;

        SetStatus("Starting AI match: " + difficulty);

        if (lobby == null)
            lobby = FindFirstObjectByType<LobbyClient>();

        if (lobby != null)
            lobby.CreateAIMatch(difficulty);
    }
}