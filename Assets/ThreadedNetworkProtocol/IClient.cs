using System.Threading.Tasks;
using ThreadedNetworkProtocol;

public interface IClient
{
	Task<Error> Connect();
	Error Disconnect();
	Task<Error> Send(Serializable.Context3D p);
	Task<Error> Listen();
}
