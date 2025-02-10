using Server.Game;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Data;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;

namespace Server.DB
{
    public partial class DbTransaction : JobSerializer
    {
        public static void EquipItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Equipped = item.Equipped
            };

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Equipped)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (!success)
                    {
                        Console.WriteLine("Failed to save database");
                    }
                }
            });
        }

        public static void UseItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            if (item.Count <= 0)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    var dbItem = db.Items.FirstOrDefault(i => i.ItemDbId == item.ItemDbId);
                    if (dbItem == null)
                        return;

                    db.Items.Remove(dbItem);

                    bool success = db.SaveChangesEx();
                    if (!success)
                    {
                        Console.WriteLine("Failed to save database");
                    }
                }
            });
        }

        // callback 이용해서 예외처리하는 방법
        public static void DeleteItemNoti(Player player, Item item, Action<bool> callback)
        {
            if (player == null || item == null)
            {
                callback?.Invoke(false);
                return;
            }


            if (item.Count <= 0)
            {
                callback?.Invoke(false);
                return;
            }

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    var dbItem = db.Items.FirstOrDefault(i => i.ItemDbId == item.ItemDbId);
                    if (dbItem == null)
                    {
                        callback?.Invoke(false);
                        return;
                    }

                    db.Items.Remove(dbItem);

                    bool success = db.SaveChangesEx();
                    callback?.Invoke(success);
                }
            });
        }

        public static void QuestNoti(Player player, QuestDb questDb)
        {
            if (player == null || questDb == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {                 
                    db.Quests.Add(questDb);

                    bool success = db.SaveChangesEx();
                    if (!success)
                    {
                        Console.WriteLine("Failed to save database");
                    } 
                }
            });
        }

        public static bool QuestCompleteNoti(Player player, QuestDb questDb)
        {
            if (player == null || questDb == null)
                return false;

            bool isCompleted = false;

            using (AppDbContext db = new AppDbContext())
            {
                QuestDb existingQuest = db.Quests
                    .FirstOrDefault(q => q.QuestNumber == questDb.QuestNumber && q.PlayerDbId == questDb.PlayerDbId);

                if (existingQuest != null && existingQuest.IsAccepted && existingQuest.KillCount == 5)
                {
                    existingQuest.IsAccepted = questDb.IsAccepted;
                    db.Entry(existingQuest).State = EntityState.Modified;

                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        isCompleted = true;
                    }
                    if (!success)
                    {
                        Console.WriteLine("Failed to save database.");
                    }
                }
                else
                {
                    isCompleted = false;
                }                   
            }
            return isCompleted;
        }

        public static void QuestKillCount(int playerDbId, int questNumber, int killCount)
        {
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    QuestDb existingQuest = db.Quests
                        .FirstOrDefault(q => q.PlayerDbId == playerDbId && q.QuestNumber == questNumber && q.IsAccepted);

                    if (existingQuest != null)
                    {
                        existingQuest.KillCount = killCount;
                        db.Entry(existingQuest).State = EntityState.Modified;

                        bool success = db.SaveChangesEx();
                        if (!success)
                        {
                            Console.WriteLine("Failed to save database.");
                        }
                    }
                }
            });
        }

        public static void RewardQuest(Player player, int templateId, int count)
        {
            if (player == null)
                return;

            int? slot = player.Inven.GetEmptySlot();
            if (slot == null)
            {
                return;
            }

            ItemDb itemDb = new ItemDb()
            {
                TemplateId = templateId,
                Count = count,
                Slot = slot.Value,
                OwnerDbId = player.PlayerDbId
            };

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Items.Add(itemDb);
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        Item newItem = Item.MakeItem(itemDb);
                        player.Inven.Add(newItem);

                        S_AddItem itemPacket = new S_AddItem();
                        ItemInfo itemInfo = new ItemInfo();
                        itemInfo.MergeFrom(newItem.Info);
                        itemPacket.Items.Add(itemInfo);

                        player.Session.Send(itemPacket);
                    }
                    else
                    {
                        Console.WriteLine("Failed to save database.");
                    }
                }
            });
        }

        public static void BuyItemNoti(Player player, C_BuyStore storePacket)
        {
            if (player == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    var dbPlayer = db.Players.FirstOrDefault(p => p.PlayerDbId == player.PlayerDbId);

                    if (dbPlayer.Gold < storePacket.Price)
                    {
                        return;
                    }

                    int? slot = player.Inven.GetEmptySlot();
                    if (slot == null)
                    {
                        Console.WriteLine("No empty slots.");
                        return;
                    }

                    ItemDb itemDb = new ItemDb()
                    {
                        TemplateId = storePacket.TemplateId,
                        Slot = slot.Value,
                        Count = 1,
                        OwnerDbId = player.PlayerDbId
                    };

                    dbPlayer.Gold -= storePacket.Price;

                    db.Items.Add(itemDb);
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        Item newItem = Item.MakeItem(itemDb);
                        player.Inven.Add(newItem);

                        S_AddItem itemPacket = new S_AddItem();
                        ItemInfo itemInfo = new ItemInfo();
                        itemInfo.MergeFrom(newItem.Info);
                        itemPacket.Items.Add(itemInfo);

                        player.Session.Send(itemPacket);

                        player.Stat.Gold = dbPlayer.Gold;
                        S_BuyStore storePacket = new S_BuyStore();
                        storePacket.Gold = dbPlayer.Gold;

                        player.Session.Send(storePacket);
                    }
                    else
                    {
                        Console.WriteLine("Failed to save database");
                    }
                }
            });
        }

        // callback 이용해서 예외처리하는 방법
        public static void SellItemNoti(Player player, Item item, Action<bool> callback)
        {
            if (player == null)
            {
                callback?.Invoke(false);
                return;
            }

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    var dbItem = db.Items.FirstOrDefault(i => i.ItemDbId == item.ItemDbId);
                    if (dbItem == null)
                    {
                        callback?.Invoke(false);
                        return;
                    }

                    db.Items.Remove(dbItem);

                    var dbPlayer = db.Players.FirstOrDefault(p => p.PlayerDbId == player.PlayerDbId);
                    dbPlayer.Gold += item.Price;

                    bool success = db.SaveChangesEx();
                    callback?.Invoke(success);

                    player.Stat.Gold = dbPlayer.Gold;
                }
            });
        }

        public static void PostNoti(Player player, C_Post postPacket)
        {
            if (player == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    var findPlayer = db.Players.FirstOrDefault(p => p.PlayerName == postPacket.ReceivePlayerName);

                    var myPlayer = db.Players.FirstOrDefault(p => p.PlayerDbId == player.PlayerDbId);

                    var postNumber = db.Posts.Where(p => p.PlayerName == postPacket.ReceivePlayerName)
                                             .OrderByDescending(p => p.PostNumber)
                                             .FirstOrDefault();

                    int newpostNumber = postNumber != null ? postNumber.PostNumber + 1 : 1;

                    if (findPlayer == null)
                    {
                        Console.WriteLine($"do not find {postPacket.ReceivePlayerName}");
                        return;
                    }
                    else
                    {
                        Timestamp receivedTimestamp = postPacket.Date;
                        DateTime dateTime = receivedTimestamp.ToDateTime().ToLocalTime();

                        PostDb postdb = new PostDb()
                        {
                            SendPlayerName = myPlayer.PlayerName,
                            PostNumber = newpostNumber,
                            PlayerDbId = findPlayer.PlayerDbId,
                            PlayerName = postPacket.ReceivePlayerName,
                            Message = postPacket.Message,
                            Date = dateTime
                        };

                        db.Posts.Add(postdb);
                        bool success = db.SaveChangesEx();
                        if (success == false)
                            return;

                        var receiverSession = SessionManager.Instance.FindByPlayerName(postPacket.ReceivePlayerName);

                        if (receiverSession != null)
                        {
                            S_Post newPostPacket = new S_Post()
                            {
                                SendPlayerName = myPlayer.PlayerName,
                                Date = receivedTimestamp,
                                PostNumber = newpostNumber,
                            };

                            receiverSession.Send(newPostPacket);
                        }
                    }
                }
            });
        }

        public static void OpenPostNoti(Player player, C_OpenPost openpostPacket)
        {
            if (player == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    var dbPost = db.Posts.FirstOrDefault(p => p.SendPlayerName == openpostPacket.SendPlayerName && p.PostNumber == openpostPacket.PostNumber);

                    if (dbPost == null)
                        return;

                    string message = dbPost.Message;

                    DateTime postDate = dbPost.Date;
                    Timestamp timestamp = Timestamp.FromDateTime(postDate.ToUniversalTime());

                    S_OpenPost newPost = new S_OpenPost();
                    newPost.SendPlayerName = dbPost.SendPlayerName;
                    newPost.Message = message;
                    newPost.Date = timestamp;

                    player.Session.Send(newPost);
                }
            });
        }
    }
}
