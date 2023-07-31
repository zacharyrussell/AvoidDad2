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

    public GameObject menu;

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
            leaveButton.SetActive(true);

            //GameObject go = Instantiate(baby, Vector3.zero, Quaternion.identity);
            //go.SetActive(true);
            //go.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId, true);
            //SpawnServerRPC(NetworkManager.Singleton.LocalClientId);
            //server RPC to spawn player

            //if (NetworkManager.Singleton.IsClient) SpawnPlayerServerRpc(clientId, 0);
        }
       
    }


    //[ServerRpc(RequireOwnership = false)] //server owns this object but client can request a spawn

    public void SpawnPlayerServerRpc(ulong clientId, int prefabId)
    {
        GameObject newPlayer;
        if (prefabId == 0)
            newPlayer = (GameObject)Instantiate(baby, Vector3.zero, Quaternion.identity);
        else
            newPlayer = (GameObject)Instantiate(baby, Vector3.zero, Quaternion.identity);
        var netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            menu.SetActive(true);
            leaveButton.SetActive(false);
            passwordEntryUI.SetActive(true);
            
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
            //if(SelectedCharacter.text == "Dad")
            //{

            //    response.PlayerPrefabHash = 1557686693;
            //}
            //else
            //{
            //    response.PlayerPrefabHash = 3030887073;
            //}
            return;
        }
        // Additional connection data defined by user code
        var connectionData = request.Payload;
        string password = Encoding.ASCII.GetString(connectionData);
        bool approval = password == passwordInput.text;
        //response.CreatePlayerObject = true;
        //if (SelectedCharacter.text == "Dad")
        //{

        //    response.PlayerPrefabHash = 1557686693;
        //}
        //else
        //{
        //    response.PlayerPrefabHash = 3030887073;
        //}
        response.CreatePlayerObject = false;
        response.Approved = approval;
    }

}

