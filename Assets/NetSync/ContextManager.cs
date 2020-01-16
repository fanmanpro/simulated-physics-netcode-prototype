using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ThreadedNetworkProtocol;

public class ContextManager : MonoBehaviour
{
	public Dictionary<string, NetSynced.Rigidbody2D> rigidbodiesByGUID;

	void Start()
	{
		rigidbodiesByGUID = new Dictionary<string, NetSynced.Rigidbody2D>();
		foreach (GameObject rootGameObject in gameObject.scene.GetRootGameObjects())
		{
			NetSynced.Rigidbody2D[] rigidbodies = rootGameObject.GetComponentsInChildren<NetSynced.Rigidbody2D>();
			foreach (NetSynced.Rigidbody2D rigidbody in rigidbodies)
			{
				rigidbodiesByGUID.Add(rigidbody.GUID, rigidbody);
			}
		}
	}
}
