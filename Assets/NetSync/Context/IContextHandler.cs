public interface IContextHandler
{
	void HandleContext(Serializable.Context3D c);
	void SendContext(int tick);
}
