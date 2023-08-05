using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// A simple FPP (First Person Perspective) camera rotation script.
/// Like those found in most FPS (First Person Shooter) games.
/// </summary>
public class PlayerCam2 : MonoBehaviour
{
	public Rigidbody playerBody;
	[SerializeField] Transform orientation; 
	public float Sensitivity
	{
		get { return sensitivity; }
		set { sensitivity = value; }
	}
	[Range(0.1f, 40f)] [SerializeField] float sensitivity = 2f;
	[Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
	[Range(0f, 90f)] [SerializeField] float yRotationLimit = 88f;

	Vector2 rotation = Vector2.zero;
	const string xAxis = "Mouse X"; //Strings in direct code generate garbage, storing and re-using them creates no garbage
	const string yAxis = "Mouse Y";

	bool gamePadConnected = false;

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		gamePadConnected = Input.GetJoystickNames().Length != 0;
	}
	void FixedUpdate()
	{
		if (gamePadConnected) gamePadControl();
		else keyboardControl(); 
		//playerBody.MoveRotation(playerBody.rotation * Quaternion.Euler(0, rotation.x * Time.deltaTime * 60, 0));
		//playerBody.GetComponent<Rigidbody>().transform.localRotation = xQuat;
	}
	private void keyboardControl()
    {
		rotation.x += Input.GetAxis(xAxis) * sensitivity;
		rotation.y += Input.GetAxis(yAxis) * sensitivity;
		rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);
		var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
		var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);

		transform.localRotation = yQuat; 		 //orientation.localRotation = xQuat;
		playerBody.transform.localRotation = xQuat;
	}

	private void gamePadControl()
    {
		rotation.x += Gamepad.all[0].rightStick.x.value * sensitivity;
		rotation.y += Gamepad.all[0].rightStick.y.value * sensitivity;
		rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);
		var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
		var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);
		transform.localRotation = yQuat;         //orientation.localRotation = xQuat;
		playerBody.transform.localRotation = xQuat;
	}

}