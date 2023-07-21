using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Session;

namespace Server
{
    internal class GameRoom
    {
        // Dictionary로 개선할수도있겠다
        // 아마도 내부적으로 쓰레드safe하지 않기 때문에 Rc가 발생할 수 있는 코드는 락으로 감싸야한다
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();
        
        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = chat + $"{chat} I am {packet.playerId} ";
            ArraySegment<byte> segment = packet.Write();

            lock (_lock)
            {
                foreach (ClientSession s in _sessions)
                    s.Send(segment);
            }

        }

        public void Enter(ClientSession session)
        {
            lock(_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock(_lock)
            {
                _sessions.Remove(session);
            }
        }

    }
}
