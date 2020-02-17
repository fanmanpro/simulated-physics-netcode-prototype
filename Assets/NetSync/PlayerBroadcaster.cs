using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ThreadedNetworkProtocol;

public class PlayerBroadcaster : MonoBehaviour
{
	public Client client;
	private List<NetSynced.Rigidbody2D> rigidbodies;


	void Start()
	{
		rigidbodies = new List<NetSynced.Rigidbody2D>();
		enabled = false;
	}

	public void AddRigidbody(NetSynced.Rigidbody2D rigidbody)
	{
		rigidbodies.Add(rigidbody);
		enabled = true;
	}

	void FixedUpdate()
	{
		Gamedata.Context context = new Gamedata.Context
		{
			RigidBodies = { rigidbodies.Select(r => r.Export()) }
		};
		client.Send(PacketType.Unreliable, new Gamedata.Packet { OpCode = Gamedata.Header.Types.OpCode.Context, Data = Any.Pack(context) });
	}
}
