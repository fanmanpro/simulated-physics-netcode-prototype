namespace ThreadedNetworkProtocol
{
	// rename this to something that is more like a result that can have a log, warning, error, of fatal return type
	public class Error
	{
		public string message = "";
		public Error(string m) {
			message = m;
		}
		public Error(string m, params object[] a) {
			message = string.Format(m, a);
		}
		public override string ToString()
		{
			return message;
		}
	}
}
