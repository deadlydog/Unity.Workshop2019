using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float acceleration;
    public float defaultMaxSpeed;
    private float maxSpeed;

    public float jumpPower;
    public float jetpackPower;
    public float tiltPower;

    public Transform body;
    private Quaternion defaultRotation;
    public ParticleSystem jetpackParticles;
    private bool landing = false;	// If the player is landing and still straightening up.
    public float landingTimer;		// How long it takes the player to straighten up after they land.
    private bool grounded = false;
    public LayerMask layerMask;
    private Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        defaultRotation = transform.rotation;
    }

    // Runs before Start()
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        maxSpeed = defaultMaxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();

        if (Input.GetAxis("Horizontal") != 0 && grounded && !landing)
        {
            MoveDirection(Input.GetAxis("Horizontal"));
        }
        else if (Input.GetAxis("Horizontal") != 0 && !grounded && !landing)
        {
			TiltDirectrion(Input.GetAxis("Horizontal"));
        }

		if (rigidBody.velocity.magnitude > maxSpeed)
		{
			rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;
		}

		if (Input.GetKeyDown(KeyCode.Space) && grounded && !landing)
		{
			Jump();
		}
    }

	private void TiltDirectrion(float direction)
	{
		rigidBody.AddTorque(transform.forward * -direction * tiltPower);
	}

	private void Jump()
	{
		rigidBody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
	}

    private void MoveDirection(float direction)
    {
        rigidBody.AddForce(Vector3.right * direction * acceleration);

		if (direction > 0)
		{
			body.localEulerAngles = Vector3.zero;
		}
		else
		{
			body.localEulerAngles = new Vector3(0, 180, 0);
		}
    }

    private void GroundCheck()
    {
        // If the player's position is within 1 unit of the ground, they are on the ground.
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, 1f, layerMask))
        {
            if (!grounded)
            {
				StartCoroutine(StraightenPlayer());
            }

            grounded = true;

            // Freeze the Players rotation while on the ground so they stand straight up.
            rigidBody.constraints = RigidbodyConstraints.FreezePositionZ
                | RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationY
                | RigidbodyConstraints.FreezeRotationZ;

            maxSpeed = defaultMaxSpeed;
        }
        // Else the Player is above the ground quite a bit (i.e. they are still up in the air).
        else
        {
            grounded = false;
            maxSpeed = defaultMaxSpeed * 20f;
            rigidBody.constraints = RigidbodyConstraints.FreezePositionZ;
        }
    }

	private IEnumerator StraightenPlayer()
	{
		landing = true;
		float timer = 0;

		Quaternion currentRotation = transform.rotation;

		while (timer < landingTimer)
		{
			timer += Time.deltaTime;
			transform.rotation = Quaternion.Slerp(currentRotation, defaultRotation, timer / landingTimer);
			yield return null;
		}

		landing = false;
	}
}
