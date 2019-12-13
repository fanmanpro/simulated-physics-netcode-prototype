using System;

namespace Netcode
{
	public static class GUID
	{
		public static string Gen()
		{
			return Guid.NewGuid().ToString();
		}
	}
}
