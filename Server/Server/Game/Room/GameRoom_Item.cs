using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Server.DB;

namespace Server.Game
{
	public partial class GameRoom : JobSerializer
	{
        public void HandleEquipItem(Player player, C_EquipItem equipPacket)
        {
            if (player == null)
                return;

            player.HandleEquipItem(equipPacket);
            
        }

        public void HandleUseItem(Player player, C_UseItem usePacket)
        {
            if (player == null)
                return;

            player.HandleUseItem(usePacket);

        }

        public void HandleDeleteItem(Player player, C_DeleteItem deleteItemPacket)
        {
            if (player == null)
                return;

            player.HandleDeleteItem(deleteItemPacket);

        }

        public void HandleNewHp(Player player, C_ChangeHp newHpPacket)
        {
            if (player == null)
                return;

            player.HandleNewHp(newHpPacket);

        }

        public void HandleBuyStore(Player player, C_BuyStore buyPacket)
        {
            if (player == null)
                return;

            player.HandleBuyStore(buyPacket);

        }

        public void HandleSellStore(Player player, C_SellStore sellPacket)
        {
            if (player == null)
                return;

            player.HandleSellStore(sellPacket);

        }

    }
}
