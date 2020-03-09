using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ThreadedNetworkProtocol;

public class ContextManager : MonoBehaviour
{
	public Client client;
	public Dictionary<string, NetSynced.Rigidbody2D> worldOwnedRigidbodiesByGUID;
	public Dictionary<string, NetSynced.Rigidbody2D> playerOwnedRigidbodiesByGUID;
	public List<string> worldOwnedGuids;
	public List<string> playerOwnedGuids;

	void Start()
	{
		worldOwnedRigidbodiesByGUID = new Dictionary<string, NetSynced.Rigidbody2D>();
		playerOwnedRigidbodiesByGUID = new Dictionary<string, NetSynced.Rigidbody2D>();
		foreach (GameObject rootGameObject in gameObject.scene.GetRootGameObjects())
		{
			NetSynced.Rigidbody2D[] rigidbodies = rootGameObject.GetComponentsInChildren<NetSynced.Rigidbody2D>();
			foreach (NetSynced.Rigidbody2D rigidbody in rigidbodies)
			{
				worldOwnedRigidbodiesByGUID.Add(rigidbody.GUID, rigidbody);
			}
		}
		worldOwnedGuids = worldOwnedRigidbodiesByGUID.Select(s => s.Key).ToList();
		playerOwnedGuids = playerOwnedRigidbodiesByGUID.Select(s => s.Key).ToList();
	}

	public void RegisterPlayerOwned(string guid)
	{
		NetSynced.Rigidbody2D r;
		if (!worldOwnedRigidbodiesByGUID.TryGetValue(guid, out r))
			return;
		playerOwnedRigidbodiesByGUID.Add(guid, r);
		worldOwnedRigidbodiesByGUID.Remove(guid);
		worldOwnedGuids = worldOwnedRigidbodiesByGUID.Select(s => s.Key).ToList();
		playerOwnedGuids = playerOwnedRigidbodiesByGUID.Select(s => s.Key).ToList();
	}

	void FixedUpdate()
	{
		//Gamedata.Context context = new Gamedata.Context
		//{
		//	RigidBodies = { playerOwnedRigidbodiesByGUID.Select(r => r.Value.Export()) },
		//};
		//Debug.Log(context.RigidBodies.Count);
		//client.Send(PacketType.Unreliable, new Gamedata.Packet { OpCode = Gamedata.Header.Types.OpCode.Context, Data = Any.Pack(context) });
	}
}
