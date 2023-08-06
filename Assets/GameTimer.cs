using UnityEngine;
using System.Collections;
using TMPro;
public class GameTimer : MonoBehaviour
{

    public float targetTime = 65.0f;

    void Update()
    {

        targetTime -= Time.deltaTime;

        int minutes = (int)targetTime / 60;
        float seconds = targetTime - (minutes * 60);

        string secondString = seconds.ToString().Substring(0,4);
        if(seconds < 10 && minutes != 0)
        {
            secondString = "0" + secondString;
        }
        if (targetTime < 60)
        {
            this.GetComponent<TMP_Text>().color = Color.red;
        }
        if(minutes != 0)
        {
            secondString = secondString.Substring(0, 2);
            this.GetComponent<TMP_Text>().text = minutes.ToString() + ":" + secondString;
        }
        else
        {
            this.GetComponent<TMP_Text>().text = secondString;
        }



    }



}