using UnityEngine;
using System.Collections;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class DaddyGrab : NetworkBehaviour
{
    public float weaponRange = 0.1f;
    [SerializeField] Camera fpsCam;
    [SerializeField] GameObject grab; // Holds a reference to the first person camera
    private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);    // WaitForSeconds object used by our ShotEffect coroutine, determines time laser line will remain visible
    private AudioSource gunAudio;                                        // Reference to the audio source which will play our shooting sound effect
    private LineRenderer laserLine;                                        // Reference to the LineRenderer component which will display our laserline
    private float nextFire;                                                // Float to store the time the player will be allowed to fire again, after firing
    bool gamePadConnected = false;
    public float bulletSpreadAmount = 2f;

    void Start()
    {
        gamePadConnected = Input.GetJoystickNames().Length != 0;
    }







    [ServerRpc]
    public void BabyCapturedServerRpc()
    {
        FindObjectOfType<GameLogic>().BabyCaughtClientRpc();
    }

    void Update()
    {
        
        RaycastHit hit;
        Vector3 rayOrigin = fpsCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
        Vector3 fwd = fpsCam.transform.forward;
        //fwd = fwd + fpsCam.transform.TransformDirection(new Vector3(Random.Range(-bulletSpreadAmount, bulletSpreadAmount), Random.Range(-bulletSpreadAmount, bulletSpreadAmount)));

        if (Physics.Raycast(rayOrigin, fwd, out hit, 2f) && hit.collider.gameObject.CompareTag("Baby") )
        {

            if (!hit.collider.gameObject.CompareTag("Baby")) { return; }
            //Debug.DrawRay(rayOrigin, fpsCam.transform.forward, Color.cyan, 1f);
            print(hit.distance);
            grab.SetActive(true);

            if (gamePadConnected)
            {
                if (Gamepad.all[0].rightShoulder.isPressed)
                {
                    BabyCapturedServerRpc();
                    //Debug.DrawRay(rayOrigin, fpsCam.transform.forward, Color.cyan, 1f);
                    print("BABY CAPTURED");
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    BabyCapturedServerRpc();
                    //Debug.DrawRay(rayOrigin, fpsCam.transform.forward, Color.cyan, 1f);
                    print("BABY CAPTURED");
                }
            }
        }
        else
        {
            grab.SetActive(false);
        }

    }
}