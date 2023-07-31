using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class AdvPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float sprintSpeed;
    public float groundDrag;
    public float airDrag;

    public float jumpForce;
    public float jumpCooldown;

    public float fastFallForce;
    public float airMultiplier;

    public float airDashSpeed;
    bool readyToJump;
    bool isSprinting;
    bool isMoving;

    bool readyToFastFall;

    bool readyToAirDash;

    [Header("Stamina")]
    public float standingRegen;
    public float walkingRegen;
    public float sprintingRegen;
    public float AirDashCost;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

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

    [SerializeField] Animator _baby;
    public UnitStamina _playerStamina = new UnitStamina(100f, 100f, 30f, false);
    [HideInInspector] public TextMeshProUGUI text_speed;
    bool gamePadConnected = false;
    enum PlayerState
    {
        Idle,
        Walking,
        Sprinting,
        Jumping,
        AirDashing
    }
    PlayerState lastState;
    PlayerState playerState;


    private void Start()
    {
        lastState = PlayerState.Idle;
        playerState = PlayerState.Idle;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        gamePadConnected = Input.GetJoystickNames().Length != 0;
        readyToJump = true;
        readyToAirDash = true;
        readyToFastFall = true;
    }

    private void Update()
    {

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + -0.7f, whatIsGround);
        MyInput();
        PlayerRegenStamina();
        UpdatePlayerState();
        AnimatePlayer();
        Debug.Log(playerState);
        Debug.Log(grounded);

        // handle drag
        if (grounded)
        {
            rb.drag = groundDrag;
            readyToFastFall = true;
            readyToAirDash = true;
        }

        else
            rb.drag = airDrag;

        
    }

    private void FixedUpdate()
    {
        MovePlayer();
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

        if (Input.GetKey(KeyCode.LeftShift))
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        if (Input.GetKey(KeyCode.E) && readyToAirDash && !grounded)
        {
            readyToAirDash = false;
            AirDash();
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

        if (Gamepad.all[0].rightTrigger.wasPressedThisFrame && readyToAirDash && !grounded)
        {
            readyToAirDash = false;
            AirDash();
        }

        if (Gamepad.all[0].xButton.isPressed && readyToFastFall && !grounded)
        {
            readyToFastFall = false;
            FastFall();
        }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
        if(gamePadConnected)ControllerControl();
        KeyboardControl();
        

    }

    private void MovePlayer()
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
    }

    private void AirDash()
    {
        // calculate movement direction
        if (_playerStamina.Stamina >= AirDashCost)
        {
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
            rb.AddForce(moveDirection.normalized * airDashSpeed * 10f * airMultiplier, ForceMode.Impulse);
            PlayerUseStamina(AirDashCost);
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
    private void ResetAirDash()
    {
        readyToAirDash = true;
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
        if (!grounded)
        {
            if (readyToAirDash)
            {
                playerState = PlayerState.Jumping;
            }
            else
            {
                playerState = PlayerState.AirDashing;
            }
        }
        else
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                if (gamePadConnected && Gamepad.all[0].rightTrigger.isPressed && Input.GetAxis("Vertical") > 0)
                {
                    playerState = PlayerState.Sprinting;
                }
                else if (Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0)
                {
                    playerState = PlayerState.Sprinting;
                }
                else
                {
                    playerState = PlayerState.Walking;
                }
            }
            else
            {
                playerState = PlayerState.Idle;
            }
        }
    }

    private void AnimatePlayer()
    {
        if (lastState != playerState)
        {
            _baby.SetBool("isIdle", false);
            _baby.SetBool("isWalking", false);
            _baby.SetBool("isSprinting", false);
            _baby.SetBool("isJumping", false);
            _baby.SetBool("isAirDashing", false);

            switch (playerState)
            {
                case PlayerState.Idle:
                _baby.SetBool("isIdle", true);
                break;

                case PlayerState.Walking:
                _baby.SetBool("isWalking", true);
                break;

                case PlayerState.Sprinting:
                _baby.SetBool("isSprinting", true);
                break;

                case PlayerState.Jumping:
                _baby.SetBool("isJumping", true);
                break;

                case PlayerState.AirDashing:
                _baby.SetBool("isAirDashing", true);
                break;
            }

            lastState = playerState;
        }
    }
}