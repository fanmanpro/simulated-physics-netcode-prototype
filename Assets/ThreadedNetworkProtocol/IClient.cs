using System.Threading.Tasks;
using Google.Protobuf;
using ThreadedNetworkProtocol;

public interface IClient
{
	Task<ILog> Connect();
	ILog Disconnect();
	Task<ILog> Send(byte[] p);
	Task<ILog> Listen();
}
