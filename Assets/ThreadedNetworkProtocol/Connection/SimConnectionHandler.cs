using System.Linq;
using Google.Protobuf.Collections;
using Serializable;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreadedNetworkProtocol
{
	public class SimConnectionHandler : MonoBehaviour, IConnectionHandler
	{
		public Client client;
		public SeatManager seatManager;

		public void Disconnect()
		{
			client.UDPTryDisconnect();
			client.TCPTryDisconnect();
			SceneManager.UnloadSceneAsync(gameObject.scene);
		}

		public void HandleConnect()
		{
			RepeatedField<Serializable.Seat> seats = new RepeatedField<Serializable.Seat>();
			seats.AddRange(seatManager.seats.Select(s => new Serializable.Seat() {
				GUID = s.GUID
			}));
			foreach(Serializable.Seat guid in seats) {
				Debug.Log("Doing seating for " + guid.GUID);
			}
			client.Send(new Serializable.Packet()
			{
				OpCode = Packet.Types.OpCode.SeatConfiguration,
				Data = Google.Protobuf.WellKnownTypes.Any.Pack(new SeatConfiguration()
				{
					Seats = { seats }
				}),
			});
			Debug.Log("Connection handled...");
		}

		public void HandleDisconnect()
		{
		}

		public void HandleReconnect()
		{
		}
	}
}
