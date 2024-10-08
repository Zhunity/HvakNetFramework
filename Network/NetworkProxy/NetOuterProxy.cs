using NetMsg.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Lockstep.Network
{
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