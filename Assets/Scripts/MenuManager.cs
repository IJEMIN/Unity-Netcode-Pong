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

    public void CreateGameAsHost()
    {
        var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        transport.ConnectionData.Port = DefaultPort;
        
        if (NetworkManager.Singleton.StartHost())
        {
            NetworkManager.Singleton.SceneManager.LoadScene("InGame", LoadSceneMode.Single);
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
