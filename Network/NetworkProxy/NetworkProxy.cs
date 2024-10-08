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
}