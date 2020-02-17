using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
	private PlayerSeat seat;
	private bool up = false, down = false, stop = false;
	void Start()
	{
		ContextManager context = null;
		foreach (GameObject rootGameObject in gameObject.scene.GetRootGameObjects())
		{
			context = rootGameObject.GetComponentInChildren<ContextManager>();
			if (context != null)
			{
				break;
			}
		}
		if (context == null)
			return;

		seat = GetComponent<PlayerSeat>();
		foreach (NetSync g in seat.ownerOf)
		{
			g.gameObject.AddComponent<PlayerController>();
			context.RegisterPlayerOwned(g.GUID);
		}
	}
}
