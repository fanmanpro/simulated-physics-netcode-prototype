using UnityEngine;

public class Player: MonoBehaviour
{
	private NetSynced.Rigidbody2D rb;
	private bool up = false, down = false, stop = false;
	void Start()
	{
		rb = GetComponent<NetSynced.Rigidbody2D>();
	}

	void FixedUpdate()
	{
		//if (stop)
		//{
		//	rb.Velocity = new Vector2 { x = 0, y = 0 };
		//	stop = false;
		//}
		//else
		//{
		//	if (up)
		//	{
		//		rb.Velocity = new Vector2 { x = 0, y = 3 };
		//	}
		//	else if (down)
		//	{
		//		rb.Velocity = new Vector2 { x = 0, y = -3 };
		//	}
		//}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow) && !up)
		{
			if (down)
			{
				down = false;
			}
			up = true;
		}
		if (Input.GetKeyDown(KeyCode.DownArrow) && !down)
		{
			if (up)
			{
				up = false;
			}
			down = true;
		}
		if ((Input.GetKeyUp(KeyCode.DownArrow) && down) || (Input.GetKeyUp(KeyCode.UpArrow) && up))
		{
			down = up = false;
			stop = true;
		}
	}
}
