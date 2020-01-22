using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreadedNetworkProtocol;

public class ContextBroadcaster : MonoBehaviour
{
	public Client client;
	private List<NetSynced.Rigidbody2D> rigidbodies;
	public bool Started = false;

	// these are in bytes, and 32768 is 32KiB
	public float MaxPacketSize = 32768;

	// this can grow depending on how many objects the sim server can
	// handle to send across to all clients with the current sim
	public float MaxObjects = 32;

	public float FullSimulationStatePacketSize = 768;
	void FixedUpdate()
	{
		prepareContext();
	}

	void Start()
	{
		Application.targetFrameRate = 60;
		rigidbodies = new List<NetSynced.Rigidbody2D>();
		foreach (GameObject rootGameObject in gameObject.scene.GetRootGameObjects())
		{
			rigidbodies.AddRange(rootGameObject.GetComponentsInChildren<NetSynced.Rigidbody2D>());
		}
		StartCoroutine(awaitStart());
	}

	IEnumerator awaitStart()
	{
		yield return new WaitUntil(() => { return Started; });
		//InvokeRepeating("prepareContext", 0, 0.3333f);
	}

	void prepareContext()
	{
		List<Gamedata.Rigidbody> rigidBodies = new List<Gamedata.Rigidbody>();
		int offset = 0;
		//for (int r = 0; r < GameServer.RigidbodyByGUID.Count; r++)
		int r = 0;
		foreach (NetSynced.Rigidbody2D rb in rigidbodies)
		{
			if ((r - offset + 1) * 30 > FullSimulationStatePacketSize)
			{
				sendContext(rigidBodies);
				rigidBodies.Clear();
				offset = r;
			}

			//if (!rigidbody.IsAwake()) continue;

			rigidBodies.Add(rb.Export());
			r++;
		}
		if (rigidBodies.Count > 0)
		{
			sendContext(rigidBodies);
			rigidBodies.Clear();
		}
	}
	void sendContext(List<Gamedata.Rigidbody> rigidBodies)
	{
		Gamedata.Context context = new Gamedata.Context
		{
			RigidBodies = { rigidBodies }
		};
		client.Send(PacketType.Unreliable, new Gamedata.Packet { OpCode = Gamedata.Header.Types.OpCode.Context, Data = Any.Pack(context) });
	}
}
