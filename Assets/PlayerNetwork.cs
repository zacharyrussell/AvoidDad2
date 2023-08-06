using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] GameObject player;
    private readonly NetworkVariable<PlayerNetworkData> _netState = new(writePerm: NetworkVariableWritePermission.Owner);
    private Vector3 _vel;
    private float _rotVel;
    [SerializeField] private float _cheapInterpolationTime = 0.1f;
    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            PlayerState currentState = PlayerState.Idle;
            if(CompareTag("Dad")) {
                currentState = FindAnyObjectByType<DadMovement>().GetPlayerState();
            }
            else if (CompareTag("Baby"))
            {
                currentState = FindAnyObjectByType<BabyMovement>().GetPlayerState();
            }
            _netState.Value = new PlayerNetworkData()
            {
                Position = transform.position,
                Rotation = transform.rotation.eulerAngles,
                Animation = currentState
            };
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, _netState.Value.Position, ref _vel, _cheapInterpolationTime);
            transform.rotation = Quaternion.Euler(0, Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, _netState.Value.Rotation.y, ref _rotVel, _cheapInterpolationTime), 0);
            print("Calling State update");
            print(_netState.Value.Animation);
            if (CompareTag("Dad"))
            {
                FindAnyObjectByType<DadMovement>().SetPlayerState(_netState.Value.Animation);
            }
            else if (CompareTag("Baby"))
            {

                FindAnyObjectByType<BabyMovement>().SetPlayerState(_netState.Value.Animation);
            }
        }
    }






    struct PlayerNetworkData : INetworkSerializable
    {
        private float _x, _y, _z;
        private short _yRot;
        private byte state;

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


        internal PlayerState Animation
        {
            get => byteToState(state);

            set => state = StateToByte(value);
        }
        internal Vector3 Rotation
        {
            get => new Vector3(0, _yRot, 0);
            set => _yRot = (short)value.y;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _x);
            serializer.SerializeValue(ref _y);
            serializer.SerializeValue(ref _z);
            serializer.SerializeValue(ref _yRot);
            serializer.SerializeValue(ref state);
        }
        public PlayerState byteToState(byte byteState)
        {

            if (byteState == (byte)1)
            {
                //idle
                return PlayerState.Idle;
            }
            else if (byteState == (byte)2)
            {
                //walking
                return PlayerState.Walking;
            }
            else if (byteState == (byte)3)
            {
                //running
                return PlayerState.Sprinting;
            }
            else if (byteState == (byte)4)
            {
                //jump
                return PlayerState.Jumping;
            }
            else if (byteState == (byte)5)
            {
                //diving
                return PlayerState.Diving;
            }
            else if (byteState == (byte)6)
            {
                //airdashing
                return PlayerState.AirDashing;
            }
            return PlayerState.Idle;

        }
        public byte StateToByte(PlayerState state)
        {

            switch (state)
            {

                case PlayerState.Idle:
                return (byte)1;

                case PlayerState.Walking:
                    return (byte)2;


                case PlayerState.Sprinting:
                    return (byte)3;


                case PlayerState.Jumping:
                    return (byte)4;


                case PlayerState.Diving:
                    return (byte)5;


                case PlayerState.AirDashing:
                return (byte)6;

                default:
                return (byte)1;
            }
        }
    }
}


