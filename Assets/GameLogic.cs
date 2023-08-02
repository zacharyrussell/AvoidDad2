using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameLogic : NetworkBehaviour
{
    [SerializeField] GameObject gameOver;
    // Start is called before the first frame update



    [ServerRpc]
    public void BabyCaughtServerRpc()
    {
        //gameOver.SetActive(true);
        BabyCaughtClientRpc();
    }

    [ClientRpc]
    public void BabyCaughtClientRpc()
    {
        print("Calling on Client");
        gameOver.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        FindObjectOfType<spawner>().DespawnAllServerRpc();
    }

    public void mainMenu()
    {
        FindObjectOfType<PasswordNetworkManager>().Leave();
    }

    public void playAgain()
    {
        gameOver.SetActive(false);
        spawner spawn = FindObjectOfType<spawner>();
        int charId = spawn.charId;
        spawn.serverSpawnServerRpc(NetworkManager.Singleton.LocalClientId, charId);
    }



}
