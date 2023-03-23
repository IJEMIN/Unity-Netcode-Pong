using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    // Minimum needed count of Player who is ready
    private const int MinimumPlayerCount = 2;
    
    public Text lobbyText;

    private Dictionary<ulong, bool> _clientsInLobbyReadyStateDictionary;

    public override void OnNetworkSpawn()
    {
        _clientsInLobbyReadyStateDictionary = new Dictionary<ulong, bool>();
        
        // 먼저 자기 자신을 등록
        _clientsInLobbyReadyStateDictionary.Add(NetworkManager.LocalClientId, false);

        // 만약 우리가 서버(호스트)라면, 클라이언트들이 접속했는지 콜백을 통해 감지하고 관리해야함
        if (IsServer)
        {
            //Server will be notified when a client connects or disconnects
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnectedCallback;

            // 서버는 OnLoadComplete를 모든 클라이언트들에 대해서 듣는다는 것에 주의!
            NetworkManager.SceneManager.OnLoadComplete += OnClientSceneLoadComplete;
        }

        //Update our lobby
        UpdateLobbyText();
    }

    private void OnClientSceneLoadComplete(ulong clientid, string scenename, LoadSceneMode loadscenemode)
    {
        if (IsServer)
        {
            if (!_clientsInLobbyReadyStateDictionary.ContainsKey(clientid))
            {
                _clientsInLobbyReadyStateDictionary.Add(clientid, false);
                UpdateLobbyText();
            }

            UpdateAndCheckPlayersInLobby();
        }
    }


    private void UpdateLobbyText()
    {
        var stringBuilder = new StringBuilder();
        foreach (var clientLobbyStatusPair in _clientsInLobbyReadyStateDictionary)
        {
            var clientId = clientLobbyStatusPair.Key;
            var isReady = clientLobbyStatusPair.Value;

            if (isReady)
            {
                stringBuilder.AppendLine($"PLAYER_{clientId} : READY");    
            }
            else
            {
                stringBuilder.AppendLine($"PLAYER_{clientId} : NOT READY");
            }
        }
        
        lobbyText.text = stringBuilder.ToString();
    }

    /// <summary>
    ///     UpdateAndCheckPlayersInLobby
    ///     Checks to see if we have at least 2 or more people to start
    /// </summary>
    private void UpdateAndCheckPlayersInLobby()
    {
        var enoughPlayer = _clientsInLobbyReadyStateDictionary.Count >= MinimumPlayerCount;
        var allReady = true;
        foreach (var clientReadyStatePair in _clientsInLobbyReadyStateDictionary)
        {
            var clientId = clientReadyStatePair.Key;
            var isReady = clientReadyStatePair.Value;
            
            SendClientReadyStatusUpdatesClientRpc(clientId, isReady);

            if (!isReady)
            {
                allReady = false;
            }
        }

        if (enoughPlayer && allReady)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.SceneManager.OnLoadComplete -= OnClientSceneLoadComplete;
            NetworkManager.SceneManager.LoadScene("InGame", LoadSceneMode.Single);
        }
    }

   

    /// <summary>
    ///     OnClientConnectedCallback
    ///     Since we are entering a lobby and Netcode's NetworkManager is spawning the player,
    ///     the server can be configured to only listen for connected clients at this stage.
    /// </summary>
    /// <param name="clientId">client that connected</param>
    private void OnClientConnectedCallback(ulong clientId)
    {
        if (IsServer)
        {
            if (!_clientsInLobbyReadyStateDictionary.ContainsKey(clientId))
            {
                _clientsInLobbyReadyStateDictionary.Add(clientId, false);
            }

            UpdateLobbyText();
            UpdateAndCheckPlayersInLobby();
        }
    }

    private void OnClientDisconnectedCallback(ulong clientId)
    {
        if (IsServer)
        {
            if (_clientsInLobbyReadyStateDictionary.ContainsKey(clientId))
            {
                _clientsInLobbyReadyStateDictionary.Remove(clientId);
                UpdateLobbyText();
            }
        }
    }

    [ClientRpc]
    private void SendClientReadyStatusUpdatesClientRpc(ulong clientId, bool isReady)
    {
        if (IsServer)
        {
            return;
        }
        
        _clientsInLobbyReadyStateDictionary[clientId] = isReady;
        UpdateLobbyText();
    }


    /// <summary>
    ///     PlayerIsReady
    ///     Tied to the Ready button in the InvadersLobby scene
    /// </summary>
    public void PlayerIsReady()
    {
        _clientsInLobbyReadyStateDictionary[NetworkManager.Singleton.LocalClientId] = true;
        if (IsServer)
        {
            UpdateAndCheckPlayersInLobby();
        }
        else
        {
            OnClientIsReadyServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        UpdateLobbyText();
    }

    /// <summary>
    ///     OnClientIsReadyServerRpc
    ///     Sent to the server when the player clicks the ready button
    /// </summary>
    /// <param name="clientid">clientId that is ready</param>
    [ServerRpc(RequireOwnership = false)]
    private void OnClientIsReadyServerRpc(ulong clientid)
    {
        if (_clientsInLobbyReadyStateDictionary.ContainsKey(clientid))
        {
            _clientsInLobbyReadyStateDictionary[clientid] = true;
            UpdateAndCheckPlayersInLobby();
            UpdateLobbyText();
        }
    }
}