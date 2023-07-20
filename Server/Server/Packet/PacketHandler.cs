using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class PacketHandler
    {
        // 어떤 세션에서 불렸고 어떤 패킷인지를 인자로 받는다
        public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
        {
            PlayerInfoReq p = packet as PlayerInfoReq;

            Console.WriteLine($"PlayerInfoReq : {p.playerId} {p.name}");

            foreach (PlayerInfoReq.Skill skill in p.skills)
            {
                Console.WriteLine($"Skill {skill.id}, {skill.level}, {skill.duration}");
            }
        }
    }
}
