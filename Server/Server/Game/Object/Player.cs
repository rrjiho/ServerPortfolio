using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Server.Game
{
	public class Player : GameObject
	{
		public int PlayerDbId { get; set; }
        public string PlayerName { get; set; }
		public ClientSession Session { get; set; }
		public Inventory Inven { get; private set; } = new Inventory();
        public StatInfo statInfo { get; set; }

		public int WeaponDamage { get; private set; }
		public int ArmorDefence { get; private set; }

        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
        public override int TotalDefence { get { return ArmorDefence; } }

        public bool IsQuestNumOne { get; private set; } = false;
        public int KillQuestNumber { get; set; }
        private int killCount = 0;

        public Player()
		{
			ObjectType = GameObjectType.Player;
		}

		public override void OnDamaged(GameObject attacker, int damage)
		{
			base.OnDamaged(attacker, damage);
		}

		public override void OnDead(GameObject attacker)
		{
			base.OnDead(attacker);
		}

		public void OnLeaveGame()
		{
            DbTransaction.SavePlayerStatus(this, Room);
        }

        public void HandleEquipItem(C_EquipItem equipPacket)
        {
            Item item = Inven.Get(equipPacket.ItemDbId);
            if (item == null)
                return;

            if (item.ItemType == ItemType.Consumable)
            {
                return;
            }                

            if (equipPacket.Equipped)
            {
                Item unequipItem = null;

                if (item.ItemType == ItemType.Weapon)
                {
                    unequipItem = Inven.Find(i => i.Equipped && i.ItemType == ItemType.Weapon);
                }
                else if (item.ItemType == ItemType.Armor)
                {
                    ArmorType armorType = ((Armor)item).ArmorType;
                    unequipItem = Inven.Find(i => i.Equipped && i.ItemType == ItemType.Armor && ((Armor)i).ArmorType == armorType);
                }

                if (unequipItem != null)
                {
                    unequipItem.Equipped = false;

                    DbTransaction.EquipItemNoti(this, unequipItem);

                    S_EquipItem equipOkItem = new S_EquipItem();
                    equipOkItem.ItemDbId = unequipItem.ItemDbId;
                    equipOkItem.Equipped = unequipItem.Equipped;
                    Session.Send(equipOkItem);
                }
            }

            {
                item.Equipped = equipPacket.Equipped;

                DbTransaction.EquipItemNoti(this, item);

                S_EquipItem equipOkItem = new S_EquipItem();
                equipOkItem.ItemDbId = item.ItemDbId;
                equipOkItem.Equipped = equipPacket.Equipped;
                Session.Send(equipOkItem);
            }

            RefreshAdditionalStat();
        }

        public void HandleUseItem(C_UseItem usePacket)
        {
            Item item = Inven.Get(usePacket.ItemDbId);
            if (item == null)
                return;

            if (item.ItemType == ItemType.Consumable)
            {
                Inven.Remove(item);

                Consumable potion = (Consumable)item;

                DbTransaction.UseItemNoti(this, potion);

                S_UseItem useItem = new S_UseItem();
                useItem.ItemDbId = potion.ItemDbId;
                Session.Send(useItem);
            }
            else
            {
                return;
            }

        }

        public void HandleDeleteItem(C_DeleteItem deleteItemPacket)
        {
            Item item = Inven.Get(deleteItemPacket.ItemDbId);
            if (item == null)
                return;

            Inven.Remove(item);

            DbTransaction.DeleteItemNoti(this, item, success =>
            {
                if (success)
                {
                    S_DeleteItem deleteItem = new S_DeleteItem();
                    deleteItem.ItemDbId = item.ItemDbId;
                    Session.Send(deleteItem);
                }
                else
                {
                    Console.WriteLine("Failed to save database");
                }
            });

            
        }

        public void HandleNewHp(C_ChangeHp newHpPacket)
        {
            Player player = this;
            if (player == null)
                return;

            Hp = newHpPacket.Hp;

            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = player.Id;
            changePacket.Hp = Hp;
            Room.Broadcast(changePacket);
        }

        public void HandleChat(C_Chat chatPacket)
        {
            Player player = this;
            if (player == null)
                return;

            S_Chat chatMessage = new S_Chat();
            chatMessage.ObjectId = player.Info.ObjectId;
            chatMessage.PlayerName = player.Session.PlayerName;
            chatMessage.Message = chatPacket.Message;

            Room.Broadcast(chatMessage);
        }

        public void HandleQuest(C_Quest questPacket)
        {
            QuestDb questDb = new QuestDb
            {
                PlayerDbId = PlayerDbId
            };

            if (questPacket.QuestNumber == 1)
            {
                if (questPacket.Accepted)
                {
                    questDb.IsAccepted = questPacket.Accepted;
                    questDb.QuestNumber = questPacket.QuestNumber;
                    questDb.KillCount = 0;

                    SetQuestNumOne(true);
                    KillQuestNumber = questPacket.QuestNumber;
                    
                    DbTransaction.QuestNoti(this, questDb);           
                }

                if (questPacket.Completed)
                {
                    questDb.IsAccepted = questPacket.Accepted;
                    questDb.QuestNumber = questPacket.QuestNumber;
                    bool completionSuccessful = DbTransaction.QuestCompleteNoti(this, questDb);
                    if (completionSuccessful)
                    {
                        S_Quest questCompletePacket = new S_Quest();
                        questCompletePacket.Completed = questPacket.Completed;
                        Session.Send(questCompletePacket);

                        int rewardTemplateId = 200;
                        int rewardCount = 1; 
                        DbTransaction.RewardQuest(this, rewardTemplateId, rewardCount);
                    }
                }
            }
            else
            {
                return;
            }
        }

        public void HandleBuyStore(C_BuyStore buyPacket)
        {
            DbTransaction.BuyItemNoti(this, buyPacket);
        }

        public void HandleSellStore(C_SellStore sellPacket)
        {
            Item item = Inven.Get(sellPacket.ItemDbId);
            if (item == null)
                return;

            Inven.Remove(item);

            DbTransaction.SellItemNoti(this, item, success =>
            {
                if (success)
                {
                    S_SellStore storepacket = new S_SellStore();
                    storepacket.ItemDbId = item.ItemDbId;
                    storepacket.Gold = Stat.Gold;
                    Session.Send(storepacket);
                }
                else
                {
                    Console.WriteLine("Failed to save database");
                }
            });

        }

        public void HandlePost(C_Post postPacket)
        {
            DbTransaction.PostNoti(this, postPacket);
        }

        public void HandleOpenPost(C_OpenPost openpostPacket)
        {
            DbTransaction.OpenPostNoti(this, openpostPacket);
        }

        public void SetQuestNumOne(bool value)
        {
            IsQuestNumOne = value;
        }

        public void HandleQuestKillCount()
        {
            if (IsQuestNumOne)
            {
                killCount++;
                DbTransaction.QuestKillCount(PlayerDbId, KillQuestNumber, killCount);

                if (killCount >= 5)
                {
                    SetQuestNumOne(false);
                }
            }
        }

        public void RefreshAdditionalStat()
        {
            WeaponDamage = 0;
            ArmorDefence = 0;

            foreach(Item item in Inven.Items.Values)
            {
                if (item.Equipped == false)
                    continue;

                switch(item.ItemType)
                {
                    case ItemType.Weapon:
                        WeaponDamage += ((Weapon)item).Damage;
                        break;
                    case ItemType.Armor:
                        ArmorDefence += ((Armor)item).Defence;
                        break;
                }
            }
        }
    }
}
