using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LobbyLogic : NetworkBehaviour
{

    private List<ulong> clientList;
    private List<LobbyPlayer> lobbyPlayers;
    [SerializeField] GameObject lobbyUI; 

    public override void OnNetworkSpawn()
    {
        clientList = new List<ulong>();
        lobbyPlayers = new List<LobbyPlayer>();
        if (IsClient)
        {
            lobbyUI.SetActive(true);
            SpawnLobbyPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
            //FindAnyObjectByType<ListHandler>().AddLobbyPlayer(NetworkManager.Singleton.LocalClientId, "Placeholder");
        }
    }



    public void startGame()
    {
        print("CLicked button");
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
        print("In client RPC");
        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            print("AA");
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
        print("Updating");
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
        print("Updating client RPC");
        FindAnyObjectByType<ListHandler>().updateListCS(clientId, selectedCharacter);
    }





    [ServerRpc(RequireOwnership = false)]
    public void SpawnLobbyPlayerServerRpc(ulong clientId)
    {
        List<ulong> playersToSpawn = new(NetworkManager.Singleton.ConnectedClientsIds);
        lobbyPlayers.Add(new LobbyPlayer(clientId, "Undecided"));
        //Sync joined players
        foreach(ulong id in playersToSpawn)
        {
            SpawnLobbyPlayerClientRpc(id);
        }

    }

    [ClientRpc]
    public void SpawnLobbyPlayerClientRpc(ulong clientId)
    {
        foreach (ulong id in clientList)
        {
            if (id == clientId) return;
        }

        FindAnyObjectByType<ListHandler>().AddLobbyPlayer(clientId, "Placeholder");
        clientList.Add(clientId);
        lobbyPlayers.Add(new LobbyPlayer(clientId, "Undecided"));
    }
}
