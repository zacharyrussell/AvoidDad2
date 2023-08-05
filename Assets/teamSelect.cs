using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Unity.Netcode;


//public struct LobbyPlayer{

//    public ulong clientID;
//    public string name;
//    public string selectedCharacter;
//    public GameObject lobbyDisplay;
//    public float yValue;

//    public LobbyPlayer(ulong p, string v1, string v2, GameObject go, float yValue) : this()
//    {
//        this.clientID = p;
//        this.name = v1;
//        this.selectedCharacter = v2;
//        this.lobbyDisplay = go;
//        this.yValue = yValue;
//    }
//}

public class teamSelect : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject prefab;
    public List<LobbyPlayer> playersInList;


    void Start()
    {
        playersInList = new List<LobbyPlayer>();
    }
    public void AddPlayer(LobbyPlayer lobPlayer)
    {
        Vector3 position = new Vector3(this.transform.position.x, this.transform.position.y + -50*playersInList.Count, this.transform.position.z);
        GameObject go = Instantiate(lobPlayer.lobbyDisplay, position, Quaternion.identity);
        go.transform.parent = transform;

        //LobbyPlayer curPlayer = new(NetworkManager.Singleton.LocalClientId, "Fake Player", "sf", go);
        playersInList.Add(lobPlayer);
        TMP_Text textChild = go.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
        textChild.text = lobPlayer.name;
        //GameObject child = originalGameObject.transform.GetChild(0).gameObject;
        //go.transform.position.y = playersInList.Count;
    }


    public void RemovePlayer(LobbyPlayer currentPlayer)
    {
        for(int i = 0; i < playersInList.Count; i++)
        {
            if(playersInList[i].clientID == currentPlayer.clientID)
            {
                DestroyImmediate(playersInList[i].lobbyDisplay, true);
                playersInList.Remove(playersInList[i]);
                shiftList(i);
            }
        }
    }


    private void shiftList(int i)
    {
        for (int j = i; j < playersInList.Count; j++)
        {
            print(j + 1 - i);
            GameObject curPlayer = playersInList[j].lobbyDisplay;
            curPlayer.transform.position = new Vector3(curPlayer.transform.position.x, curPlayer.transform.position.y + 50, curPlayer.transform.position.z);
        }

    }
    // Update is called once per frame
    void Update()
    {
        //print(playersInList[0].name);
    }
}
