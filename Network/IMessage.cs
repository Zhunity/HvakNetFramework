namespace Lockstep.Network {
	/// <summary>
	/// TODO IBaseMsg和IMessage有什么区别
	/// </summary>
	public interface IMessage {
         ushort opcode { get; set; }
    }

    /// <summary>
    /// 没用？ 
    /// </summary>
    public interface IRequest : IMessage {
        int RpcId { get; set; }
    }

    public interface IResponse : IMessage {
        int Error { get; set; }
        string Message { get; set; }
        int RpcId { get; set; }
    }

    public class ResponseMessage : IResponse {
        public  ushort opcode { get; set; }
        public int Error { get; set; }
        public string Message { get; set; }
        public int RpcId { get; set; }
    }
}