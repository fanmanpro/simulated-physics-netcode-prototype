using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using ThreadedNetworkProtocol;
using UnityEngine;

public class ClientContextHandler : MonoBehaviour, IContextHandler
{
	public ClientContextManager clientContextManager;
	public Client client;

	public List<NetSynced.Rigidbody3D> rigidbodies;

	// // these are in bytes, and 32768 is 32KiB
	// public float MaxPacketSize = 32768;

	// // this can grow depending on how many objects the sim server can
	// // handle to send across to all clients with the current sim
	// public float MaxObjects = 32;

	// public float FullSimulationStatePacketSize = 768;

	void Awake()
	{
		rigidbodies = new List<NetSynced.Rigidbody3D>();
		foreach (GameObject rootGameObject in gameObject.scene.GetRootGameObjects())
		{
			rigidbodies.AddRange(rootGameObject.GetComponentsInChildren<NetSynced.Rigidbody3D>());
		}
	}

	public void HandleContext(Serializable.Context3D context)
	{
		foreach (Serializable.Transform st in context.Transforms)
		{
			NetSynced.Transform t;
			if (clientContextManager.worldOwnedTransformsByGUID.TryGetValue(st.ID, out t))
			{
				t.transform.position = st.Position.ToUnityVector();
			}
		}
		foreach (Serializable.Rigidbody3D sr in context.RigidBodies)
		{
			NetSynced.Rigidbody3D r;
			if (clientContextManager.worldOwnedRigidbodiesByGUID.TryGetValue(sr.ID, out r))
			{
				r.Sync(sr.Position.ToUnityVector(), sr.Rotation.ToUnityQuaterion(), sr.Velocity.ToUnityVector());
			}
		}
		Debug.Log("Synced " + context.RigidBodies.Count + " rigidbodies");
	}

	public void SendContext(int tick)
	{
		Serializable.Context3D context = new Serializable.Context3D
		{
			Tick = tick,
			RigidBodies = { clientContextManager.playerOwnedRigidbodiesByGUID.Select(r => r.Value.Export()) },
		};
		// Debug.Log(context.RigidBodies.Count);
		client.Send(context);
	}
}
