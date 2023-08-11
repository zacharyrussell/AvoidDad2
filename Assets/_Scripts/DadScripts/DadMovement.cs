using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class DadMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float sprintSpeed;
    public float diveSpeed;
    public float throwSpeed;
    public float groundDrag;
    public float airDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float diveCooldown;
    public float fastFallForce;
    public float airMultiplier;
    bool readyToJump;
    bool readyToDive;

    bool readyToFastFall;

    [Header("Stamina")]
    public float diveCost;
    public float standingRegen;
    public float walkingRegen;
    public float sprintingRegen;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    [Header("StepUp")]
    [SerializeField] GameObject stepUpper;
    [SerializeField] GameObject stepLower;
    [SerializeField] float stepHeight = 0.3f;
    [SerializeField] float stepSmooth = 0.1f;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    [SerializeField] Camera _camera;
    [SerializeField] StaminaBar _staminaBar;
    [SerializeField] CapsuleCollider collider;
    [SerializeField] LayerMask whatIsPlayer;
    [SerializeField] private float PickupRange;
    [SerializeField] private Transform PickupTarget;
    private Rigidbody CurrentObject;

    [SerializeField] Animator _animator;
    [SerializeField] Animator _armAnimator;
    public UnitStamina _playerStamina = new UnitStamina(100f, 100f, 30f, false);
    [HideInInspector] public TextMeshProUGUI text_speed;
    bool gamePadConnected = false;
    PlayerState lastState;
    PlayerState playerState;
    float setGroundDrag;
    bool diveInCooldown;


    private void Start()
    {
        diveInCooldown = false;
        lastState = PlayerState.Idle;
        playerState = PlayerState.Idle;
        setGroundDrag = groundDrag;
        rb = GetComponent<Rigidbody>();
        
        rb.freezeRotation = true;
        gamePadConnected = Input.GetJoystickNames().Length != 0;
        readyToJump = true;
        readyToDive = true;
        readyToFastFall = true;
        stepUpper.transform.position = new Vector3(stepUpper.transform.position.x, stepHeight, stepUpper.transform.position.z);
    }

    private void Update()
    {
        if (!IsOwner) return;
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + -0.7f, whatIsGround);
        MyInput();
        PlayerRegenStamina();
        UpdatePlayerState();
        AnimatePlayer();
        

        // handle drag
        if (grounded)
        {
            rb.drag = groundDrag;
            readyToFastFall = true;
            if (!readyToDive && !diveInCooldown)
            {
                diveInCooldown = true;
                Invoke(nameof(ResetDive), diveCooldown);
                }
        }

        else
            rb.drag = airDrag;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        StepClimb();
        if (CurrentObject)
        {
            Vector3 directionToPoint = PickupTarget.position + new Vector3(-0.5f, 0.5f, 0) - CurrentObject.position;
            float distanceToPoint = directionToPoint.magnitude;

            CurrentObject.velocity = directionToPoint * 12f * distanceToPoint;
        }
    }


    private void KeyboardControl()
    {
        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }


        if (Input.GetKey(KeyCode.Q) && readyToFastFall && !grounded)
        {
            readyToFastFall = false;
            FastFall();
        }
        if (Input.GetKey(KeyCode.E) && readyToDive)
        {
            readyToDive = false;
            groundDrag = 2;
            Dive();
        }
    }

    private void ControllerControl()
    {
        // when to jump
        if (Gamepad.all[0].aButton.wasPressedThisFrame && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Gamepad.all[0].xButton.isPressed && readyToFastFall && !grounded)
        {
            readyToFastFall = false;
            FastFall();
        }
        if (Gamepad.all[0].bButton.wasPressedThisFrame && readyToDive)
        {
            
            Dive();
        }
        if (Gamepad.all[0].rightShoulder.wasPressedThisFrame)
        {
            CarryDad();
        }
    }

    private void MyInput()
    {
        if (readyToDive)
        {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
        if(gamePadConnected)ControllerControl();
        KeyboardControl();
        }
        

    }

    private void MovePlayer()
    {
        if (readyToDive)
        {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        float speedvar;
        if (playerState == PlayerState.Sprinting)
        {
            speedvar = sprintSpeed;
        }
        else
        {
            speedvar = moveSpeed;
        }

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * speedvar * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * speedvar * 10f * airMultiplier, ForceMode.Force);
    }}

    void StepClimb()
    {
        if (playerState != PlayerState.Idle)
        {
        Vector3[] angles = {Vector3.forward, Vector3.back, Vector3.left, Vector3.right, 
                            new Vector3(1.5f, 0f, 1f), new Vector3(-1.5f, 0f,1f), new Vector3(1.5f, 0f, -1f), new Vector3(-1.5f,0f,-1f)};
        for (int i = 0; i < angles.Length; i++)
        {
        if (Physics.Raycast(stepLower.transform.position, transform.TransformDirection(angles[i]), 0.1f, whatIsGround))
        {
            
            if (!Physics.Raycast(stepUpper.transform.position, transform.TransformDirection(angles[i]), 0.2f, whatIsGround))
            {
                rb.AddForce(transform.up * 90, ForceMode.Force);
                break;
                
            }
        }}}

    }

    private void Dive()
    {
        // calculate movement direction
        if (_playerStamina.Stamina >= diveCost)
        {
            groundDrag = 2;
            readyToDive = false;
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
            rb.AddForce(moveDirection.normalized * diveSpeed * 10f * airMultiplier, ForceMode.Impulse);
            PlayerUseStamina(diveCost);
        }
        
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }

        text_speed.SetText("Speed: " + flatVel.magnitude);
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void FastFall()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(-transform.up * fastFallForce, ForceMode.Impulse);
    }

    private void CarryDad()
    {
        if (CurrentObject)
            {
                CurrentObject.useGravity = true;
                CurrentObject.GetComponent<Rigidbody>().AddForce(moveDirection.normalized * throwSpeed * 10f * airMultiplier, ForceMode.Impulse);
                CurrentObject = null;

                return;
            }
            Ray CameraRay = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(CameraRay, out RaycastHit HitInfo, PickupRange, whatIsPlayer))
            {
                
                CurrentObject = HitInfo.rigidbody;
                CurrentObject.useGravity = false;
            }
    }
    private void ResetJump()
    {
        readyToJump = true;
    }
    private void ResetDive()
    {
        groundDrag = setGroundDrag;
        readyToDive = true;
        diveInCooldown = false;
    }

    private void PlayerUseStamina(float staminaAmount)
    {
        _playerStamina.UseStamina(staminaAmount);
    }

    private void PlayerRegenStamina()
    {   
        switch (playerState)
        {
            case PlayerState.Idle:
            _playerStamina.RegenStamina(standingRegen);
            break;

            case PlayerState.Walking:
            _playerStamina.RegenStamina(walkingRegen);
            break;
        }
        _staminaBar.SetStamina(_playerStamina.Stamina);
        }

    private void UpdatePlayerState()
    {
        if (CurrentObject)
        {
            playerState = PlayerState.Carrying;
        }
        else if (!readyToDive)
        {
            playerState = PlayerState.Diving;
        }
        
        else if (!grounded)
        {
                playerState = PlayerState.Jumping;
        }
        else if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            if (gamePadConnected && (Gamepad.all[0].leftStickButton.isPressed || Gamepad.all[0].rightTrigger.isPressed) && Input.GetAxis("Vertical") > 0)
                { playerState = PlayerState.Sprinting; }

                else if (Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0) { playerState = PlayerState.Sprinting; }
                else { playerState = PlayerState.Walking; }
            }
        else { playerState = PlayerState.Idle; }
    }



    public PlayerState GetPlayerState()
    {
        return playerState;
    }

    public void SetPlayerState(PlayerState netRecievedState)
    {
        playerState = netRecievedState;
        AnimatePlayer();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (playerState != PlayerState.Diving) return;

        if (collision.gameObject.CompareTag("Baby"))
        {
            FindAnyObjectByType<DaddyGrab>().BabyCapturedServerRpc();
        }
    }

    private void AnimatePlayer()
    {
        if (lastState != playerState)
        {
            Animator[] animators = {_animator, _armAnimator};
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].SetBool("isIdle", false);
                animators[i].SetBool("isWalking", false);
                animators[i].SetBool("isSprinting", false);
                animators[i].SetBool("isJumping", false);
                animators[i].SetBool("isDiving", false);
                animators[i].SetBool("isCarrying", false);

                switch (playerState)
                {
                    case PlayerState.Idle:
                    animators[i].SetBool("isIdle", true);
                    break;

                    case PlayerState.Walking:
                    animators[i].SetBool("isWalking", true);
                    break;

                    case PlayerState.Sprinting:
                    animators[i].SetBool("isSprinting", true);
                    break;

                    case PlayerState.Jumping:
                    animators[i].SetBool("isJumping", true);
                    break;

                    case PlayerState.Diving:
                    animators[i].SetBool("isDiving", true);
                    break;

                    case PlayerState.Carrying:
                    animators[i].SetBool("isCarrying", true);
                    break;
                }
            }

            lastState = playerState;
        }
    }
}