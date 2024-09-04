using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lockstep.Network
{
	internal class TcpConnectService : TcpService, IConnectService
	{
		public void StartAsClient()
		{
		}

		/// <summary>
		/// 连接到服务端
		/// </summary>
		/// <param name="ipEndPoint"></param>
		/// <returns></returns>
		public AChannel ConnectChannel(IPEndPoint ipEndPoint)
		{
			TcpClient tcpClient = new TcpClient();
			TcpConnectChannel channel = new TcpConnectChannel();
			channel.Start(tcpClient, ipEndPoint, this);
			this.idChannels[channel.Id] = channel;

			return channel;
		}
	}
}
