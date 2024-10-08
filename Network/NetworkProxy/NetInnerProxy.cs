using NetMsg.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Lockstep.Network
{
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
}