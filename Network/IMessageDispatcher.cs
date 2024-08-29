using NetMsg.Common;

namespace Lockstep.Network {
    public interface IMessageDispatcher {
        void Dispatch(Session session, ushort opcode, BaseMsg msg);
    }
}