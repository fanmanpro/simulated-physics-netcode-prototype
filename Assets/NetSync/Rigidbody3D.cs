using System;
using System.ComponentModel;
using Netcode;
using UnityEngine;

namespace NetSynced
{
	[RequireComponent(typeof(UnityEngine.Rigidbody), typeof(NetSync))]
	public class Rigidbody3D : MonoBehaviour
	{
		//public GameServer GameServer;
		public new UnityEngine.Rigidbody rb;
		private NetSync netSync;
		public string GUID => netSync.GUID;

		private Vector3 syncEndPosition = Vector3.zero;
		private Vector2 syncStartPosition = Vector3.zero;
		private Quaternion syncEndRotation = Quaternion.identity;
		private Quaternion syncStartRotation = Quaternion.identity;

		float t;
		const float serverTicksPerSecond = 20;
		float timeToReachTarget = 1.0f / serverTicksPerSecond;

		private bool doUpdate = false;

		public bool DebugIsPlayerOwned = false;

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
			syncEndRotation = transform.rotation;

			rb = GetComponent<UnityEngine.Rigidbody>();
		}

		// using this is never really good for multiplayer games because it means sending too many packets.
		public void Sync(Vector3 p, Quaternion r, Vector3 v)
		{
			if (DebugIsPlayerOwned) {
				Debug.Log("Player owned is syncing from sim!");
			}
			doUpdate = true;

			t = 0;
			syncStartPosition = rb.position;
			syncEndPosition = p;

			syncStartRotation = rb.rotation;
			syncEndRotation = r;
		}

		private void FixedUpdate()
		{
			if (!doUpdate) return;

			t += Time.deltaTime * 10;// / timeToReachTarget;

			//rb.MovePosition(Vector3.Lerp(syncStartPosition, syncEndPosition, t));
			//rb.MoveRotation(Quaternion.Lerp(syncStartRotation, syncEndRotation, t));
			rb.MovePosition(syncEndPosition);
			rb.MoveRotation(syncEndRotation);
		}

		public Serializable.Rigidbody3D Export()
		{
			return new Serializable.Rigidbody3D
			{
				ID = netSync.GUID,
				Position = new Serializable.Vector3
				{
					X = rb.position.x,
					Y = rb.position.y,
					Z = rb.position.z,
				},
				Rotation = new Serializable.Quaternion
				{
					W = rb.rotation.w,
					X = rb.rotation.x,
					Y = rb.rotation.y,
					Z = rb.rotation.z,
				},
				Velocity = new Serializable.Vector3
				{
					X = rb.velocity.x,
					Y = rb.velocity.y,
					Z = rb.velocity.z,
				},
			};
		}
	}
}