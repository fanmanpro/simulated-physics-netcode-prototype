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
		public new UnityEngine.Rigidbody2D rigidbody;
		private NetSync netSync;
		public string GUID => netSync.GUID;

		private float lastSynchronizationTime = 0f;
		public float syncDelay = 0f;
		public float syncTime = 0f;
		//private Vector3 syncStartPosition = Vector3.zero;
		private Vector3 syncEndPosition = Vector3.zero;

		// copies of Rigidbody2D
		private Vector2 syncStartPosition = Vector3.zero;
		//public Vector2 Position { get => syncStartPosition; set => setPosition(value); }

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

			lastSynchronizationTime = Time.time;
			doUpdate = false;

			rigidbody = GetComponent<UnityEngine.Rigidbody2D>();
		}

		// using this is never really good for multiplayer games because it means sending too many packets.
		public void Sync(Vector2 p, Vector2 v)
		{
			doUpdate = true;

			syncTime = 0f;
			//syncDelay = Time.time - lastSynchronizationTime;
			syncDelay = 0.5f;
			lastSynchronizationTime = Time.time;

			syncEndPosition = p * syncDelay;
			syncStartPosition = rigidbody.position;
			Debug.Log(updatesBetweenSyncs);
			updatesBetweenSyncs = 0;
		}

		public float updatesBetweenSyncs = 0;
		private void Update()
		{
			if (!doUpdate) return;
			// Time.deltaTime = 0.01f
			// syncDelay = 0.5f
			// 
			updatesBetweenSyncs += (1 );


// 


			//syncTime += Time.deltaTime;
			//Debug.Log(updatesBetweenSyncs / Time.deltaTime);
			rigidbody.position = Vector3.Lerp(syncStartPosition, syncEndPosition, 1);
		}

		public Serializable.Rigidbody Export()
		{
			return new Serializable.Rigidbody
			{
				ID = netSync.GUID,
				Position = new Serializable.Vector3
				{
					X = rigidbody.position.x,
					Y = rigidbody.position.y,
				},
				Velocity = new Serializable.Vector2
				{
					X = rigidbody.velocity.x,
					Y = rigidbody.velocity.y,
				},
			};
		}
	}
}