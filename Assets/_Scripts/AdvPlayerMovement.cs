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


    [HideInInspector] public TextMeshProUGUI text_speed;


    private void Start()
    {

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        readyToAirDash = true;
        readyToFastFall = true;
        isSprinting = false;
        isMoving = false;
    }

    private void Update()
    {

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        MyInput();
        PlayerRegenStamina();

        Debug.Log(GameManager.gameManager._playerStamina.Stamina);
 

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

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Gamepad.all[0].leftStick.x.value != 0f && Gamepad.all[0].leftStick.y.value != 0f){
            isMoving = true;
        }
        else{
            isMoving = false;
        }

        // when to jump
        if ((Input.GetKey(jumpKey) || Gamepad.all[0].aButton.isPressed) && readyToJump && grounded)
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

        if (Gamepad.all[0].rightTrigger.isPressed && grounded)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        float speedvar;
        if (isSprinting)
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
        if (GameManager.gameManager._playerStamina.Stamina >= AirDashCost)
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
        GameManager.gameManager._playerStamina.UseStamina(staminaAmount);
        _staminaBar.SetStamina(staminaAmount);
    }

    private void PlayerRegenStamina()
    {   
        float regenAmount;
        if (!isSprinting && grounded)
        {
            if (!isMoving)
            {
                regenAmount = standingRegen;
            }
            else
            {
                regenAmount = walkingRegen;
            }
        GameManager.gameManager._playerStamina.RegenStamina(regenAmount);
        _staminaBar.SetStamina(GameManager.gameManager._playerStamina.Stamina);
        }
    }
}