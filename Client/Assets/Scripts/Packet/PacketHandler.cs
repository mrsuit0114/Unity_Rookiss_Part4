using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


class PacketHandler
{
    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastEnterGame p = packet as S_BroadcastEnterGame;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.EnterGame(p);
    }
    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastLeaveGame p = packet as S_BroadcastLeaveGame;
        ServerSession serverSession = session as ServerSession;
        PlayerManager.Instance.LeaveGame(p);
    }
    public static void S_PlayerListHandler(PacketSession session, IPacket packet)
    {
        S_PlayerList p = packet as S_PlayerList;
        ServerSession serverSession = session as ServerSession;

        PlayerManager.Instance.Add(p);

    }
    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastMove p = packet as S_BroadcastMove;
        ServerSession serverSession = session as ServerSession;
        PlayerManager.Instance.Move(p);
    }
}

