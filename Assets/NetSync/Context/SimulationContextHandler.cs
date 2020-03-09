using System.Collections.Generic;
using ThreadedNetworkProtocol;
using UnityEngine;

public class SimulationContextHandler : MonoBehaviour, IContextHandler
{
	public Client client;

	public List<NetSynced.Rigidbody2D> rigidbodies;

	// these are in bytes, and 32768 is 32KiB
	public float MaxPacketSize = 32768;

	// this can grow depending on how many objects the sim server can
	// handle to send across to all clients with the current sim
	public float MaxObjects = 32;

	public float FullSimulationStatePacketSize = 768;

	void Start()
	{
		rigidbodies = new List<NetSynced.Rigidbody2D>();
		foreach (GameObject rootGameObject in gameObject.scene.GetRootGameObjects())
		{
			rigidbodies.AddRange(rootGameObject.GetComponentsInChildren<NetSynced.Rigidbody2D>());
		}
	}

	public void HandleContext(Serializable.Context context)
	{
		List<Serializable.Transform> transforms = new List<Serializable.Transform>();
		int offset = 0;

		int r = 0;
		foreach (NetSynced.Rigidbody2D rb in rigidbodies)
		{
			if ((r - offset + 1) * 30 > FullSimulationStatePacketSize)
			{
				client.Send(PacketType.Unreliable, new Serializable.Context { Tick = context.Tick, Transforms = { transforms } });
				transforms.Clear();
				offset = r;
			}

			//if (!rigidbody.IsAwake()) continue;
			transforms.Add(rb.Export());
			r++;
		}
		if (transforms.Count > 0)
		{
			client.Send(PacketType.Unreliable, new Serializable.Context { Tick = context.Tick, Transforms = { transforms } });
			transforms.Clear();
		}
	}
}
