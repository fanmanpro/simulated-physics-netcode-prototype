using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
	private Seat seat;
	private bool up = false, down = false, stop = false;
	void Start()
	{
		ClientContextManager context = null;
		foreach (GameObject rootGameObject in gameObject.scene.GetRootGameObjects())
		{
			context = rootGameObject.GetComponentInChildren<ClientContextManager>();
			if (context != null)
			{
				break;
			}
		}
		if (context == null)
			return;

		seat = GetComponent<Seat>();
		foreach (NetSync g in seat.ownerOf)
		{
			g.gameObject.AddComponent<PlayerController>();
			context.RegisterPlayerOwned(g.GUID);
		}
	}
}
