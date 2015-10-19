using UnityEngine;

public class StrafeMovement : MonoBehaviour
{
    [SerializeField]
    private float accel = 200f;
    [SerializeField]
    private float airAccel = 200f;
    [SerializeField]
    private float maxSpeed = 6.4f;
    [SerializeField]
    private float maxAirSpeed = 0.6f;
    [SerializeField]
    private float friction = 8f;
    [SerializeField]
    private float jumpForce = 5f;
    [SerializeField]
    private LayerMask groundLayers;

    [SerializeField]
    private GameObject camObj;

    private float lastJumpPress = -1f;
    private float jumpPressDuration = 0.1f;
	private bool onGround = false;

	private bool jumpKeyPressed = false;
    
	void Update()
	{
		//Set key states
		if(Input.GetButton("Jump"))
			{ jumpKeyPressed = true; } else { jumpKeyPressed = false; }
			
		if(jumpKeyPressed)
		{
			lastJumpPress = Time.time;
		}
	}

	void FixedUpdate()
	{
		Vector2 input = new Vector2();

        input.x = Input.GetAxis("Horizontal");
		input.y = Input.GetAxis("Vertical");

		//Friction
		Vector3 tempVelocity = calculateFriction(GetComponent<Rigidbody>().velocity);

		//Add movement
		tempVelocity += calculateMovement(input, tempVelocity);		
		
		//Apply
		if(!GetComponent<Rigidbody>().isKinematic)
		{
			GetComponent<Rigidbody>().velocity = tempVelocity;
		}
	}

	public Vector3 calculateFriction(Vector3 currentVelocity)
	{
        onGround = checkGround();
		float speed = currentVelocity.magnitude; 

		//Code from https://flafla2.github.io/2015/02/14/bunnyhop.html
		if(onGround && !Input.GetButton("Jump") && speed != 0f)
		{
			float drop = speed * friction * Time.deltaTime;
			return currentVelocity * (Mathf.Max(speed - drop, 0f) / speed);
		}

		return currentVelocity;
	}

	//Do movement input here
	public Vector3 calculateMovement(Vector2 input, Vector3 velocity)
	{
        onGround = checkGround();

		//Different acceleration values for ground and air
		float curAccel = accel;
		if(!onGround)
			curAccel = airAccel;

		//Ground speed
		float curMaxSpeed = maxSpeed;

		//Air speed
		if(!onGround)
			curMaxSpeed = maxAirSpeed;

		//Get input and make it a vector
		Vector3 camRotation = new Vector3(0f, camObj.transform.rotation.eulerAngles.y, camObj.transform.rotation.eulerAngles.z);
		Vector3 inputVelocity = Quaternion.Euler(camRotation) * new Vector3(input.x * curAccel, 0f, input.y * curAccel);

		//Ignore vertical component of rotated input
		Vector3 alignedInputVelocity = new Vector3(inputVelocity.x, 0f, inputVelocity.z) * Time.deltaTime;
		
		//Get current velocity
		Vector3 currentVelocity = new Vector3(velocity.x, 0f, velocity.z);

		//How close the current speed to max velocity is (1 = not moving, 0 = at/over max speed)
		float max = Mathf.Max(0f, 1 - (currentVelocity.magnitude / curMaxSpeed));

		//How perpendicular the input to the current velocity is (0 = 90°)
		float velocityDot = Vector3.Dot(currentVelocity, alignedInputVelocity);

		//Scale the input to the max speed
		Vector3 modifiedVelocity = alignedInputVelocity * max;

		//The more perpendicular the input is, the more the input velocity will be applied
		Vector3 correctVelocity = Vector3.Lerp(alignedInputVelocity, modifiedVelocity, velocityDot);

		//Apply jump
		correctVelocity += getJumpVelocity(velocity.y);

		//Return
		return correctVelocity;
	}

	private Vector3 getJumpVelocity(float yVelocity)
	{
		Vector3 jumpVelocity = Vector3.zero;

		//Calculate jump
		if(Time.time < lastJumpPress + jumpPressDuration && yVelocity < jumpForce && checkGround())
		{
			lastJumpPress = -1f;
			jumpVelocity = new Vector3(0f, jumpForce - yVelocity, 0f);
		}

		return jumpVelocity;
	}

	public bool getJumpKeyPressed()
	{
		return jumpKeyPressed;
	}
	
	public bool checkGround()
	{
        Ray ray = new Ray(transform.position, Vector3.down);
        bool result = Physics.Raycast(ray, GetComponent<Collider>().bounds.extents.y + 0.1f, groundLayers);
        return result;
	}
    
	private float getVelocity()
	{
		return Vector3.Magnitude(GetComponent<Rigidbody>().velocity);
	}
}
