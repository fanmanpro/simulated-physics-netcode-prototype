using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
	private Rigidbody rb;
	public bool forward = false, backward = false, stop = false;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		if (rb == null) gameObject.SetActive(false);
	}

	void FixedUpdate()
	{
		if (stop)
		{
			// rb.velocity = new Vector3 { x = 0, y = 0, z = 0 };
			stop = false;
		}
		else
		{
			if (forward)
			{
				Vector3 forward = rb.transform.forward * 3;
				rb.velocity = forward;
			}
			else if (backward)
			{
				Vector3 backward = rb.transform.forward * -3;
				rb.velocity = backward;
			}
		}
	}

	void Update()
	{
		KeyCode forwadKey = KeyCode.S;
		KeyCode backwardKey = KeyCode.W;

		if (!gameObject.scene.name.Equals(SceneManager.GetActiveScene().name))
		{
			forwadKey = KeyCode.S;
			backwardKey = KeyCode.W;
		}

		if (Input.GetKey(backwardKey) && !forward)
		{
			if (backward)
			{
				backward = false;
			}
			forward = true;
		}
		else if (Input.GetKey(forwadKey) && !backward)
		{
			if (forward)
			{
				forward = false;
			}
			backward = true;
		}
		else if (!Input.GetKey(forwadKey) && !Input.GetKey(backwardKey))
		{
			backward = forward = false;
			stop = true;
		}
	}
}
