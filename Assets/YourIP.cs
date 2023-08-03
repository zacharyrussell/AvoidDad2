using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;

public class YourIP : MonoBehaviour
{

    [SerializeField] TMP_Text text;
    // Start is called before the first frame update
    void Start()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        print(host.AddressList[0]);
        text.text = "Your IP: " + host.AddressList[0].ToString();
    }

    // Update is called once per frame
}
