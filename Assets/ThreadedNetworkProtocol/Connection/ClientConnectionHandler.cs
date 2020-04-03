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
		}

		public void HandleDisconnect()
		{
		}

		public void HandleReconnect()
		{
		}
	}
}
