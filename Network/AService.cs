using System;
using System.Net;
using System.Threading.Tasks;

namespace Lockstep.Network
{
	/// <summary>
	/// 网络连接类型，TCP， UDP
	/// </summary>
	public enum NetworkProtocol
	{
		TCP,
	}

	/// <summary>
	/// TCPService， UDPService的父类？
	/// TODO 这个是啥，有什么用
	/// </summary>
	public abstract class AService :NetBase
	{
		public abstract AChannel GetChannel(long id);

		public abstract Task<AChannel> AcceptChannel();

		public abstract AChannel ConnectChannel(IPEndPoint ipEndPoint);

		public abstract void Remove(long channelId);

		public abstract void Update();
	}
}