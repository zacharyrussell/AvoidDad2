using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Server : NetworkBehaviour
{


    public static Server Instance;

    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 30f;
    private const int BUFFER_SIZE = 1024;

    private StatePayload[] stateBuffer;
    private Queue<InputPayload> inputQueue;

    void Awake()
    {
        Instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;
        stateBuffer = new StatePayload[BUFFER_SIZE];
        inputQueue = new Queue<InputPayload>();


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

    private void HandleTick()
    {
        int bufferIndex = -1;
        while(inputQueue.Count > 0)
        {
            InputPayload inputPayload = inputQueue.Dequeue();
            bufferIndex = inputPayload.tick % BUFFER_SIZE;
            StatePayload statePayload = ProcessMovement(inputPayload);
            stateBuffer[bufferIndex] = statePayload;
        }

        if(bufferIndex != -1)
        {
            SendToClient_ClientRpc(stateBuffer[bufferIndex]);
        }
    }


    [ClientRpc]
    void SendToClient_ClientRpc(StatePayload statePayload)
    {
        //yield return new WaitForSeconds(0.02f);

        Client.Instance.OnServerMovementState(statePayload);
    }

    StatePayload ProcessMovement(InputPayload input)
    {
        transform.position += input.inputVector * 5f * minTimeBetweenTicks;
        return new StatePayload()
        {
            tick = input.tick,
            position = transform.position,
        };
    }

    public void OnClientInput(InputPayload inputPayload)
    {
        inputQueue.Enqueue(inputPayload);
    }

}
