using UnityEngine;

public class NetSync : MonoBehaviour
{
	//public GameServer GameServer;
	public string GUID;

	void Reset()
	{
		GUID = Netcode.GUID.Gen();
	}

	void Start()
	{
		//GameServer.AddNetSync(GUID, gameObject);
	}
}
