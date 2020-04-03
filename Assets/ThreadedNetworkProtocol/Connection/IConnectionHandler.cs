namespace ThreadedNetworkProtocol
{
	public interface IConnectionHandler
	{
		void HandleConnect();
		void HandleReconnect();
		void HandleDisconnect();
		void Disconnect(); // this is just so we can safely disconnect because Unity doesn't properly close sockets when stopping Run in the editor
	}
}