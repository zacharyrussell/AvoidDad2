using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class LobbyLogic : NetworkBehaviour
{

    private List<ulong> clientList;
    private List<LobbyPlayer> lobbyPlayers;
    [SerializeField] GameObject lobbyUI;
    [SerializeField] TMP_InputField nameField;
    [SerializeField] GameObject joinOverlay;
    [SerializeField] GameObject hostOverlay;

    private string PlayerName = "DefaultName";



    public void updateName()
    {
        PlayerName = nameField.text;
    }


    public override void OnNetworkSpawn()
    {
        clientList = new List<ulong>();
        lobbyPlayers = new List<LobbyPlayer>();
        if (IsClient)
        {
            lobbyUI.SetActive(true);
            SpawnLobbyPlayerServerRpc(NetworkManager.Singleton.LocalClientId, PlayerName);
            //FindAnyObjectByType<ListHandler>().AddLobbyPlayer(NetworkManager.Singleton.LocalClientId, "Placeholder");
        }
    }



    public void startGame()
    {
        startGameServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    public void startGameServerRpc()
    {
        startGameClientRpc();
    }


    [ClientRpc]
    public void startGameClientRpc()
    {
        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            if (lobbyPlayers[i].clientID == NetworkManager.Singleton.LocalClientId)
            {
                print(lobbyPlayers[i].selectedCharacter);
                FindAnyObjectByType<spawner>().spawnPlayers(lobbyPlayers[i].clientID, lobbyPlayers[i].selectedCharacter);
                return;
            }
        }

    }




    public void updatePosition(ulong clientId, string selectedCharacter)
    {
        updatePositionServerRpc(clientId, selectedCharacter);
    }


    [ServerRpc(RequireOwnership = false)]
    public void updatePositionServerRpc(ulong clientId, string selectedCharacter)
    {
        for(int i = 0; i<lobbyPlayers.Count; i++)
        {
            if(lobbyPlayers[i].clientID == clientId)
            {
                LobbyPlayer tmpPlayer = lobbyPlayers[i];
                tmpPlayer.selectedCharacter = selectedCharacter;
                lobbyPlayers[i] = tmpPlayer;
                break ;
            }
        }
        updatePositionClientRpc(clientId, selectedCharacter);
        
    }

    [ClientRpc]
    public void updatePositionClientRpc(ulong clientId, string selectedCharacter)
    {
        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            if (lobbyPlayers[i].clientID == clientId)
            {
                LobbyPlayer tmpPlayer = lobbyPlayers[i];
                tmpPlayer.selectedCharacter = selectedCharacter;
                lobbyPlayers[i] = tmpPlayer;
                break;
            }
        }
        if (clientId == NetworkManager.Singleton.LocalClientId) return;
        FindAnyObjectByType<ListHandler>().updateListCS(clientId, selectedCharacter);
    }





    [ServerRpc(RequireOwnership = false)]
    public void SpawnLobbyPlayerServerRpc(ulong clientId, string Pname)
    {
        List<ulong> playersToSpawn = new(NetworkManager.Singleton.ConnectedClientsIds);
        lobbyPlayers.Add(new LobbyPlayer(clientId, "Undecided", Pname));
        //Sync joined players

        for(int i = 0; i< lobbyPlayers.Count; i++)
        {
            SpawnLobbyPlayerClientRpc(lobbyPlayers[i].clientID, lobbyPlayers[i].name, lobbyPlayers[i].selectedCharacter);
        }

        //foreach(LobbyPlayer p in lobbyPlayers)
        //{
        //    SpawnLobbyPlayerClientRpc(p.clientID, p.name, p.selectedCharacter);
        //}
        //foreach(ulong id in playersToSpawn)
        //{
        //    SpawnLobbyPlayerClientRpc(id, Pname);
        //}

    }

    [ClientRpc]
    public void SpawnLobbyPlayerClientRpc(ulong clientId, string Pname, string selectedCharacter)
    {
        foreach (ulong id in clientList)
        {
            if (id == clientId) return;
        }

        FindAnyObjectByType<ListHandler>().AddLobbyPlayer(clientId, Pname, selectedCharacter);
        clientList.Add(clientId);
        lobbyPlayers.Add(new LobbyPlayer(clientId, selectedCharacter, Pname));
    }



    public void enableJoinOverlay()
    {
        joinOverlay.SetActive(true);

    }
    public void disableJoinOverlay()
    {
        joinOverlay.SetActive(false);
    }

    public void enableHostOverlay()
    {
        hostOverlay.SetActive(true);

    }
    public void disableHostOverlay()
    {
        hostOverlay.SetActive(false);
    }

}
