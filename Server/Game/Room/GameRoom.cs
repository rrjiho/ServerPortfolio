﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public partial class GameRoom : JobSerializer
	{
		public int RoomId { get; set; }

		Dictionary<int, Player> _players = new Dictionary<int, Player>();
		Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
		Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

		public Map Map { get; private set; } = new Map();

		public void Init(int mapId)
		{
			Map.LoadMap(mapId);
            Random rand = new Random();

            for (int i = 0; i < 6; i++)
			{
                Monster monster = ObjectManager.Instance.Add<Monster>();
                monster.Init(1);
                monster.CellPos = new Vector2Int(rand.Next(8, 22), rand.Next(-10, 1));
                EnterGame(monster);
            }
			
		}

		public void Update()
		{			

			Flush();
		}

		public void EnterGame(GameObject gameObject)
		{
			if (gameObject == null)
				return;

			GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

			if (type == GameObjectType.Player)
			{
				Player player = gameObject as Player;
				_players.Add(gameObject.Id, player);
				player.Room = this;

				player.RefreshAdditionalStat();

				Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));

				{
					S_EnterGame enterPacket = new S_EnterGame();
					enterPacket.Player = player.Info;
					player.Session.Send(enterPacket);

					S_Spawn spawnPacket = new S_Spawn();
					foreach (Player p in _players.Values)
					{
						if (player != p)
							spawnPacket.Objects.Add(p.Info);
					}

					foreach (Monster m in _monsters.Values)
						spawnPacket.Objects.Add(m.Info);

					foreach (Projectile p in _projectiles.Values)
						spawnPacket.Objects.Add(p.Info);

					player.Session.Send(spawnPacket);
				}
			}
			else if (type == GameObjectType.Monster)
			{
				Monster monster = gameObject as Monster;
				_monsters.Add(gameObject.Id, monster);
				monster.Room = this;

				Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));

				monster.Update();
			}
			else if (type == GameObjectType.Projectile)
			{
				Projectile projectile = gameObject as Projectile;
				_projectiles.Add(gameObject.Id, projectile);
				projectile.Room = this;

				projectile.Update();
			}
			
			{
				S_Spawn spawnPacket = new S_Spawn();
				spawnPacket.Objects.Add(gameObject.Info);
				foreach (Player p in _players.Values)
				{
					if (p.Id != gameObject.Id)
						p.Session.Send(spawnPacket);
				}
			}
		}

		public void LeaveGame(int objectId)
		{
			GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

			if (type == GameObjectType.Player)
			{
				Player player = null;
				if (_players.Remove(objectId, out player) == false)
					return;

				player.OnLeaveGame();
				Map.ApplyLeave(player);
				player.Room = null;

				{
					S_LeaveGame leavePacket = new S_LeaveGame();
					player.Session.Send(leavePacket);
				}
			}
			else if (type == GameObjectType.Monster)
			{
				Monster monster = null;
				if (_monsters.Remove(objectId, out monster) == false)
					return;

				Map.ApplyLeave(monster);
				monster.Room = null;
			}
			else if (type == GameObjectType.Projectile)
			{
				Projectile projectile = null;
				if (_projectiles.Remove(objectId, out projectile) == false)
					return;

				projectile.Room = null;
			}

			{
				S_Despawn despawnPacket = new S_Despawn();
				despawnPacket.ObjectIds.Add(objectId);
				foreach (Player p in _players.Values)
				{
					if (p.Id != objectId)
						p.Session.Send(despawnPacket);
				}
			}
		}

        public void HandleChat(Player player, C_Chat ChatPacket)
        {
            if (player == null)
                return;

            player.HandleChat(ChatPacket);

        }

        public void HandlePost(Player player, C_Post postPacket)
        {
            if (player == null)
                return;

            player.HandlePost(postPacket);

        }
        public void HandleOpenPost(Player player, C_OpenPost openpostPacket)
        {
            if (player == null)
                return;

            player.HandleOpenPost(openpostPacket);

        }

        public Player FindPlayer(Func<GameObject, bool> condition)
		{
			foreach (Player player in _players.Values)
			{
				if (condition.Invoke(player))
					return player;
			}

			return null;
		}

		public void Broadcast(IMessage packet)
		{
			foreach (Player p in _players.Values)
			{
				p.Session.Send(packet);
			}
		}
	}
}
