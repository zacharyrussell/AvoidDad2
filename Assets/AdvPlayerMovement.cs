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

    public float groundDrag;
    public float airDrag;

    public float jumpForce;
    public float jumpCooldown;

    public float fFallForce;
    public float FFallCooldown;
    public float airMultiplier;

    public float boostSpeed;

    public float boostCooldown;
    bool readyToJump;

    bool readyToFall;

    bool readyToBoost;

    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float sprintSpeed;

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


    [HideInInspector] public TextMeshProUGUI text_speed;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        readyToBoost = true;
        readyToFall = true;
    }

    private void Update()
    {

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        //SpeedControl();

        // handle drag
        if (grounded)
        {
            rb.drag = groundDrag;
            readyToFall = true;
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

        // when to jump
        if ((Input.GetKey(jumpKey) || Gamepad.all[0].aButton.isPressed) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Gamepad.all[0].rightTrigger.isPressed && readyToBoost)
        {
            readyToBoost = false;
            Boost();
            Invoke(nameof(ResetBoost), boostCooldown);
        }

        if (Gamepad.all[0].xButton.isPressed && readyToFall && !grounded)
        {
            readyToFall = false;
            FFall();
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void Boost()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * boostSpeed * 10f, ForceMode.Impulse);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * boostSpeed * 10f * airMultiplier, ForceMode.Impulse);
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

    private void FFall()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(-transform.up * fFallForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }
    private void ResetBoost()
    {
        readyToBoost = true;
    }
}