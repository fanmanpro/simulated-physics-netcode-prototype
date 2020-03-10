using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using ThreadedNetworkProtocol;
using UnityEngine;

public class ClientContextHandler : MonoBehaviour, IContextHandler
{
	public Client client;
	public ContextManager ContextManager;

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
			NetSynced.Transform t;
			if (ContextManager.worldOwnedTransformsByGUID.TryGetValue(st.ID, out t))
			{
				t.transform.position = st.Position.ToUnityVector();
				//t.transform.rotation = st.Rotation;
				//hendrik you are here, busy reading a context for a client based on what they know of object ownership using contextmanager

				//t.position = rbMessage.Position.ToUnityVector();
				//rb.rigidbody.velocity = rbMessage.Velocity.ToUnityVector();
				//rb.rigidbody.rotation = rbMessage.Rotation;
				//Debug.Log("[GAM] [Context] " + rb.rigidbody.position);
			}
		}
		foreach (Serializable.Rigidbody sr in context.RigidBodies)
		{
			NetSynced.Rigidbody2D r;
			if (ContextManager.worldOwnedRigidbodiesByGUID.TryGetValue(sr.ID, out r))
			{
				//Vector2 currentPosition = r.rigidbody.position;
				//Vector2 endPosition = 

				r.Sync(sr.Position.ToUnityVector(), sr.Velocity.ToUnityVector());

				//r.Set.rigidbody.position = Vector3.Lerp(currentPosition, endPosition, syncTime / syncDelay);

				//hendrik you are here, busy reading a context for a client based on what they know of object ownership using contextmanager

				//t.position = rbMessage.Position.ToUnityVector();
				//rb.rigidbody.velocity = rbMessage.Velocity.ToUnityVector();
				//rb.rigidbody.rotation = rbMessage.Rotation;
				//Debug.Log("[GAM] [Context] " + rb.rigidbody.position);
			}
		}
	}
}
