using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public struct InputPayload : INetworkSerializable
{
    public int tick;
    public Vector3 inputVector;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref inputVector);

    }
}
public struct StatePayload : INetworkSerializable
{
    public int tick;
    public Vector3 position;



    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref position);

    }
}


public class Client : NetworkBehaviour
{
    public static Client Instance;

    //Shared
    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 30f;
    private const int BUFFER_SIZE = 1024;

    //CLIENT
    private StatePayload[] stateBuffer;
    private InputPayload[] inputBuffer;
    private StatePayload latestServerState;
    private StatePayload lastProccesedState;
    private float horizontalInput;
    private float verticalInput;


    // Start is called before the first frame update
    void Start()
    {
        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        stateBuffer = new StatePayload[BUFFER_SIZE];
        inputBuffer = new InputPayload[BUFFER_SIZE];
    }


    void HandleTick()
    {

        if(!latestServerState.Equals(default(StatePayload)) && (lastProccesedState.Equals(default(StatePayload)) || !latestServerState.Equals(lastProccesedState))){
            HandleServerReconciliation();
        }



        int bufferIndex = currentTick % BUFFER_SIZE;

        //Add payload to inputBuffer
        InputPayload inputPayload = new InputPayload();
        inputPayload.tick = currentTick;
        inputPayload.inputVector = new Vector3(horizontalInput, 0, verticalInput);

        // Add payload to stateBuffer
        stateBuffer[bufferIndex] = ProcessMovement(inputPayload);

        SendToServer_ServerRpc(inputPayload);
    }

    private void HandleServerReconciliation()
    {
        lastProccesedState = latestServerState;
        int ServerStateBufferIndex = latestServerState.tick % BUFFER_SIZE;
        float positionError = Vector3.Distance(latestServerState.position, stateBuffer[ServerStateBufferIndex].position);

        if(positionError > 0.001f)
        {
            Debug.Log("Reconciling!");

            // Rewind and replay
            transform.position = latestServerState.position;

            //Update buffer at index of latest server state
            stateBuffer[ServerStateBufferIndex] = latestServerState;

            // now resimulate the rest of the ticks in between current client and latest server tick
            int tickToProcess = latestServerState.tick + 1;

            while(tickToProcess < currentTick)
            {
                int bufferIndex = tickToProcess % BUFFER_SIZE;
                StatePayload statePayload = ProcessMovement(inputBuffer[bufferIndex]);
                stateBuffer[bufferIndex] = statePayload;
                tickToProcess++;
            }
        }
    }

    [ServerRpc]
    void SendToServer_ServerRpc(InputPayload inputPayload)
    {
        //yield return new WaitForSeconds(0.02f);
        Server.Instance.OnClientInput(inputPayload);

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

    public void OnServerMovementState(StatePayload serverState)
    {
        latestServerState = serverState;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        timer += Time.deltaTime;

        while(timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }
    }
}
