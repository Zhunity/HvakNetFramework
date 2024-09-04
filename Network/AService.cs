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
		UDP, // TODO
	}


	/// <summary>
	/// TCPService， UDPService的父类？
	/// </summary>
	public abstract class AService : NetBase
	{
	
		public abstract AChannel GetChannel(long id);

		public abstract void Remove(long channelId);

		public abstract void Update();
	}

	public interface IAcceptService
	{
		public void StartAsServer(IPEndPoint ipEndPoint);

		public Task<AChannel> AcceptChannel();
	}

	public interface IConnectService
	{
		public void StartAsClient();

		public AChannel ConnectChannel(IPEndPoint ipEndPoint);
	}
}