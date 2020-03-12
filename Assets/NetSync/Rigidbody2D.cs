using System;
using System.ComponentModel;
using Netcode;
using UnityEngine;

namespace NetSynced
{
	[RequireComponent(typeof(UnityEngine.Rigidbody2D), typeof(NetSync))]
	public class Rigidbody2D : MonoBehaviour
	{
		//public GameServer GameServer;
		public new UnityEngine.Rigidbody2D rb;
		private NetSync netSync;
		public string GUID => netSync.GUID;

		//private Vector3 syncStartPosition = Vector3.zero;
		private Vector3 syncEndPosition = Vector3.zero;

		// copies of Rigidbody2D
		private Vector2 syncStartPosition = Vector3.zero;
		//public Vector2 Position { get => syncStartPosition; set => setPosition(value); }
		float t;
		const float serverTicksPerSecond = 20;
		float timeToReachTarget = 1.0f / serverTicksPerSecond;

		private bool doUpdate = false;

		// storage objects to avoid constant memory allocation and deallocation. e.g. "new" keyword abuse.

		void Awake()
		{
			netSync = GetComponent<NetSync>();
			if (netSync == null)
			{
				Debug.LogWarning("Rigidbody2D must contain NetSync component to sync across network.");
				enabled = false;
				return;
			}

			doUpdate = false;
			syncEndPosition = transform.position;

			rb = GetComponent<UnityEngine.Rigidbody2D>();
		}

		// using this is never really good for multiplayer games because it means sending too many packets.
		public void Sync(Vector2 p, Vector2 v)
		{
			doUpdate = true;

			t = 0;
			syncStartPosition = GetComponent<Rigidbody>().position;
			syncEndPosition = p;
		}

		private void FixedUpdate()
		{
			if (!doUpdate) return;

			t += Time.deltaTime / timeToReachTarget;

			rb.position = Vector3.Lerp(syncStartPosition, syncEndPosition, t);
		}

		public Serializable.Rigidbody2D Export()
		{
			return new Serializable.Rigidbody2D
			{
				ID = netSync.GUID,
				Position = new Serializable.Vector2
				{
					X = rb.position.x,
					Y = rb.position.y,
				},
				Velocity = new Serializable.Vector2
				{
					X = rb.velocity.x,
					Y = rb.velocity.y,
				},
			};
		}
	}
}