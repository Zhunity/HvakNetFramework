using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Lockstep.Network
{
	internal class TcpAcceptService : TcpService, IAcceptService
	{
		private TcpListener acceptor;

		public void StartAsServer(IPEndPoint ipEndPoint)
		{
			this.acceptor = new TcpListener(ipEndPoint);
			this.acceptor.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			this.acceptor.Server.NoDelay = true;
			this.acceptor.Start();
		}

		/// <summary>
		/// 服务器调用，监听客户端连接
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public async Task<AChannel> AcceptChannel()
		{
			if (this.acceptor == null)
			{
				throw new Exception("service construct must use host and port param");
			}
			// https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.tcplistener.accepttcpclientasync?view=net-8.0
			// https://learn.microsoft.com/zh-cn/dotnet/fundamentals/networking/sockets/tcp-classes?redirectedfrom=MSDN
			TcpClient tcpClient = await this.acceptor.AcceptTcpClientAsync();
			TcpAcceptChannel channel = new TcpAcceptChannel();
			channel.Start(tcpClient, this);
			this.idChannels[channel.Id] = channel;
			return channel;
		}

		public override void Dispose()
		{
			base.Dispose();
			if (this.acceptor == null)
			{
				return;
			}
			this.acceptor.Stop();
			this.acceptor = null;
		}

	}
}
