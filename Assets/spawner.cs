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

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            if(SelectedCharacter.text == "Dad")
            {
                print("DAD SPAWNING");
                serverSpawnServerRpc(NetworkManager.Singleton.LocalClientId, 1);
            }
            else
            {
                print("Baby SPAWNING");
                serverSpawnServerRpc(NetworkManager.Singleton.LocalClientId, 0);
            }

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
            m_PrefabInstance = Instantiate(dad, Vector3.zero, Quaternion.identity);
        }
        
        // Optional, this example applies the spawner's position and rotation to the new instance
        //m_PrefabInstance.transform.position = transform.position;
        //m_PrefabInstance.transform.rotation = transform.rotation;
        // Get the instance's NetworkObject and Spawn
        NetworkObject m_SpawnedNetworkObject = m_PrefabInstance.GetComponent<NetworkObject>();
        //m_SpawnedNetworkObject.SpawnAsPlayerObject(clientId);

        m_SpawnedNetworkObject.SpawnWithOwnership(clientId);

    }
}
