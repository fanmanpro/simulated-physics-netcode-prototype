using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SimulationContextManager : MonoBehaviour 
{
	public Dictionary<string, NetSynced.Rigidbody3D> worldOwnedRigidbodiesByGUID;
	public Dictionary<string, NetSynced.Transform> worldOwnedTransformsByGUID;
	public List<string> worldOwnedGuids;

	void Start()
	{
		worldOwnedRigidbodiesByGUID = new Dictionary<string, NetSynced.Rigidbody3D>();
		worldOwnedTransformsByGUID = new Dictionary<string, NetSynced.Transform>();

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
		worldOwnedGuids.AddRange(worldOwnedTransformsByGUID.Select(s => s.Key).ToList());
	}
}
