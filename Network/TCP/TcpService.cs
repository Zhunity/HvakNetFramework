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
		/// 即可做client也可做server
		/// </summary>
		public TcpService(IPEndPoint ipEndPoint)
		{
			this.acceptor = new TcpListener(ipEndPoint);
			this.acceptor.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			this.acceptor.Server.NoDelay = true;
			this.acceptor.Start();
		}

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

		public override async Task<AChannel> AcceptChannel()
		{
			if (this.acceptor == null)
			{
				throw new Exception("service construct must use host and port param");
			}
			TcpClient tcpClient = await this.acceptor.AcceptTcpClientAsync();
			TcpChannel channel = new TcpChannel(tcpClient, this);
			this.idChannels[channel.Id] = channel;
			return channel;
		}

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