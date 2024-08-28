using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Lockstep.Network
{
	public sealed class TcpService: AService
	{
		private TcpListener acceptor;

		private readonly Dictionary<long, TcpChannel> idChannels = new Dictionary<long, TcpChannel>();
		
		/// <summary>
		/// 当作服务端启动？
		/// </summary>
		public TcpService(IPEndPoint ipEndPoint)
		{
			this.acceptor = new TcpListener(ipEndPoint);
			this.acceptor.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			this.acceptor.Server.NoDelay = true;
			this.acceptor.Start();
		}

		/// <summary>
		/// 当作客户端启动？
		/// </summary>
		public TcpService()
		{
		}

		public override void Dispose()
		{
			if (this.acceptor == null)
			{
				return;
			}

			foreach (long id in this.idChannels.Keys.ToArray())
			{
				TcpChannel channel = this.idChannels[id];
				channel.Dispose();
			}
			this.acceptor.Stop();
			this.acceptor = null;
		}
		
		public override AChannel GetChannel(long id)
		{
			TcpChannel channel = null;
			this.idChannels.TryGetValue(id, out channel);
			return channel;
		}

		/// <summary>
		/// 服务器调用，监听客户端连接
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public override async Task<AChannel> AcceptChannel()
		{
			if (this.acceptor == null)
			{
				throw new Exception("service construct must use host and port param");
			}
			// https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.tcplistener.accepttcpclientasync?view=net-8.0
			// https://learn.microsoft.com/zh-cn/dotnet/fundamentals/networking/sockets/tcp-classes?redirectedfrom=MSDN
			TcpClient tcpClient = await this.acceptor.AcceptTcpClientAsync();
			TcpChannel channel = new TcpChannel(tcpClient, this);
			this.idChannels[channel.Id] = channel;
			return channel;
		}

		/// <summary>
		/// 连接到服务端
		/// </summary>
		/// <param name="ipEndPoint"></param>
		/// <returns></returns>
		public override AChannel ConnectChannel(IPEndPoint ipEndPoint)
		{
			TcpClient tcpClient = new TcpClient();
			TcpChannel channel = new TcpChannel(tcpClient, ipEndPoint, this);
			this.idChannels[channel.Id] = channel;

			return channel;
		}


		public override void Remove(long id)
		{
			TcpChannel channel;
			if (!this.idChannels.TryGetValue(id, out channel))
			{
				return;
			}
			if (channel == null)
			{
				return;
			}
			this.idChannels.Remove(id);
			channel.Dispose();
		}

		public override void Update()
		{
		}
	}
}