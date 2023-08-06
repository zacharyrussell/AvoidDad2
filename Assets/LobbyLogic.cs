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

    [ServerRpc(RequireOwnership = false)]
    public void removePlayerServerRpc(ulong clientId)
    {
        //print("Removeing Player");
        //print("Old Count: " + lobbyPlayers.Count.ToString());
        int indexToRemove = 0;
        for (int i = 0; i< lobbyPlayers.Count; i++)
        {
            if(lobbyPlayers[i].clientID == clientId)
            {
                //remove from list
                print("FOUND");
                indexToRemove = i;
                break;
                //lobbyPlayers.Remove(lobbyPlayers[i]);
            }
        }
        lobbyPlayers.RemoveAt(indexToRemove);
        //lobbyPlayers = new List<LobbyPlayer>();
        //print("New Count: " + lobbyPlayers.Count.ToString());
        RemovePlayerClientRpc(clientId);
    }



    public void removePlayerLocal()
    {
        clientList = new List<ulong>();
        lobbyPlayers = new List<LobbyPlayer>();
    }

    [ClientRpc]
    public void RemovePlayerClientRpc(ulong clientId)
    {
        print("removing");
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            //this player left
            clientList = new List<ulong>();
            lobbyPlayers = new List<LobbyPlayer>();
        }

        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            if (lobbyPlayers[i].clientID == clientId)
            {
                //remove from list
                lobbyPlayers.Remove(lobbyPlayers[i]);
            }
        }

    }


    public override void OnNetworkSpawn()
    {

        
        if (IsClient)
        {
            clientList = new List<ulong>();
            lobbyPlayers = new List<LobbyPlayer>();
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
        if(lobbyPlayers == null)
        {
            clientList = new List<ulong>();
            lobbyPlayers = new List<LobbyPlayer>();
        }
        
        List<ulong> playersToSpawn = new(NetworkManager.Singleton.ConnectedClientsIds);
        if (IsServer)
        {
            lobbyPlayers.Add(new LobbyPlayer(clientId, "Undecided", Pname));
        }
        
        //Sync joined players

        for(int i = 0; i< lobbyPlayers.Count; i++)
        {
            SpawnLobbyPlayerClientRpc(lobbyPlayers[i].clientID, lobbyPlayers[i].name, lobbyPlayers[i].selectedCharacter);
        }

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
        if (!IsServer)
        {
            lobbyPlayers.Add(new LobbyPlayer(clientId, selectedCharacter, Pname));
        }
        
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
