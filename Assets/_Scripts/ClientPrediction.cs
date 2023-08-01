using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPrediction : MonoBehaviour
{



    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;

    private const float SERVER_TICK_RATE = 30f;
    // Start is called before the first frame update
    void Start()
    {
        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        while(timer >= minTimeBetweenTicks)
        {

            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }
    }


    void HandleTick()
    {

    }
}
