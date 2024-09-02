using System.Collections.Generic;
using System.Net;

namespace Lockstep.Network
{
	public static class NetworkUtil
	{
		public static IPEndPoint ToIPEndPoint(string host, int port)
		{
			return new IPEndPoint(IPAddress.Parse(host), port);
		}

		public static IPEndPoint ToIPEndPoint(string address)
		{
			int index = address.LastIndexOf(':');
			string host = address.Substring(0, index);
			string p = address.Substring(index + 1);
			int port = int.Parse(p);
			return ToIPEndPoint(host, port);
		}
	}

	public static class OpcodeHelper
	{
		private static readonly HashSet<ushort> needDebugLogMessageSet = new HashSet<ushort> { 1 };

		public static bool IsNeedDebugLogMessage(ushort opcode)
		{
			if (opcode > 1000)
			{
				return true;
			}

			if (needDebugLogMessageSet.Contains(opcode))
			{
				return true;
			}

			return false;
		}

		public static bool IsClientHotfixMessage(ushort opcode)
		{
			return opcode > 10000;
		}
	}
}
