using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    MyPlayer _myPlayer;
    Dictionary<int, Player> _players = new Dictionary<int, Player>();

    public static PlayerManager Instance { get; } = new PlayerManager();

    public void Add(S_PlayerList packet)
    {
        Object obj = Resources.Load("Player");

        foreach(S_PlayerList.Player p in packet.players)
        {
            GameObject go = Object.Instantiate(obj) as GameObject;

            if(p.isSelf)
            {
                MyPlayer myplayer = go.AddComponent<MyPlayer>();
                myplayer.PlayerId = p.playerId;
                myplayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                _myPlayer = myplayer;
            }
            else
            {
                Player player = go.AddComponent<Player>();
                player.PlayerId = p.playerId;
                player.transform.position = new Vector3(p.posX,p.posY, p.posZ);
                _players.Add(p.playerId, player);
            }
        }

    }

    internal void EnterGame(S_BroadcastEnterGame p)
    {
        if (p.playerId == _myPlayer.PlayerId)
            return;
        Object obj = Resources.Load("Player");
        GameObject go = Object.Instantiate(obj) as GameObject;

        Player player = go.AddComponent<Player>();
        player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
        _players.Add(p.playerId, player);
    }
    internal void Move(S_BroadcastMove p)
    {
        if(_myPlayer.PlayerId == p.playerId)
        {
            _myPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
        }
        else
        {
            Player player = null;
            if(_players.TryGetValue(p.playerId,out player))
            {
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
            }
        }
    }

    internal void LeaveGame(S_BroadcastLeaveGame p)
    {
        if(_myPlayer.PlayerId == p.playerId)
        {
            GameObject.Destroy(_myPlayer.gameObject);
            _myPlayer = null;
        }
        else
        {
            Player player = null;
            if (_players.TryGetValue(p.playerId, out player))
            {
                GameObject.Destroy(player.gameObject);
                _players.Remove(p.playerId);
            }
        }
    }

}
