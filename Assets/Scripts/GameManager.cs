using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameManager>();

            return instance;
        }
    }
    
    private static GameManager instance;
    
    public bool IsGameActive { get; set; }

    public Text scoreText;
    public Transform[] spawnPositions;
    public Goalpost[] goalposts;
    public GameObject ballPrefab;

    private const int WinScore = 10;
    public Text gameoverText;
    public GameObject gameoverPanel;

    // only used on server
    private Dictionary<ulong, int> playerScores = new();

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.IsServer)
        {
            SpawnPlayer();
            SpawnBall();
            
            foreach (var clientsId in NetworkManager.ConnectedClientsIds)
            {
                playerScores[clientsId] = 0;
            }
        }

        IsGameActive = true;
        
        gameoverPanel.SetActive(false);
        UpdateScoreTextClientRpc(0, 0);
    }
    
    private void SpawnPlayer()
    {
        var clientsList = NetworkManager.ConnectedClientsList;

        // Pong only can be played by 2 players 
        if (clientsList.Count != 2)
        {
            Debug.LogError("Pong can only be played by 2 players...");
            return;
        }
        
        for (var i = 0; i < clientsList.Count; i++)
        {
            var client = clientsList[i];
            var playerControl = client.PlayerObject.GetComponent<PlayerControl>();
            var spawnPosition = spawnPositions[i];
            
            playerControl.SpawnToPositionClientRpc(spawnPosition.position);
            playerControl.SetRenderActiveClientRpc(true);

        }

        goalposts[0].OwnerId = clientsList[0].ClientId;
        goalposts[0].OpponentId = clientsList[1].ClientId;
        
        goalposts[1].OwnerId = clientsList[1].ClientId;
        goalposts[1].OpponentId = clientsList[0].ClientId;
    }

    private void SpawnBall()
    {
        var ballGameObject = Instantiate(ballPrefab, Vector2.zero, Quaternion.identity);
        var ball = ballGameObject.GetComponent<Ball>();
        ball.NetworkObject.Spawn();
    }

    public void AddScore(ulong playerId, int score)
    {
        playerScores[playerId] += score;
        var scores = playerScores.Values.ToArray();
        
        var player1Score = scores[0];
        var player2Score = scores[1];
        
        UpdateScoreTextClientRpc(player1Score, player2Score);

        if (playerScores[playerId] >= WinScore)
        {
            EndGame(playerId);
        }
    }

    [ClientRpc]
    private void UpdateScoreTextClientRpc(int player1Score, int player2Score)
    {
        scoreText.text = $"{player1Score} : {player2Score}";
    }
    
    public void EndGame(ulong winnerId)
    {
        if (!IsServer)
        {
            return;
        }

        foreach (var networkClient in NetworkManager.ConnectedClientsList)
        {
            var playerControl = networkClient.PlayerObject.GetComponent<PlayerControl>();
            playerControl.SetRenderActiveClientRpc(false);
        }
        
        EndGameClientRpc(winnerId);
    }
    
    [ClientRpc]
    public void EndGameClientRpc(ulong winnerId)
    {
        IsGameActive = false;
        if (winnerId == NetworkManager.LocalClientId)
        {
            gameoverText.text = "You Win!";
        }
        else
        {
            gameoverText.text = "You Lose!";
        }
        
        gameoverPanel.SetActive(true);
    }
    
    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("Menu");
    }
}