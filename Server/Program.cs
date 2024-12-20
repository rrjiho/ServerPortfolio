﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using SharedDB;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();
        static void GameLogicTask()
        {
            while (true)
            {
                GameLogic.Instance.Update();
                Thread.Sleep(0);
            }
        }

        static void DbTask()
        {
            while (true)
            {
                DbTransaction.Instance.Flush();
                Thread.Sleep(0);
            }
        }

        static void NetWorkTask()
        {
            while (true)
            {
                List<ClientSession> sessions = SessionManager.Instance.GetSessions();
                foreach(ClientSession session in sessions)
                {
                    session.FlushSend();
                }

                Thread.Sleep(0);
            }
        }
        static void StartServerInfoTask()
        {
            var t = new System.Timers.Timer();
            t.AutoReset = true;
            t.Elapsed += new System.Timers.ElapsedEventHandler((s, e) =>
            {
                using (SharedDbContext shared = new SharedDbContext())
                {
                    ServerDb serverDb = shared.Servers.Where(s => s.Name == Name).FirstOrDefault();
                    if (serverDb != null)
                    {
                        serverDb.IpAddress = IpAddress;
                        serverDb.Port = Port;
                        shared.SaveChangesEx();
                    }
                    else
                    {
                        serverDb = new ServerDb()
                        {
                            Name = Program.Name,
                            IpAddress = Program.IpAddress,
                            Port = Program.Port
                        };
                        shared.Servers.Add(serverDb);
                        shared.SaveChangesEx();
                    }
                }
            });
            t.Interval = 10 * 1000;
            t.Start();
        }

        public static string Name { get; } = "Seoul";
		public static int Port { get; } = 7777;
		public static string IpAddress { get; set; }

		static void Main(string[] args)
		{
            ConfigManager.LoadConfig();
			DataManager.LoadData();

            GameLogic.Instance.Push(() => { GameLogic.Instance.Add(1); });

			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            IpAddress = ipAddr.ToString();


            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate("DefalutPlayerName"); });
			Console.WriteLine("Listening...");

            StartServerInfoTask();

            {
                Thread t = new Thread(DbTask);
                t.Name = "DB";
                t.Start();
            }

            {
                Thread t = new Thread(NetWorkTask);
                t.Name = "Network Send";
                t.Start();
            }

            Thread.CurrentThread.Name = "GameLogic";
            GameLogicTask();
		}    
    }
}
