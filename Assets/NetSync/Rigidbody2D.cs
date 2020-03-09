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

		// copies of Rigidbody2D
		//private Vector2 velocity;
		//public Vector2 Velocity { get => velocity; set => SetVelocity(value); }
		//private Vector2 position;
		//public Vector2 Position { get => position; set => setPosition(value); }

		// storage objects to avoid constant memory allocation and deallocation. e.g. "new" keyword abuse.
		//private Gamedata.Velocity velocityStorage;
		//private Gamedata.Force forceStorage;

		//private string guid;

		void Awake()
		{
			netSync = GetComponent<NetSync>();
			if (netSync == null)
			{
				Debug.LogWarning("Rigidbody2D must contain NetSync component to sync across network.");
				enabled = false;
				return;
			}
			//velocityStorage = new Gamedata.Velocity { ID = netSync.GUID, X = 0, Y = 0 };
			//forceStorage = new Gamedata.Force { ID = netSync.GUID, X = 0, Y = 0 };

			rigidbody = GetComponent<UnityEngine.Rigidbody2D>();
			//GameServer.AddRigidbody(netSync.GUID, this);
		}


		//public void SetVelocity(Vector2 v, bool sync = true)
		//{
		//	if (sync)
		//	{
		//		velocityStorage.X = v.x;
		//		velocityStorage.Y = v.y;
		//		//GameServer.Sync(velocityStorage);
		//	}
		//	//if (sync) UDPConnection.Send(Gamedata.Header.Types.OpCode.Velocity, new Gamedata.Velocity { ID = Guid, X = v.x, Y = v.y });
		//	velocity = v;
		//	rigidbody.velocity = v;
		//}

		//// using this is never really good for multiplayer games because it means sending too many packets.
		//private void setPosition(Vector2 p, bool sync = true)
		//{
		//	//if (sync) UDPConnection.Send(Gamedata.Header.Types.OpCode.Position, new Gamedata.Position { ID = Guid, X = p.x, Y = p.y });
		//	position = p;
		//	rigidbody.position = p;
		//}

		//public void AddForce(Vector2 f, ForceMode2D mode = ForceMode2D.Force, bool sync = true)
		//{
		//	if (sync)
		//	{
		//		forceStorage.X = f.x;
		//		forceStorage.Y = f.y;
		//		forceStorage.ForceMode = (Gamedata.Force.Types.ForceMode)(int)mode;
		//		//GameServer.Sync(forceStorage);
		//	}
		//	//if (sync) UDPConnection.Send(Gamedata.Header.Types.OpCode.Force, new Gamedata.Force { ID = Guid, X = f.x, Y = f.y, ForceMode = (Gamedata.Force.Types.ForceMode)(int)mode });
		//	rigidbody.AddForce(f, mode);
		//}

		public Serializable.Transform Export()
		{
			//if (!rigidbody.IsAwake()) return null;
			return new Serializable.Transform
			{
				//ID = netSync.GUID,
				//Prefab = Gamedata.Prefab.DemoBox,
				Position = new Serializable.Vector2
				{
					X = rigidbody.position.x,
					Y = rigidbody.position.y,
				},
				//Rotation = rigidbody.rotation,
				//Velocity = new Gamedata.Vector2
				//{
				//	X = rigidbody.velocity.x,
				//	Y = rigidbody.velocity.y,
				//},
			};
		}
	}
}