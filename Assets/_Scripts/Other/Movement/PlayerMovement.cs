using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class NetworkSpawn : NetworkBehaviour
{
    [SerializeField] GameObject _camera;
    [SerializeField] GameObject Baby;

    public override void OnNetworkSpawn()
    {
        print(IsOwner);
        if (!IsOwner) return;
        Cursor.lockState = CursorLockMode.Locked;
        //Baby.SetActive(false);
        _camera.SetActive(true);
    }

}