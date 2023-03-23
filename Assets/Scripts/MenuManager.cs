using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Text _infoText;
    public InputField _hostAddressInputField;
    private const ushort DefaultPort = 7777;

    private void Awake()
    {
        _infoText.text = string.Empty;
    }

    // set max player in session as 2 
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count < 2)
        {
            response.Approved = true;
        }
        else
        {
            response.Approved = false;
            response.Reason = "Max player in session is 2";
        }
    }
    public void CreateGameAsHost()
    {
        var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        transport.ConnectionData.Port = DefaultPort;

        // set max player in session as 2
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = false; 
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

        if (NetworkManager.Singleton.StartHost())
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        }
        else
        {
            _infoText.text = "Host failed to start";
            Debug.LogError("Host failed to start");
        }
    }

    public void JoinGameAsClient()
    {
        var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        transport.SetConnectionData(_hostAddressInputField.text, DefaultPort);
        
        if (!NetworkManager.Singleton.StartClient())
        {
            _infoText.text = "Client failed to start";
            Debug.LogError("Client failed to start");
        }
    }
}
