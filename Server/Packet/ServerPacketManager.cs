using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
		
	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }

	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.CMove, MakePacket<C_Move>);
		_handler.Add((ushort)MsgId.CMove, PacketHandler.C_MoveHandler);		
		_onRecv.Add((ushort)MsgId.CSkill, MakePacket<C_Skill>);
		_handler.Add((ushort)MsgId.CSkill, PacketHandler.C_SkillHandler);		
		_onRecv.Add((ushort)MsgId.CLogin, MakePacket<C_Login>);
		_handler.Add((ushort)MsgId.CLogin, PacketHandler.C_LoginHandler);		
		_onRecv.Add((ushort)MsgId.CEnterGame, MakePacket<C_EnterGame>);
		_handler.Add((ushort)MsgId.CEnterGame, PacketHandler.C_EnterGameHandler);		
		_onRecv.Add((ushort)MsgId.CCreatePlayer, MakePacket<C_CreatePlayer>);
		_handler.Add((ushort)MsgId.CCreatePlayer, PacketHandler.C_CreatePlayerHandler);		
		_onRecv.Add((ushort)MsgId.CEquipItem, MakePacket<C_EquipItem>);
		_handler.Add((ushort)MsgId.CEquipItem, PacketHandler.C_EquipItemHandler);		
		_onRecv.Add((ushort)MsgId.CUseItem, MakePacket<C_UseItem>);
		_handler.Add((ushort)MsgId.CUseItem, PacketHandler.C_UseItemHandler);		
		_onRecv.Add((ushort)MsgId.CDeleteItem, MakePacket<C_DeleteItem>);
		_handler.Add((ushort)MsgId.CDeleteItem, PacketHandler.C_DeleteItemHandler);		
		_onRecv.Add((ushort)MsgId.CChangeHp, MakePacket<C_ChangeHp>);
		_handler.Add((ushort)MsgId.CChangeHp, PacketHandler.C_ChangeHpHandler);		
		_onRecv.Add((ushort)MsgId.CChat, MakePacket<C_Chat>);
		_handler.Add((ushort)MsgId.CChat, PacketHandler.C_ChatHandler);		
		_onRecv.Add((ushort)MsgId.CQuest, MakePacket<C_Quest>);
		_handler.Add((ushort)MsgId.CQuest, PacketHandler.C_QuestHandler);		
		_onRecv.Add((ushort)MsgId.CBuyStore, MakePacket<C_BuyStore>);
		_handler.Add((ushort)MsgId.CBuyStore, PacketHandler.C_BuyStoreHandler);		
		_onRecv.Add((ushort)MsgId.CSellStore, MakePacket<C_SellStore>);
		_handler.Add((ushort)MsgId.CSellStore, PacketHandler.C_SellStoreHandler);		
		_onRecv.Add((ushort)MsgId.CPost, MakePacket<C_Post>);
		_handler.Add((ushort)MsgId.CPost, PacketHandler.C_PostHandler);		
		_onRecv.Add((ushort)MsgId.COpenPost, MakePacket<C_OpenPost>);
		_handler.Add((ushort)MsgId.COpenPost, PacketHandler.C_OpenPostHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
		{
			CustomHandler.Invoke(session, pkt, id);
		}
		else
		{
			Action<PacketSession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}