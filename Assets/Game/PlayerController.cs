using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
	private Rigidbody2D rb;
	private bool up = false, down = false, stop = false;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate()
	{
		if (stop)
		{
			rb.velocity = new Vector2 { x = 0, y = 0 };
			stop = false;
		}
		else
		{
			if (up)
			{
				rb.velocity = new Vector2 { x = 0, y = 3 };
			}
			else if (down)
			{
				rb.velocity = new Vector2 { x = 0, y = -3 };
			}
		}
	}

	void Update()
	{
		KeyCode downKey = KeyCode.DownArrow;
		KeyCode upKey = KeyCode.UpArrow;

		if (!gameObject.scene.name.Equals(SceneManager.GetActiveScene().name))
		{
			downKey = KeyCode.S;
			upKey = KeyCode.W;
		}

		if (Input.GetKeyDown(upKey) && !up)
		{
			if (down)
			{
				down = false;
			}
			up = true;
		}
		if (Input.GetKeyDown(downKey) && !down)
		{
			if (up)
			{
				up = false;
			}
			down = true;
		}
		if ((Input.GetKeyUp(downKey) && down) || (Input.GetKeyUp(upKey) && up))
		{
			down = up = false;
			stop = true;
		}
	}
}
