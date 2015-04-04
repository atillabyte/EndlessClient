﻿using System;
using EOLib;
using EOLib.Data;

namespace EndlessClient.Handlers
{
	public static class Item
	{
		public static bool GetItem(short uid)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (client == null || !client.ConnectedAndInitialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Item, PacketAction.Get);
			pkt.AddShort(uid);

			return client.SendPacket(pkt);
		}

		public static void DropItem(short id, int amount, byte x = 255, byte y = 255) //255 means use character's current location
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (client == null || !client.ConnectedAndInitialized)
				return;

			Packet pkt = new Packet(PacketFamily.Item, PacketAction.Drop);
			pkt.AddShort(id);
			pkt.AddInt(amount);
			if (x == 255 && y == 255)
			{
				pkt.AddByte(x);
				pkt.AddByte(y);
			}
			else
			{
				pkt.AddChar(x);
				pkt.AddChar(y);
			}

			client.SendPacket(pkt);
		}

		public static void JunkItem(short id, int amount)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (client == null || !client.ConnectedAndInitialized)
				return;
			
			Packet pkt = new Packet(PacketFamily.Item, PacketAction.Junk);
			pkt.AddShort(id);
			pkt.AddInt(amount);

			client.SendPacket(pkt);
		}

		/// <summary>
		/// Sent when an Item is dropped by the MainPlayer
		/// See ItemAddResponse for when another player drops an item
		/// </summary>
		public static void ItemDropResponse(Packet pkt)
		{
			short _id = pkt.GetShort();
			int _amount = pkt.GetThree();
			int characterAmount = pkt.GetInt(); //amount remaining for the character
			MapItem item = new MapItem
			{
				id = _id,
				amount = _amount,
				uid = pkt.GetShort(),
				x = pkt.GetChar(),
				y = pkt.GetChar(),
				//turn off drop protection since main player dropped it
				time = DateTime.Now.AddSeconds(-5),
				npcDrop = false,
				playerID = World.Instance.MainPlayer.ActiveCharacter.ID
			};
			byte characterWeight = pkt.GetChar(), characterMaxWeight = pkt.GetChar(); //character adjusted weights

			World.Instance.ActiveMapRenderer.AddMapItem(item);
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(_id, characterAmount, characterWeight, characterMaxWeight);

			ItemRecord rec = World.Instance.EIF.GetItemRecordByID(_id);
			EOGame.Instance.Hud.AddChat(ChatTabs.System, "", string.Format("{0} {1} {2}", World.GetString(DATCONST2.STATUS_LABEL_ITEM_DROP_YOU_DROPPED), _amount, rec.Name), ChatType.DownArrow);
			EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, DATCONST2.STATUS_LABEL_ITEM_DROP_YOU_DROPPED, string.Format(" {0} {1}", _amount, rec.Name));
		}

		/// <summary>
		/// Item is added to the map (sent when another player drops an item)
		/// </summary>
		public static void ItemAddResponse(Packet pkt)
		{
			World.Instance.ActiveMapRenderer.AddMapItem(new MapItem
			{
				id = pkt.GetShort(),
				uid = pkt.GetShort(),
				amount = pkt.GetThree(),
				x = pkt.GetChar(),
				y = pkt.GetChar(),
				time = DateTime.Now,
				npcDrop = false,
				playerID = -1 //unknown ID! so it will say Item is protected w/o "by player"
			});
		}

		public static void ItemRemoveResponse(Packet pkt)
		{
			short itemUid = pkt.GetShort();
			World.Instance.ActiveMapRenderer.RemoveMapItem(itemUid);
		}

		public static void ItemJunkResponse(Packet pkt)
		{
			short id = pkt.GetShort();
			int amountRemoved = pkt.GetThree();//don't really care - just math it
			int amountRemaining = pkt.GetInt();
			byte weight = pkt.GetChar();
			byte maxWeight = pkt.GetChar();

			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amountRemaining, weight, maxWeight);

			ItemRecord rec = World.Instance.EIF.GetItemRecordByID(id);
			EOGame.Instance.Hud.AddChat(ChatTabs.System, "", string.Format("{0} {1} {2}", World.GetString(DATCONST2.STATUS_LABEL_ITEM_JUNK_YOU_JUNKED), amountRemoved, rec.Name), ChatType.DownArrow);
			EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, DATCONST2.STATUS_LABEL_ITEM_JUNK_YOU_JUNKED, string.Format(" {0} {1}", amountRemoved, rec.Name));
		}

		/// <summary>
		/// Main player is picking an item up off the map
		/// </summary>
		public static void ItemGetResponse(Packet pkt)
		{
			short uid = pkt.GetShort();
			short id = pkt.GetShort();
			int amountTaken = pkt.GetThree();
			byte weight = pkt.GetChar();
			byte maxWeight = pkt.GetChar();

			if (uid != 0)
			{
				World.Instance.ActiveMapRenderer.UpdateMapItemAmount(uid, amountTaken);
			}

			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amountTaken, weight, maxWeight, true);

			ItemRecord rec = World.Instance.EIF.GetItemRecordByID(id);
			EOGame.Instance.Hud.AddChat(ChatTabs.System, "", string.Format("{0} {1} {2}", World.GetString(DATCONST2.STATUS_LABEL_ITEM_PICKUP_YOU_PICKED_UP), amountTaken, rec.Name), ChatType.UpArrow);
			EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, DATCONST2.STATUS_LABEL_ITEM_PICKUP_YOU_PICKED_UP, string.Format(" {0} {1}", amountTaken, rec.Name));
		}

		public static bool ItemUse(short itemID)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Item, PacketAction.Use);
			pkt.AddShort(itemID);

			return client.SendPacket(pkt);
		}
		/// <summary>
		/// Handles ITEM_REPLY (ITEM_USE response)
		/// </summary>
		public static void ItemReply(Packet pkt)
		{
			ItemType type = (ItemType) pkt.GetChar();
			short itemID = pkt.GetShort();
			int itemCountRemaining = pkt.GetInt();
			byte weight = pkt.GetChar();
			byte maxWeight = pkt.GetChar();
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(itemID, itemCountRemaining, weight, maxWeight);

			//format differs based on item type
			//(keeping this in order with how eoserv ITEM_USE handler is ordered
			switch (type)
			{
				case ItemType.Teleport: /*Warp packet handles the rest!*/ break;
				case ItemType.Heal:
				{
					int hpGain = pkt.GetInt();
					short hp = pkt.GetShort();
					short tp = pkt.GetShort();

					World.Instance.MainPlayer.ActiveCharacter.Stats.SetHP(hp);
					World.Instance.MainPlayer.ActiveCharacter.Stats.SetTP(tp);

					int percent = (int)Math.Round(100.0 * ((double)hp / World.Instance.MainPlayer.ActiveCharacter.Stats.maxhp));

					if(hpGain > 0)
						World.Instance.ActiveCharacterRenderer.SetDamageCounterValue(hpGain, percent, true);
					EOGame.Instance.Hud.RefreshStats();
				}
					break;
				case ItemType.HairDye:
				{
					byte hairColor = pkt.GetChar();
					World.Instance.MainPlayer.ActiveCharacter.RenderData.SetHairColor(hairColor);
				}
					break;
				case ItemType.Beer:
					World.Instance.ActiveCharacterRenderer.MakeDrunk();
					EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.STATUS_LABEL_ITEM_USE_DRUNK);
					break;
				case ItemType.EffectPotion:
				{
					short effect = pkt.GetShort();
					//World.Instance.ActiveCharacterRenderer.ShowEffect(effect);
					//todo: it appears effects are hard-coded into the original client. this is gross. 
					//	I'm thinking about re-compiling the resources from gfx024 into a new file with better numbers.
				}
					break;
			}
		}
	}
}
