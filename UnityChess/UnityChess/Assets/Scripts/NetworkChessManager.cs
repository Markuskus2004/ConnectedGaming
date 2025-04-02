using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityChess;

public class NetworkChessManager : MonoBehaviour
{
    // UI References
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Text resultText;

    // Network sync reference
    private NetworkChessSync networkSync;

    // Game Manager Reference
    private GameManager gameManager;
    private NetworkManager networkManager;

    private void Start()
    {
        // Find game managers
        gameManager = GameManager.Instance;

        // Find the NetworkManager
        networkManager = GetComponent<NetworkManager>();
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
            Debug.Log("Found NetworkManager through FindObjectOfType: " + (networkManager != null));
        }

        // Set up button listeners
        if (hostButton != null) hostButton.onClick.AddListener(HostGame);
        if (joinButton != null) joinButton.onClick.AddListener(JoinGame);
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameClicked);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(OnLoadGameClicked);
    }

    // Button click handlers
    private void HostGame()
    {
        Debug.Log("Host button clicked");

        if (networkManager == null)
        {
            Debug.LogError("Cannot host: NetworkManager not found!");
            if (resultText != null) resultText.text = "Error: Cannot host game";
            return;
        }

        // Start as host
        networkManager.StartHost();
        Debug.Log("Started as Host - Playing as White");

        if (resultText != null)
        {
            resultText.text = "Hosting Game - Playing as White";
        }

        // Find the network sync component
        networkSync = FindObjectOfType<NetworkChessSync>();
        if (networkSync != null && networkManager.IsServer)
        {
            // Initialize White player
            networkSync.WhitePlayerClientId.Value = networkManager.LocalClientId;
        }
    }

    private void JoinGame()
    {
        Debug.Log("Join button clicked");

        if (networkManager == null)
        {
            Debug.LogError("Cannot join: NetworkManager not found!");
            if (resultText != null) resultText.text = "Error: Cannot join game";
            return;
        }

        // Start as client
        networkManager.StartClient();
        Debug.Log("Started as Client - Playing as Black");

        if (resultText != null)
        {
            resultText.text = "Joined Game - Playing as Black";
        }
    }

    private void OnNewGameClicked()
    {
        if (networkManager != null && networkManager.IsConnectedClient)
        {
            Debug.Log("Requesting new game over network");
            // Notify other players that a new game is starting (would need to implement this)
        }
        else if (gameManager != null)
        {
            gameManager.StartNewGame();
        }
    }

    private void OnLoadGameClicked()
    {
        if (networkManager != null && networkManager.IsConnectedClient)
        {
            Debug.Log("Loading games not supported in multiplayer mode");

            if (resultText != null)
            {
                resultText.text = "Loading games not supported in multiplayer";
            }
        }
    }
}