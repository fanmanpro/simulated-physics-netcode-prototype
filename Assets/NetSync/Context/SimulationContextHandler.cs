using System.Collections.Generic;
using ThreadedNetworkProtocol;
using UnityEngine;

public class SimulationContextHandler : MonoBehaviour, IContextHandler
{
	public Client client;

	public List<NetSynced.Rigidbody2D> rigidbodies;
	public List<NetSynced.Transform> transforms;

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

		transforms = new List<NetSynced.Transform>();
		foreach (GameObject rootGameObject in gameObject.scene.GetRootGameObjects())
		{
			transforms.AddRange(rootGameObject.GetComponentsInChildren<NetSynced.Transform>());
		}
	}

	public void HandleContext(Serializable.Context context)
	{
		List<Serializable.Rigidbody> _rigidbodies = new List<Serializable.Rigidbody>();
		int offset = 0;

		int r = 0;
		foreach (NetSynced.Rigidbody2D rb in rigidbodies)
		{
			if ((r - offset + 1) * 30 > FullSimulationStatePacketSize)
			{
				client.Send(PacketType.Unreliable, new Serializable.Context { Tick = context.Tick, RigidBodies = { _rigidbodies } });
				_rigidbodies.Clear();
				offset = r;
			}

			//if (!rigidbody.IsAwake()) continue;
			_rigidbodies.Add(rb.Export());
			r++;
		}
		if (_rigidbodies.Count > 0)
		{
			client.Send(PacketType.Unreliable, new Serializable.Context { Tick = context.Tick, RigidBodies = { _rigidbodies } });
			_rigidbodies.Clear();
		}
	}
}
