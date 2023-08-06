using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class GameLogic : NetworkBehaviour
{
    [SerializeField] TMP_Text gameOverText;
    [SerializeField] GameObject gameOver;
    // Start is called before the first frame update
    private bool timerStarted = false;
    private float time = 150.0f;

    [ServerRpc]
    public void BabyCaughtServerRpc()
    {
        //gameOver.SetActive(true);
        BabyCaughtClientRpc();
    }


    [ServerRpc]
    public void BabyEvadedServerRpc()
    {
        //gameOver.SetActive(true);
        BabyEvadedClientRpc();
    }


    [ClientRpc]
    public void BabyEvadedClientRpc()
    {
        gameOver.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameOverText.text = "The baby has gotten the best of you!";
        FindObjectOfType<spawner>().DespawnAllServerRpc();
    }

    [ClientRpc]
    public void BabyCaughtClientRpc()
    {
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
    public void startTimer()
    {
        timerStarted = true;
    }

    private void Update()
    {
        if (timerStarted)
        {
            time -= Time.deltaTime;
            if(time <= 0)
            {
                BabyEvadedServerRpc();
            }
        }



    }

}
