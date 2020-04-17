using Serializable;
using ThreadedNetworkProtocol;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimPacketHandler : MonoBehaviour, IPacketHandler
{
	public Client client;
	public SimulationContextHandler simulationContextHandler;
	public void HandlePacket(Packet p)
	{
		switch (p.OpCode)
		{
			case Serializable.Packet.Types.OpCode.RunSimulation:
				{
					Serializable.RunSimulation runSimulation;
					if (!p.Data.TryUnpack(out runSimulation)) return;

					// Debug.Log("sim received run simulation packet " + runSimulation.Tick);
					client.UDPTryConnect();
					simulationContextHandler.SendContext(runSimulation.Tick);
				}
				break;
			case Serializable.Packet.Types.OpCode.ReloadSimulation:
				{
					/* 
					figure out how to properly dispose the connections
						issues:
						* stream.ReadAsync is locking a thread with await, then stream gets destroyed, so its throws
					create a scene "manager" that stays active and reloads the correct scene
					start the sim paused, unpause it when seats are loaded
					have a "player X joined" connection feed announcement
					*/
					//  AsyncOperation unloadScene = SceneManager.UnloadSceneAsync(gameObject.scene.buildIndex);
					//  unloadScene.completed += (s) =>
					//  {
						// SceneManager.LoadSceneAsync(gameObject.scene.buildIndex, LoadSceneMode.Single);
					//  };
				}
				break;
		}
	}
}
