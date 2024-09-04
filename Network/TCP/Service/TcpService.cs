using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Lockstep.Network
{
	public class TcpService: AService
	{
		protected readonly Dictionary<long, TcpChannel> idChannels = new Dictionary<long, TcpChannel>();
		
		public TcpService()
		{
		}
		

		public override void Dispose()
		{
			foreach (long id in this.idChannels.Keys.ToArray())
			{
				TcpChannel channel = this.idChannels[id];
				channel.Dispose();
			}
		}
		
		public override AChannel GetChannel(long id)
		{
			TcpChannel channel = null;
			this.idChannels.TryGetValue(id, out channel);
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