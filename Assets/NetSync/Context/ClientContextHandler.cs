using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using ThreadedNetworkProtocol;
using UnityEngine;

public class ClientContextHandler : MonoBehaviour, IContextHandler
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
		foreach (Serializable.Transform st in context.Transforms)
		{
			Transform t;
			if (contextManager.worldOwnedRigidbodiesByGUID.TryGetValue(st.ID, out t))
			{
				 hendrik you are here, busy reading a context for a client based on what they know of object ownership using contextmanager

			//t.position = rbMessage.Position.ToUnityVector();
				//rb.rigidbody.velocity = rbMessage.Velocity.ToUnityVector();
				//rb.rigidbody.rotation = rbMessage.Rotation;
				//Debug.Log("[GAM] [Context] " + rb.rigidbody.position);
			}
		}
	}
}
