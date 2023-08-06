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
    [SerializeField] GameObject _camera;
    [SerializeField] StaminaBar _staminaBar;
    [SerializeField] CapsuleCollider collider;

    [SerializeField] Animator _animator;
    public UnitStamina _playerStamina = new UnitStamina(100f, 100f, 30f, false);
    [HideInInspector] public TextMeshProUGUI text_speed;
    bool gamePadConnected = false;
    enum PlayerState
    {
        Idle,
        Walking,
        Sprinting,
        Jumping,
        Diving
    }
    PlayerState lastState;
    PlayerState playerState;
    float setGroundDrag;
    bool diveInCooldown;


    private void Start()
    {
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
                Invoke(nameof(ResetDive), diveCooldown);}
        }

        else
            rb.drag = airDrag;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        StepClimb();
        
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
            readyToDive = false;
            groundDrag = 2;
            Dive();
            
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
                    print("lower step detected");
                    if (!Physics.Raycast(stepUpper.transform.position, transform.TransformDirection(angles[i]), 0.2f, whatIsGround))
                    {
                        rb.AddForce(transform.up * 90, ForceMode.Force);
                        break;
                        print("stepping up!");
                    }
                }
            }
        }

    }

    private void Dive()
    {
        // calculate movement direction
        if (_playerStamina.Stamina >= diveCost)
        {
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
   
            if (!readyToDive)
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

    private void AnimatePlayer()
    {
        if (!IsOwner)
        {
            return;
        }
        if (lastState != playerState)
        {
            _animator.SetBool("isIdle", false);
            _animator.SetBool("isWalking", false);
            _animator.SetBool("isSprinting", false);
            //_animator.SetBool("isJumping", false);
            _animator.SetBool("isDiving", false);

            switch (playerState)
            {
                case PlayerState.Idle:
                _animator.SetBool("isIdle", true);
                break;

                case PlayerState.Walking:
                _animator.SetBool("isWalking", true);
                break;

                case PlayerState.Sprinting:
                _animator.SetBool("isSprinting", true);
                break;

                case PlayerState.Jumping:
                //_animator.SetBool("isJumping", true);
                break;

                case PlayerState.Diving:
                _animator.SetBool("isDiving", true);
                break;
            }

            lastState = playerState;
        }
    }
}