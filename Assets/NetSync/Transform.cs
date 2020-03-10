using System;
//using Assets.Netcode.Helpers;
using UnityEngine;

namespace NetSynced
{
	public class Transform : MonoBehaviour
	{
		//public UDPConnection UDPConnection;

		public string GUID;

		// copies of transform
		private Vector2 position;
		public Vector2 Position { get => position; set => setPosition(value); }

		void Reset()
		{
			//Guid = GUID.Gen();
		}

		private void Awake()
		{
			//UDPConnection.Add(Guid, this);
		}

		private void setPosition(Vector2 v)
		{
			//UDPConnection.Send(Gamedata.Header.Types.OpCode.Position, new Gamedata.Position { ID = Guid, X = v.x, Y = v.y });
			//velocity = v;
		}

		public void AddForce(Vector2 f, ForceMode2D mode = ForceMode2D.Force, bool sync = true)
		{
			if (sync)
			{
				//UDPConnection.Send(Gamedata.Header.Types.OpCode.Force, new Gamedata.Force { ID = Guid, X = f.x, Y = f.y, ForceMode = (Gamedata.Force.Types.ForceMode)(int)mode });
			}
			//transform.AddForce(f, mode);
		}
	}
}