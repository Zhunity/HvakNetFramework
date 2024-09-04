using NetMsg.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Lockstep.Network
{

	public abstract class NetworkProxy : NetBase
	{
		protected AService Service;

		protected readonly Dictionary<long, Session> sessions = new Dictionary<long, Session>();

		/// <summary>
		/// 解析具体是哪条协议的
		/// </summary>
		public IMessagePacker MessagePacker { get; set; }

		/// <summary>
		/// 在客户端是Client，在服务器是Server
		/// TODO 感觉这么传给NetProxy真的好吗？
		/// </summary>
		public IMessageDispatcher MessageDispatcher { get; set; }

		public virtual void Remove(long id)
		{
			Session session;
			if (!this.sessions.TryGetValue(id, out session))
			{
				return;
			}

			this.sessions.Remove(id);
			session.Dispose();
		}

		public Session Get(long id)
		{
			Session session;
			this.sessions.TryGetValue(id, out session);
			return session;
		}

		public static Session CreateSession(NetworkProxy net, AChannel c)
		{
			Session session = new Session { Id = IdGenerater.GenerateId() };
			session.Awake(net, c);
			return session;
		}

		public void Update()
		{
			if (this.Service == null)
			{
				return;
			}

			this.Service.Update();
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			foreach (Session session in this.sessions.Values.ToArray())
			{
				session.Dispose();
			}

			this.Service.Dispose();
		}
	}


	public class NetInnerProxy : NetworkProxy
	{
		private IAcceptService _acceptService => this.Service as IAcceptService;

		/// <summary>
		/// 当作服务端启动？
		/// </summary>
		/// <param name="protocol"></param>
		/// <param name="ipEndPoint"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="Exception"></exception>
		public void StartAsServer(NetworkProtocol protocol, IPEndPoint ipEndPoint)
		{
			try
			{
				switch (protocol)
				{
					case NetworkProtocol.TCP:
						this.Service = new TcpAcceptService();
						this._acceptService.StartAsServer(ipEndPoint);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				this.StartAccept();
			}
			catch (Exception e)
			{
				throw new Exception($"{ipEndPoint}", e);
			}
		}

		private async void StartAccept()
		{
			while (true)
			{
				if (this.IsDisposed)
				{
					return;
				}

				await this.Accept();
			}
		}

		public virtual async Task<Session> Accept()
		{
			AChannel channel = await (this.Service as IAcceptService).AcceptChannel();
			Session session = CreateSession(this, channel);
			channel.ErrorCallback += (c, e) => { this.Remove(session.Id); };
			this.sessions.Add(session.Id, session);
			session.Start();
			return session;
		}
	}

	public class NetOuterProxy : NetworkProxy 
	{
		private IConnectService ConnectService => this.Service as IConnectService;


		/// <summary>
		/// 当作客户端启动？
		/// </summary>
		/// <param name="protocol"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void StartAsClient(NetworkProtocol protocol)
		{
			switch (protocol)
			{
				case NetworkProtocol.TCP:
					this.Service = new TcpConnectService();
					this.ConnectService.StartAsClient();

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// 创建一个新Session
		/// </summary>
		public virtual Session Create(IPEndPoint ipEndPoint)
		{
			try
			{
				AChannel channel = this.ConnectService.ConnectChannel(ipEndPoint);
				Session session = CreateSession(this, channel);
				channel.ErrorCallback += (c, e) => { this.Remove(session.Id); };
				this.sessions.Add(session.Id, session);
				return session;
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
				return null;
			}
		}
	}
}