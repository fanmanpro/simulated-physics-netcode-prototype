using System.Threading.Tasks;
using ThreadedNetworkProtocol;

public interface IClient
{
	Task<Error> Connect();
	Error Disconnect();
	Task<Error> Send(Gamedata.Packet p);
	Task<Error> Listen();
}
