using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static TheRiptide.Translation;

namespace TheRiptide
{
    //DOUBLE-SHOT SYSTEM
    public class AttachmentBlacklistConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("put black listed attachments here, see global reference config for attachment types")]
        public List<AttachmentName> BlackList { get; set; } = new List<AttachmentName>();
    }

    class AttachmentBlacklist
    {
        public static AttachmentBlacklist Singleton { get; private set; }
        private Dictionary<ItemType, uint> BannedWeaponCodes = new Dictionary<ItemType,  uint>();

        AttachmentBlacklistConfig config;

        public AttachmentBlacklist()
        {
            Singleton = this;
        }

        public void Init(AttachmentBlacklistConfig config)
        {
            this.config = config;
        }

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            //foreach(RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
            //{
            //    WorkstationController wc = room.GetComponentInChildren<WorkstationController>();
            //    if (wc != null)
            //    NetworkServer.UnSpawn(wc.gameObject);
            //}
        }

        [PluginEvent(ServerEventType.PlayerChangeItem)]
        void OnPlayerChangeItem(Player player, ushort old_item, ushort new_item)
        {
            if(player.ReferenceHub.inventory.UserInventory.Items.ContainsKey(new_item))
            {
                ItemBase item = player.ReferenceHub.inventory.UserInventory.Items[new_item];
                if(item is Firearm firearm)
                    RemoveBanned(player, firearm);
            }
        }

        [PluginEvent(ServerEventType.PlayerShotWeapon)]
        void OnShotWeapon(Player player, Firearm firearm)
        {
            RemoveBanned(player, firearm);
        }

        private void RemoveBanned(Player player, Firearm firearm)
        {
            if(!BannedWeaponCodes.ContainsKey(firearm.ItemTypeId))
            {
                int bit_pos = 0;
                uint code_mask = 0;
                foreach (var a in firearm.Attachments)
                {
                    if(config.BlackList.Contains(a.Name))
                        code_mask |= (1U << bit_pos);
                    bit_pos++;
                }
                BannedWeaponCodes.Add(firearm.ItemTypeId, ~code_mask);
            }

            uint old_code = firearm.GetCurrentAttachmentsCode();
            uint new_code = old_code & BannedWeaponCodes[firearm.ItemTypeId];
            if(new_code != old_code)
            {
                BitArray ba = new BitArray(BitConverter.GetBytes(~BannedWeaponCodes[firearm.ItemTypeId]));
                List<string> attachments = new List<string>();
                int index = 0;
                foreach(bool b in ba)
                {
                    if (b)
                        attachments.Add(firearm.Attachments[index].Name.ToString());
                    index++;
                    if (index >= firearm.Attachments.Length)
                        break;
                }
                BroadcastOverride.BroadcastLine(player, 1, 3.0f, BroadcastPriority.Medium, translation.AttachmentBanned.Replace("{attachment}", string.Join(", ", attachments)));
                BroadcastOverride.UpdateIfDirty(player);
                firearm.ApplyAttachmentsCode(new_code, true);
            }
        }
    }
}
