using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class LobbyCreatedMsg
{
    public string type;
    public string lobbyId;
}

[Serializable]
public class MatchStartMsg
{
    public string type;
    public string matchId;
}

[Serializable]
public class HelloMsg
{
    public string type;
    public int playerId;
}

[Serializable]
public class AuthSuccessMsg
{
    public string type;
    public int playerId;
    public string username;
    public string email;
    public string accessToken;
    public string refreshToken;
}

[Serializable]
public class AuthErrorMsg
{
    public string type;
    public string message;
}

[Serializable]
public class ProfileMatchDto
{
    public string sessionType;
    public string mode;
    public string difficulty;
    public string result;
    public string startedAt;
    public int durationSeconds;
}

[Serializable]
public class ProfileMsg
{
    public string type;
    public string username;
    public string email;
    public int rating;
    public int totalMatches;
    public int wins;
    public int losses;
    public int draws;
    public float winRate;
    public int totalPlayTimeSeconds;
    public int totalGoldCollected;
    public int totalLumberCollected;
    public int totalUnitsCreated;
    public int totalBuildingsBuilt;
    public ProfileMatchDto[] matches;
}

public class LobbyClient : MonoBehaviour
{
    [SerializeField] private TopBarUI topBarUI;

    [Header("Network")]
    public NetClient net;

    [Header("Test inputs")]
    public string lobbyIdToJoin = "";
    public int moveUnitId = 1;
    public float moveX = 10;
    public float moveY = 3;

    [Header("State")]
    public int myPlayerId;
    public string username;
    public string email;
    public string accessToken;
    public string refreshToken;

    public string lobbyId;
    public string matchId;
    public bool inMatch;
    public bool isAuthenticated;

    public ResourcesClientWorld resourcesWorld;
    public UnitsClientWorld world;
    public BuildingsClientWorld buildingsWorld;
    public PlayerUI playerUI;
    
    private static LobbyClient instance;
    public event Action<AuthSuccessMsg> OnAuthSuccess;
    public event Action<string> OnAuthError;
    public event Action<string> OnLobbyStatus;
    
    public event Action<ProfileMsg> OnProfileReceived;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        MainThreadDispatcher.EnsureExists();

        if (net == null)
            net = GetComponent<NetClient>();

        if (world == null)
            world = FindFirstObjectByType<UnitsClientWorld>();

        if (buildingsWorld == null)
            buildingsWorld = FindFirstObjectByType<BuildingsClientWorld>();

        if (resourcesWorld == null)
            resourcesWorld = FindFirstObjectByType<ResourcesClientWorld>();

        if (playerUI == null)
            playerUI = FindFirstObjectByType<PlayerUI>();
    }

    private void Start()
    {
        Connect();

        accessToken = PlayerPrefs.GetString("accessToken", "");
        refreshToken = PlayerPrefs.GetString("refreshToken", "");
        myPlayerId = PlayerPrefs.GetInt("playerId", 0);
        username = PlayerPrefs.GetString("username", "");
        email = PlayerPrefs.GetString("email", "");

        isAuthenticated = !string.IsNullOrEmpty(accessToken) && myPlayerId > 0;
    }

    private void OnEnable()
    {
        if (net == null) return;

        net.OnLine += HandleLine;
        net.OnConnected += OnConnected;
        net.OnDisconnected += OnDisconnected;
        
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (net == null) return;

        net.OnLine -= HandleLine;
        net.OnConnected -= OnConnected;
        net.OnDisconnected -= OnDisconnected;
        
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnConnected()
    {
        Debug.Log("LobbyClient: connected");
        OnLobbyStatus?.Invoke("Connected to server");
    }

    private void OnDisconnected(string reason)
    {
        Debug.Log("LobbyClient: disconnected " + reason);

        inMatch = false;
        matchId = null;
        lobbyId = null;

        OnLobbyStatus?.Invoke("Disconnected: " + reason);
    }

    public void Connect()
    {
        if (net == null) return;
        net.Connect();
    }

    public void RequestProfile()
    {
        if (!CheckAuth()) return;

        SendLine("{\"type\":\"profile_request\"}");
    }
    
    public void Register(string usernameInput, string emailInput, string passwordInput)
    {
        string json =
            "{\"type\":\"register\"" +
            ",\"username\":\"" + Escape(usernameInput) + "\"" +
            ",\"email\":\"" + Escape(emailInput) + "\"" +
            ",\"password\":\"" + Escape(passwordInput) + "\"" +
            "}";

        SendLine(json);
    }

    public void Login(string emailInput, string passwordInput)
    {
        string json =
            "{\"type\":\"login\"" +
            ",\"email\":\"" + Escape(emailInput) + "\"" +
            ",\"password\":\"" + Escape(passwordInput) + "\"" +
            "}";

        SendLine(json);
    }

    public void Logout()
    {
        isAuthenticated = false;
        myPlayerId = 0;
        username = "";
        email = "";
        accessToken = "";
        refreshToken = "";

        PlayerPrefs.DeleteKey("playerId");
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.DeleteKey("email");
        PlayerPrefs.DeleteKey("accessToken");
        PlayerPrefs.DeleteKey("refreshToken");
        PlayerPrefs.Save();
    }

    public void CreateLobby()
    {
        if (!CheckAuth()) return;
        SendLine("{\"type\":\"create_lobby\"}");
    }

    public void JoinLobby(string id)
    {
        if (!CheckAuth()) return;
        SendLine("{\"type\":\"join_lobby\",\"lobbyId\":\"" + Escape(id) + "\"}");
    }

    public void SetReady(bool ready)
    {
        if (!CheckAuth()) return;
        SendLine("{\"type\":\"set_ready\",\"ready\":" + (ready ? "true" : "false") + "}");
    }

    public void CmdMove(int unitId, float x, float y)
    {
        string xs = x.ToString(CultureInfo.InvariantCulture);
        string ys = y.ToString(CultureInfo.InvariantCulture);

        SendLine("{\"type\":\"cmd_move\",\"unitId\":" + unitId +
                 ",\"x\":" + xs +
                 ",\"y\":" + ys + "}");
    }

    public void CmdTrainUnit(int buildingId, string unitType)
    {
        SendLine("{\"type\":\"cmd_train_unit\",\"buildingId\":" + buildingId +
                 ",\"unitType\":\"" + Escape(unitType) + "\"}");
    }

    public void CmdAttack(int unitId, int targetId)
    {
        SendLine("{\"type\":\"cmd_attack\",\"unitId\":" + unitId + ",\"targetId\":" + targetId + "}");
    }

    public void CmdAttackBuilding(int unitId, int targetId)
    {
        SendLine("{\"type\":\"cmd_attack_building\",\"unitId\":" + unitId + ",\"targetId\":" + targetId + "}");
    }

    public void CmdStop(int unitId)
    {
        SendLine("{\"type\":\"cmd_stop\",\"unitId\":" + unitId + "}");
    }

    public void CmdEndMatch()
    {
        SendLine("{\"type\":\"cmd_end_match\"}");
    }

    public void CmdGather(int unitId, int resourceId)
    {
        Debug.Log("CMD GATHER " + unitId + " -> " + resourceId);
        SendLine("{\"type\":\"cmd_gather\",\"unitId\":" + unitId + ",\"resourceId\":" + resourceId + "}");
    }

    public void SendBuildCommand(string type, float x, float y)
    {
        string xs = x.ToString(CultureInfo.InvariantCulture);
        string ys = y.ToString(CultureInfo.InvariantCulture);

        SendLine("{\"type\":\"cmd_build\",\"buildingType\":\"" + Escape(type) +
                 "\",\"x\":" + xs + ",\"y\":" + ys + "}");
    }

    public void SendBuild(string buildingType)
    {
        SendLine("{\"type\":\"cmd_build\",\"buildingType\":\"" + Escape(buildingType) + "\",\"x\":0,\"y\":0}");
    }

    private void SendLine(string json)
    {
        if (net == null) return;
        net.SendLine(json);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            CmdTrainUnit(1, "swordsman");

        if (Input.GetKeyDown(KeyCode.C))
            CreateLobby();

        if (Input.GetKeyDown(KeyCode.J))
            JoinLobby(lobbyIdToJoin);

        if (Input.GetKeyDown(KeyCode.R))
            SetReady(true);

        if (Input.GetKeyDown(KeyCode.M))
            CmdMove(moveUnitId, moveX, moveY);

        if (Input.GetKeyDown(KeyCode.E))
            CmdEndMatch();
    }

    private void HandleLine(string json)
    {
        if (json.Contains("\"type\":\"auth_success\""))
        {
            var msg = JsonUtility.FromJson<AuthSuccessMsg>(json);

            myPlayerId = msg.playerId;
            username = msg.username;
            email = msg.email;
            accessToken = msg.accessToken;
            refreshToken = msg.refreshToken;
            isAuthenticated = true;

            PlayerPrefs.SetInt("playerId", myPlayerId);
            PlayerPrefs.SetString("username", username);
            PlayerPrefs.SetString("email", email);
            PlayerPrefs.SetString("accessToken", accessToken);
            PlayerPrefs.SetString("refreshToken", refreshToken);
            PlayerPrefs.Save();

            if (playerUI != null)
                playerUI.myPlayerId = myPlayerId;

            topBarUI?.SetPlayerId(myPlayerId);

            Debug.Log("AUTH SUCCESS playerId=" + myPlayerId + " username=" + username);

            OnAuthSuccess?.Invoke(msg);
            OnLobbyStatus?.Invoke("Authenticated as " + username);
            return;
        }

        if (json.Contains("\"type\":\"profile\""))
        {
            var msg = JsonUtility.FromJson<ProfileMsg>(json);

            OnProfileReceived?.Invoke(msg);
            OnLobbyStatus?.Invoke("Profile loaded");

            return;
        }
        
        if (json.Contains("\"type\":\"auth_error\""))
        {
            var msg = JsonUtility.FromJson<AuthErrorMsg>(json);
            Debug.LogWarning("AUTH ERROR: " + msg.message);

            OnAuthError?.Invoke(msg.message);
            OnLobbyStatus?.Invoke(msg.message);
            return;
        }

        if (json.Contains("\"type\":\"state\""))
        {
            var msg = JsonUtility.FromJson<StateMsg>(json);

            if (msg != null)
            {
                if (myPlayerId == 0)
                {
                    Debug.Log("WAITING FOR PLAYER ID...");
                    return;
                }

                world?.ApplyState(msg);
                playerUI?.UpdateFromState(msg);
                resourcesWorld?.ApplyState(msg);
                buildingsWorld?.ApplyState(msg);
            }

            return;
        }

        if (json.Contains("\"type\":\"hello\""))
        {
            var msg = JsonUtility.FromJson<HelloMsg>(json);

            if (!isAuthenticated)
                myPlayerId = msg.playerId;

            if (playerUI != null)
                playerUI.myPlayerId = myPlayerId;

            Debug.Log("Hello temp/playerId = " + msg.playerId);
            return;
        }

        if (json.Contains("\"type\":\"lobby_created\""))
        {
            var msg = JsonUtility.FromJson<LobbyCreatedMsg>(json);

            lobbyId = msg.lobbyId;
            lobbyIdToJoin = lobbyId;

            Debug.Log("Lobby created id=" + lobbyId);
            OnLobbyStatus?.Invoke("Lobby created: " + lobbyId);
            return;
        }

        if (json.Contains("\"type\":\"lobby_state\""))
        {
            Debug.Log("Lobby state: " + json);
            OnLobbyStatus?.Invoke("Lobby updated");
            return;
        }

        if (json.Contains("\"type\":\"match_start\""))
        {
            var msg = JsonUtility.FromJson<MatchStartMsg>(json);

            matchId = msg.matchId;
            inMatch = true;

            PlayerPrefs.SetInt("playerId", myPlayerId);
            PlayerPrefs.SetString("matchId", matchId);
            PlayerPrefs.SetString("accessToken", accessToken);
            PlayerPrefs.SetString("refreshToken", refreshToken);
            PlayerPrefs.Save();

            Debug.Log("Match started id=" + matchId);
            OnLobbyStatus?.Invoke("Match started");

            SceneManager.LoadScene("SampleScene");
            return;
        }

        if (json.Contains("\"type\":\"match_end\""))
        {
            if (json.Contains("\"victory\""))
            {
                GameEndUI.Instance?.ShowVictory();
            }
            else if (json.Contains("\"defeat\""))
            {
                GameEndUI.Instance?.ShowDefeat();
            }

            inMatch = false;

            Debug.Log("MATCH ENDED");

            return;
        }
        
        if (json.Contains("\"type\":\"error\""))
        {
            Debug.LogWarning("Server error: " + json);

            if (json.Contains("not_enough_resources"))
                Debug.LogWarning("Недостатньо ресурсів");

            return;
        }

        Debug.Log("Unknown msg: " + json);
    }

    private bool CheckAuth()
    {
        if (isAuthenticated) return true;

        Debug.LogWarning("Not authenticated");
        OnLobbyStatus?.Invoke("Please login first");
        return false;
    }

    private string Escape(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }
    
    public void CreateAIMatch(string difficulty)
    {
        if (!CheckAuth()) return;

        difficulty = difficulty.ToLower();

        SendLine(
            "{\"type\":\"create_ai_match\",\"difficulty\":\"" +
            Escape(difficulty) +
            "\"}"
        );

        OnLobbyStatus?.Invoke("Starting AI match: " + difficulty);
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        world = FindFirstObjectByType<UnitsClientWorld>();
        buildingsWorld = FindFirstObjectByType<BuildingsClientWorld>();
        resourcesWorld = FindFirstObjectByType<ResourcesClientWorld>();
        playerUI = FindFirstObjectByType<PlayerUI>();

        if (playerUI != null)
            playerUI.myPlayerId = myPlayerId;

        var commandPanel = FindFirstObjectByType<CommandPanelUI>();
        if (commandPanel != null)
            commandPanel.SetLobby(this);

        var buildPlacement = FindFirstObjectByType<BuildPlacementManager>();
        if (buildPlacement != null)
            buildPlacement.SetLobby(this);

        Debug.Log("LobbyClient rebound scene refs. playerId=" + myPlayerId);
    }
    
}