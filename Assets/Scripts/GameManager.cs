using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
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

    public bool IsGameActive { get; set; }
    
    private static GameManager instance;

    public Text scoreText;
    public Transform[] spawnPositions;
    public Goalpost[] goalposts;
    public Color[] playerColors;
    public GameObject playerPrefab;
    public GameObject ballPrefab;

    private Dictionary<ulong, int> playerScores;

    private void Start()
    {
        SpawnPlayer();

        IsGameActive = true;

        if (NetworkManager.IsServer)
        {
            SpawnBall();
        }
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
            
            var goalPost = goalposts[i];
            goalPost.OwnerId = client.ClientId;

            playerControl.SpawnToPositionClientRpc(spawnPosition.position);
            playerControl.SetActiveControlClientRpc(true);
        }
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
    }

    [ClientRpc]
    private void UpdateScoreTextClientRpc(int player1Score, int player2Score)
    {
        scoreText.text = $"{player1Score} : {player2Score}";
    }
}