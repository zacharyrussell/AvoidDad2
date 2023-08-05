using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports;
using System;
using System.Text;
using Unity.Networking.Transport;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class PasswordNetworkManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField AddressInput;
    [SerializeField] private GameObject passwordEntryUI;
    [SerializeField] private GameObject leaveButton;
    [SerializeField] private UnityTransport Transport;
    [SerializeField] private TMP_Text SelectedCharacter;
    [SerializeField] private GameObject baby;
    [SerializeField] private GameObject dad;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private GameObject lobby;

    public GameObject menu;

    [ServerRpc(RequireOwnership = false)]
    public void BabyCaughtServerRpc()
    {
        gameOver.SetActive(true);
        BabyCaughtClientRpc();
    }

    [ClientRpc]
    public void BabyCaughtClientRpc()
    {
        print("Calling on Client");
        gameOver.SetActive(true);
    }


    public void Host()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartHost();
         }
    }

    public void UpdateIPAddress()
    {
        Transport.ConnectionData.Address = AddressInput.text;
    }

    public void Client()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(passwordInput.text);
        NetworkManager.Singleton.StartClient();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Leave();
        }
    }

    public void Leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
        menu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        leaveButton.SetActive(false);
        passwordEntryUI.SetActive(true);
        gameOver.SetActive(false);
    }

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;        
    }

    private void HandleServerStarted()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HandleClientConnected(NetworkManager.Singleton.LocalClientId);
        }
    }


    private void HandleClientConnected(ulong clientId)
    {
        
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {

            menu.SetActive(false);
            passwordEntryUI.SetActive(false);
            //lobby.SetActive(true);

            //if (NetworkManager.Singleton.IsClient) {
            //    print("CCC");
            //    FindAnyObjectByType<LobbyLogic>().spawnLobbyPlayer(); }

            leaveButton.SetActive(true);
        }
       
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            menu.SetActive(true);
            leaveButton.SetActive(false);
            lobby.SetActive(true);
            passwordEntryUI.SetActive(true);
            gameOver.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (!NetworkManager.Singleton) return;
        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
    }
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            response.Approved = true;
            response.CreatePlayerObject = false;
            return;
        }
        // Additional connection data defined by user code
        var connectionData = request.Payload;
        string password = Encoding.ASCII.GetString(connectionData);
        bool approval = password == passwordInput.text;
        response.CreatePlayerObject = false;
        response.Approved = approval;
    }

}

