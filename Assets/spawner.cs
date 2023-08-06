using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class spawner : NetworkBehaviour
{
    [SerializeField] GameObject baby;
    [SerializeField] GameObject dad;
    [SerializeField] TMP_Text SelectedCharacter;
    public List<GameObject> Players;
    public int charId;
    [SerializeField] GameObject UI_Lobby;
    [SerializeField] GameObject menuCam;


    public void spawnPlayers(ulong clientId, string selectedCharacter)
    {
        if (IsClient)
        {
            
            if (selectedCharacter == "Dad")
            {
                print("DAD SPAWNING");
                charId = 1;
                serverSpawnServerRpc(NetworkManager.Singleton.LocalClientId, 1);
            }
            else
            {
                print("Baby SPAWNING");
                charId = 0;
                serverSpawnServerRpc(NetworkManager.Singleton.LocalClientId, 0);
            }
            UI_Lobby.SetActive(false);
            menuCam.SetActive(false);
        }
        if (IsServer)
        {
            FindAnyObjectByType<GameLogic>().startTimer();
        }
    }



    public override void OnNetworkSpawn()
    {
        return;
        if (IsClient)
        {
            if(SelectedCharacter.text == "Dad")
            {
                print("DAD SPAWNING");
                charId = 1;
                serverSpawnServerRpc(NetworkManager.Singleton.LocalClientId, 1);
            }
            else
            {
                print("Baby SPAWNING");
                charId = 0;
                serverSpawnServerRpc(NetworkManager.Singleton.LocalClientId, 0);
            }

        }

    }

    [ServerRpc]
    public void DespawnAllServerRpc()
    {
        DespawnClientRpc();

        List<GameObject> allPlayers = new List<GameObject>();

        allPlayers.AddRange(GameObject.FindGameObjectsWithTag("Dad"));
        allPlayers.AddRange(GameObject.FindGameObjectsWithTag("Baby"));
        

        
        foreach (GameObject player in allPlayers)
        {

            //player.GetComponent<NetworkObject>().Despawn();
            Destroy(player);
        }
        
    }


    [ClientRpc]
    public void DespawnClientRpc()
    {
        List<GameObject> allPlayers = new List<GameObject>();

        allPlayers.AddRange(GameObject.FindGameObjectsWithTag("Dad"));
        allPlayers.AddRange(GameObject.FindGameObjectsWithTag("Baby"));
        
        foreach (GameObject player in allPlayers)
        {
            print(player.tag);
            print("Despawning");
            //player.GetComponent<NetworkObject>().Despawn();
            Destroy(player);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void serverSpawnServerRpc(ulong clientId, int charID)
    {
        if (baby == null || dad == null)
        {
            return;
        }

        // Instantiate the GameObject Instance
        GameObject m_PrefabInstance = null;
        if (charID == 0)
        {
            m_PrefabInstance = Instantiate(baby, Vector3.zero, Quaternion.identity);
        }
        else if (charID == 1)
        {
            Vector3 dadSpawn = new Vector3(4.0f, 0.0f, 5.0f);
            m_PrefabInstance = Instantiate(dad, dadSpawn, Quaternion.identity);
        }
        
        // Optional, this example applies the spawner's position and rotation to the new instance
        //m_PrefabInstance.transform.position = transform.position;
        //m_PrefabInstance.transform.rotation = transform.rotation;
        // Get the instance's NetworkObject and Spawn
        NetworkObject m_SpawnedNetworkObject = m_PrefabInstance.GetComponent<NetworkObject>();
        //m_SpawnedNetworkObject.SpawnAsPlayerObject(clientId);

        //Players.Add(m_PrefabInstance);
        m_SpawnedNetworkObject.SpawnWithOwnership(clientId);

    }
}
