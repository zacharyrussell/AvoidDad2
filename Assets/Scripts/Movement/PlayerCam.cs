using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;
    [SerializeField] Rigidbody player;
    bool gamePadConnected = false;
    public float rotationSpeed = 0.1f;
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gamePadConnected = Input.GetJoystickNames().Length != 0;
    }


    private void mouseControl()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation += mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);


        var xQuat = Quaternion.AngleAxis(xRotation, Vector3.left);
        var yQuat = Quaternion.AngleAxis(yRotation, Vector3.up);
        transform.localRotation = xQuat * yQuat;
    }

    private void joystickControl()
    {
        float padX = Gamepad.all[0].rightStick.x.value * Time.deltaTime * sensX;
        float padY = Gamepad.all[0].rightStick.y.value * Time.deltaTime * sensY;

        yRotation += padX;

        xRotation -= padY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate cam and orientation
        // transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        player.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
    private void Update()
    {
        // get mouse input

        if(gamePadConnected)joystickControl();
        else mouseControl();
    }
}