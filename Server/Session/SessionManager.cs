using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
	class SessionManager
	{
		static SessionManager _session = new SessionManager();
		public static SessionManager Instance { get { return _session; } }

		int _sessionId = 0;
		Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
		object _lock = new object();

		public List<ClientSession> GetSessions()
		{
			List<ClientSession> sessions = new List<ClientSession>();
			lock (_lock)
			{
				sessions = _sessions.Values.ToList();
			}

			return sessions;
		}

		public ClientSession Generate(string playerName)
		{
			lock (_lock)
			{
				int sessionId = ++_sessionId;

				ClientSession session = new ClientSession();
				session.SessionId = sessionId;
                session.PlayerName = playerName;
                _sessions.Add(sessionId, session);

                Console.WriteLine($"Connected : {sessionId} - Player: {playerName}");

                return session;
			}
		}

		public ClientSession Find(int id)
		{
			lock (_lock)
			{
				ClientSession session = null;
				_sessions.TryGetValue(id, out session);
				return session;
			}
		}

        public ClientSession FindByPlayerName(string playerName)
        {
            lock (_lock)
            {
                return _sessions.Values.FirstOrDefault(session => session.PlayerName == playerName);
            }
        }

        public void Remove(ClientSession session)
		{
			lock (_lock)
			{
				_sessions.Remove(session.SessionId);
			}
		}
	}
}
