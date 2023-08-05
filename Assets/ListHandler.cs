using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
struct LobbyDisplayData : INetworkSerializable
{
    private float _x, _y, _z;
    internal Vector3 Position
    {
        get => new Vector3(_x, _y, _z);
        set
        {
            _x = value.x;
            _y = value.y;
            _z = value.z;
        }
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _x);
        serializer.SerializeValue(ref _y);
        serializer.SerializeValue(ref _z);
    }
}
public struct LobbyPlayer
{

    public ulong clientID;
    public string name;
    public string selectedCharacter;
    public GameObject lobbyDisplay;
    public float yValue;

    public LobbyPlayer(ulong p, string v1, string v2, GameObject go, float yValue) : this()
    {
        this.clientID = p;
        this.name = v1;
        this.selectedCharacter = v2;
        this.lobbyDisplay = go;
        this.yValue = yValue;
    }
    public LobbyPlayer(ulong p, string v1, string v2) : this()
    {
        this.clientID = p;
        this.selectedCharacter = v1;
        this.name = v2;
    }
}









public class ListHandler : NetworkBehaviour
{


    //private GameObject undecidedList;
    //private GameObject babyList;
    //private GameObject dadList;

    private LobbyPlayer currentPlayer;
    private string selectedPlayer = "Undecided";
    public GameObject displayPrefab;
    private float yValue;

    private GameObject displayGo;
    private Vector3 undecidedV;
    private Vector3 babyListV;
    private Vector3 dadListV;
    private bool initialize = false;

    public List<LobbyPlayer> playersInList;
    private NetworkVariable<Vector3> displayPosition = new(writePerm: NetworkVariableWritePermission.Owner);

    
    public void AddLobbyPlayer(ulong clientId, string name, string selectedCharacter)
    {
        print("AAAA");
        
        if (!initialize) initializeVars();
        print(playersInList.Count);
        float tmpyValue = this.transform.position.y - 50 * playersInList.Count;
        undecidedV.y = tmpyValue;
        babyListV.y = tmpyValue;
        dadListV.y = tmpyValue;

        Vector3 newPosition = undecidedV;
        if (selectedCharacter == "Undecided") newPosition = undecidedV;
        if (selectedCharacter == "Baby") newPosition = babyListV;
        if (selectedCharacter == "Dad") newPosition = dadListV;

        displayGo = Instantiate(displayPrefab, newPosition, Quaternion.identity);
        displayGo.transform.SetParent(this.transform);

        

        LobbyPlayer tmpPlayer = new LobbyPlayer(clientId, name, "Undecided", displayGo, tmpyValue);
        TMP_Text textChild = displayGo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
        textChild.text = tmpPlayer.name;
        playersInList.Add(tmpPlayer);
        if (clientId == NetworkManager.Singleton.LocalClientId) currentPlayer = tmpPlayer;
    }


    private void updateLocation(ulong clientId, string selectedCharacter)
    {
        LobbyPlayer player = playersInList.Find(ni => ni.clientID == clientId);
        if (selectedCharacter.Equals("Undecided"))
        {
            player.lobbyDisplay.transform.position = undecidedV;
        }
        if (selectedCharacter.Equals("Dad"))
        {
            player.lobbyDisplay.transform.position = dadListV;
        }
        if (selectedCharacter.Equals("Baby"))
        {
            player.lobbyDisplay.transform.position = babyListV;
        }
        player.selectedCharacter = selectedCharacter;
    }


    public void addPlaceholder()
    {
        yValue = this.transform.position.y - 50 * playersInList.Count;

        //transform.gameObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
        //print(dadListV);
        displayGo = Instantiate(displayPrefab, undecidedV, Quaternion.identity);
        //displayGo.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        //displayGo.GetComponent<NetworkObject>().AutoObjectParentSync = true;
        displayGo.transform.SetParent(this.transform);
        currentPlayer = new LobbyPlayer(NetworkManager.Singleton.LocalClientId, "Placeholder", selectedPlayer, displayGo, yValue);
        print("1");
        
        print("2");
        //print(displayGo.transform.parent.ToString());
        
        //print(displayGo.transform.parent.ToString());
        print("3");

        TMP_Text textChild = displayGo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
        textChild.text = currentPlayer.name;
        playersInList.Add(currentPlayer);
    }

    // Start is called before the first frame update
    void initializeVars()
    {
        playersInList = new List<LobbyPlayer>();

        yValue = this.transform.position.y - 50 * playersInList.Count;
        undecidedV = new Vector3(this.transform.position.x, yValue, this.transform.position.z);
        babyListV = new Vector3(this.transform.position.x - 350, yValue, this.transform.position.z);
        dadListV = new Vector3(this.transform.position.x + 350, yValue, this.transform.position.z);
        //AddLobbyPlayerServerRpc(NetworkManager.Singleton.LocalClientId, "Placeholder");
        //this.GetComponent<NetworkObject>().Spawn();
        //undecidedList = this.transform.GetChild(0).gameObject;
        //babyList = this.transform.GetChild(1).gameObject;
        //dadList = this.transform.GetChild(2).gameObject;
        initialize = true;
    }



    public void updateListCS(ulong clientId, string selectedCharacter)
    {
        print("Updating list for " + clientId.ToString() + " becoming: " + selectedCharacter);
        for(int i = 0; i< playersInList.Count; i++)
        {
            if(playersInList[i].clientID == clientId)
            {
                LobbyPlayer tmpPlayer = playersInList[i];
                tmpPlayer.selectedCharacter = selectedCharacter;
                playersInList[i] = tmpPlayer;
                updatePosition(playersInList[i]);
                return;
            }
        }
    }

    public void updatePosition(LobbyPlayer lobPLayer)
    {
        Vector3 newPosition = undecidedV;

        if(lobPLayer.selectedCharacter == "Undecided")newPosition = undecidedV;
        if(lobPLayer.selectedCharacter == "Baby")newPosition = babyListV;
        if(lobPLayer.selectedCharacter == "Dad")newPosition = dadListV;

        newPosition.y = lobPLayer.yValue;
        lobPLayer.lobbyDisplay.transform.position = newPosition;

    }
    public void MoveLeft()
    {
        print("MoveLeft");
        if (currentPlayer.selectedCharacter.Equals("Undecided"))
        {
            babyListV.y = currentPlayer.yValue;
            currentPlayer.lobbyDisplay.transform.position = babyListV;
            currentPlayer.selectedCharacter = "Baby";
        }
        if (currentPlayer.selectedCharacter.Equals("Dad"))
        {
            undecidedV.y = currentPlayer.yValue;
            currentPlayer.lobbyDisplay.transform.position = undecidedV;
            currentPlayer.selectedCharacter = "Undecided";
        }
        FindAnyObjectByType<LobbyLogic>().updatePosition(currentPlayer.clientID, currentPlayer.selectedCharacter);
    }

    public void MoveRight()
    {
        print("MoveRight");
        if (currentPlayer.selectedCharacter.Equals("Undecided"))
        {
            dadListV.y = currentPlayer.yValue;
            print("MOVIN TO: " + dadListV.ToString());
            
            currentPlayer.lobbyDisplay.transform.position = dadListV;
            currentPlayer.selectedCharacter = "Dad";
        }
        if (currentPlayer.selectedCharacter.Equals("Baby"))
        {
            undecidedV.y = currentPlayer.yValue;
            currentPlayer.lobbyDisplay.transform.position = undecidedV;
            currentPlayer.selectedCharacter = "Undecided";
        }
        FindAnyObjectByType<LobbyLogic>().updatePosition(currentPlayer.clientID, currentPlayer.selectedCharacter);
    }
}
