using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

using TMPro;
public class CharacterSelect : MonoBehaviour
{
    [SerializeField] TMP_Text CurrentCharacter;
    private bool baby = true;
    [SerializeField] NetworkManager netMan;
    [SerializeField] GameObject babyPrefab;
    [SerializeField] GameObject dadPrefab;


    public void SwitchCharacter()
    {
        print(baby);
        baby = !baby;
        if (baby)
        {
            CurrentCharacter.text = "Dad";
            //NetworkManager.Singleton.GetNetworkPrefabOverride(dadPrefab);
            
        }
        else
        {
            CurrentCharacter.text = "Baby";
            //NetworkManager.Singleton.GetNetworkPrefabOverride(babyPrefab);
            
        }
        
    }
}
