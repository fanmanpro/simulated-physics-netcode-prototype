using UnityEngine;

namespace ThreadedNetworkProtocol
{
	// rename this to something that is more like a result that can have a log, warning, error, of fatal return type
	public interface ILog
	{
		void Print();
	}
	public class Message
	{
		public string message = "";

		public override string ToString()
		{
			return message;
		}
	}
	public class Error : Message, ILog
	{
		public Error(string m, params object[] a)
		{
			message = string.Format(m, a);
		}

		public void Print()
		{
			Debug.LogError(message);
		}
	}
	public class Warning : Message, ILog
	{
		public Warning(string m, params object[] a)
		{
			message = string.Format(m, a);
		}

		public void Print()
		{
			Debug.LogWarning(message);
		}
	}
	public class Log : Message, ILog
	{
		public Log(string m, params object[] a)
		{
			message = string.Format(m, a);
		}

		public void Print()
		{
			Debug.Log(message);
		}
	}
}
