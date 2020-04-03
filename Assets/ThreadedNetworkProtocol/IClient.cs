using System.Threading.Tasks;
using Google.Protobuf;
using ThreadedNetworkProtocol;

public interface IClient
{
	Task<Error> Connect();
	Error Disconnect();
	Task<Error> Send(byte[] p);
	Task<Error> Listen();
}
