using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
public class BabyDistance : MonoBehaviour
{

    [SerializeField] TMP_Text distance;
    [SerializeField] GameObject dad;

    // Update is called once per frame
    void FixedUpdate()
    {

        if (GameObject.FindGameObjectsWithTag("Baby").Length != 0)
        {
            GameObject baby = GameObject.FindGameObjectsWithTag("Baby")[0];
            Vector3 babyPos = baby.transform.position;
            Vector3 dadPos = dad.transform.position;
            var distanceCalc = Vector3.Distance(babyPos, dadPos);
            int distanceCalcInt = (int)distanceCalc;
            distance.text = distanceCalcInt.ToString() + "m";
        }
        else
        {
            distance.text = "No baby located";
        }
    }
}
