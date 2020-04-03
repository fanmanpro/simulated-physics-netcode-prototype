using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ClientContextManager : MonoBehaviour 
{
	public Dictionary<string, NetSynced.Rigidbody3D> worldOwnedRigidbodiesByGUID;
	public Dictionary<string, NetSynced.Rigidbody3D> playerOwnedRigidbodiesByGUID;
	public Dictionary<string, NetSynced.Transform> worldOwnedTransformsByGUID;
	public Dictionary<string, NetSynced.Transform> playerOwnedTransformsByGUID;
	public List<string> worldOwnedGuids;
	public List<string> playerOwnedGuids;

	void Start()
	{
		worldOwnedRigidbodiesByGUID = new Dictionary<string, NetSynced.Rigidbody3D>();
		playerOwnedRigidbodiesByGUID = new Dictionary<string, NetSynced.Rigidbody3D>();
		worldOwnedTransformsByGUID = new Dictionary<string, NetSynced.Transform>();
		playerOwnedTransformsByGUID = new Dictionary<string, NetSynced.Transform>();

		foreach (GameObject rootGameObject in gameObject.scene.GetRootGameObjects())
		{
			foreach (NetSynced.Rigidbody3D r in rootGameObject.GetComponentsInChildren<NetSynced.Rigidbody3D>())
			{
				worldOwnedRigidbodiesByGUID.Add(r.GUID, r);
			}
			foreach (NetSynced.Transform t in rootGameObject.GetComponentsInChildren<NetSynced.Transform>())
			{
				worldOwnedTransformsByGUID.Add(t.GUID, t);
			}
		}
		// lazy way of just refreshing the lists
		worldOwnedGuids = worldOwnedRigidbodiesByGUID.Select(s => s.Key).ToList();
		playerOwnedGuids = playerOwnedRigidbodiesByGUID.Select(s => s.Key).ToList();
		worldOwnedGuids.AddRange(worldOwnedTransformsByGUID.Select(s => s.Key).ToList());
		playerOwnedGuids.AddRange(playerOwnedTransformsByGUID.Select(s => s.Key).ToList());
	}

	public void RegisterPlayerOwned(string guid)
	{
		NetSynced.Rigidbody3D r;
		if (!worldOwnedRigidbodiesByGUID.TryGetValue(guid, out r))
			return;

		r.DebugIsPlayerOwned = true;

		playerOwnedRigidbodiesByGUID.Add(guid, r);
		worldOwnedRigidbodiesByGUID.Remove(guid);

		// lazy way of just refreshing the lists
		worldOwnedGuids = worldOwnedRigidbodiesByGUID.Select(s => s.Key).ToList();
		playerOwnedGuids = playerOwnedRigidbodiesByGUID.Select(s => s.Key).ToList();
		worldOwnedGuids.AddRange(worldOwnedTransformsByGUID.Select(s => s.Key).ToList());
		playerOwnedGuids.AddRange(playerOwnedTransformsByGUID.Select(s => s.Key).ToList());
	}
}
