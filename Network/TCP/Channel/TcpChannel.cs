using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Lockstep.Network
{
	public class TcpChannel : AChannel {
		protected TcpClient tcpClient;

		protected readonly CircularBuffer recvBuffer = new CircularBuffer();
		protected readonly CircularBuffer sendBuffer = new CircularBuffer();

		private bool isSending;
		protected PacketParser parser;
		private bool isConnected;
		private TaskCompletionSource<Packet> recvTcs;

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();

			this.tcpClient.Close();
		}

		protected void OnAccepted()
		{
			this.isConnected = true;
			this.StartSend();
			this.StartRecv();
		}

		public override void Send(byte[] buffer, int index, int length)
		{
			if (this.IsDisposed)
			{
				throw new Exception("TChannel已经被Dispose, 不能发送消息");
			}
			byte[] size = BytesHelper.GetBytes((ushort)buffer.Length);
			this.sendBuffer.Write(size, 0, size.Length);
			this.sendBuffer.Write(buffer, index, length);
			if (this.isConnected)
			{
				this.StartSend();
			}
		}

		public override void Send(List<byte[]> buffers)
		{
			if (this.IsDisposed)
			{
				throw new Exception("TChannel已经被Dispose, 不能发送消息");
			}
			ushort size = (ushort)buffers.Select(b => b.Length).Sum();
			byte[] sizeBuffer = BytesHelper.GetBytes(size);
			this.sendBuffer.Write(sizeBuffer, 0, sizeBuffer.Length);
			foreach (byte[] buffer in buffers)
			{
				this.sendBuffer.Write(buffer, 0, buffer.Length);
			}
			if (this.isConnected)
			{
				this.StartSend();
			}
		}

		private async void StartSend()
		{
			try
			{
				if (this.IsDisposed)
				{
					return;
				}

				// 如果正在发送中,不需要再次发送
				if (this.isSending)
				{
					return;
				}

				while (true)
				{
					if (this.IsDisposed)
					{
						return;
					}

					// 没有数据需要发送
					long buffLength = this.sendBuffer.Length;
					if (buffLength == 0)
					{
						this.isSending = false;
						return;
					}

					this.isSending = true;
					
					NetworkStream stream = this.tcpClient.GetStream();
					if (!stream.CanWrite)
					{
						return;
					}

					await this.sendBuffer.ReadAsync(stream);
				}
			}
			catch (IOException)
			{
				this.OnError(SocketError.SocketError);
			}
			catch (ObjectDisposedException)
			{
				this.OnError(SocketError.SocketError);
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
				this.OnError(SocketError.SocketError);
			}
		}

		private async void StartRecv()
		{
			try
			{
				while (true)
				{
					if (this.IsDisposed)
					{
						return;
					}

					NetworkStream stream = this.tcpClient.GetStream();
					if (!stream.CanRead)
					{
						return;
					}

					int n = await this.recvBuffer.WriteAsync(stream);

					if (n == 0)
					{
						this.OnError(SocketError.NetworkReset);
						return;
					}

					if (this.recvTcs != null)
					{
						bool isOK = this.parser.Parse();
						if (isOK)
						{
							Packet packet = this.parser.GetPacket();

							var tcs = this.recvTcs;
							this.recvTcs = null;
							tcs.SetResult(packet);
						}
					}
				}
			}
			catch (IOException)
			{
				this.OnError(SocketError.SocketError);
			}
			catch (ObjectDisposedException)
			{
				this.OnError(SocketError.SocketError);
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
				this.OnError(SocketError.SocketError);
			}
		}

		public override Task<Packet> Recv()
		{
			if (this.IsDisposed)
			{
				throw new Exception("TChannel已经被Dispose, 不能接收消息");
			}

			bool isOK = this.parser.Parse();
			if (isOK)
			{
				Packet packet = this.parser.GetPacket();
				return Task.FromResult(packet);
			}

			recvTcs = new TaskCompletionSource<Packet>();
			return recvTcs.Task;
		}
	}

	public class TcpAcceptChannel : TcpChannel
	{
		/// <summary>
		/// accept
		/// 服务器等待客户端连接成功
		/// </summary>
		public void Start(TcpClient tcpClient, TcpService service)
		{
			this.Start(service, ChannelType.Accept);
			this.tcpClient = tcpClient;
			this.parser = new PacketParser(this.recvBuffer);

			IPEndPoint ipEndPoint = (IPEndPoint)this.tcpClient.Client.RemoteEndPoint;
			this.RemoteAddress = ipEndPoint;
			this.OnAccepted();
		}
	}

	public class TcpConnectChannel : TcpChannel
	{
		/// <summary>
		/// connect
		/// 客户端连接服务器成功
		/// </summary>
		public void Start(TcpClient tcpClient, IPEndPoint ipEndPoint, TcpService service)
		{
			this.Start(service, ChannelType.Connect);
			this.tcpClient = tcpClient;
			this.parser = new PacketParser(this.recvBuffer);
			this.RemoteAddress = ipEndPoint;

			this.ConnectAsync(ipEndPoint);
		}



		private async void ConnectAsync(IPEndPoint ipEndPoint)
		{
			try
			{
				await this.tcpClient.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port);

				this.OnAccepted();
			}
			catch (SocketException e)
			{
				Log.Error($"connect error: {e.SocketErrorCode}");
				this.OnError(e.SocketErrorCode);
			}
			catch (Exception e)
			{
				this.OnError(SocketError.SocketError);
				Log.Error($"connect error: {ipEndPoint} {e}");
			}
		}
	}
}