using System.Linq;
using System.Net.NetworkInformation;
using Serializable;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreadedNetworkProtocol
{
	public class ClientConnectionHandler : MonoBehaviour, IConnectionHandler
	{
		public Client client;

		public void Disconnect()
		{
			client.UDPTryDisconnect();
			client.TCPTryDisconnect();
			SceneManager.UnloadSceneAsync(gameObject.scene);
		}

		public void HandleConnect()
		{
			Debug.Log("sending client mac");

			client.Send(new Serializable.Packet()
			{
				OpCode = Packet.Types.OpCode.ClientMac,
				Data = Google.Protobuf.WellKnownTypes.Any.Pack(new ClientMAC()
				{
					// using mac address for now but later we will use a unique game
					// instance id (typically coming from platforms like steam)
					// well, we just need some identifier that can be used that
					// allows the player to reconnect
					MAC = GetMacAddress()
				}),
			});
		}

		public void HandleDisconnect()
		{
		}

		public void HandleReconnect()
		{
		}

		private string GetMacAddress()
		{
			const int MIN_MAC_ADDR_LENGTH = 12;
			string macAddress = string.Empty;
			long maxSpeed = -1;

			foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
			{
				string tempMac = nic.GetPhysicalAddress().ToString();
				if (nic.Speed > maxSpeed && !string.IsNullOrEmpty(tempMac) && tempMac.Length >= MIN_MAC_ADDR_LENGTH)
				{
					maxSpeed = nic.Speed;
					macAddress = tempMac;
				}
			}
			return macAddress;
		}
	}
}
