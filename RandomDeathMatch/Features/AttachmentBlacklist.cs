using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Attachments.Components;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TheRiptide.Translation;

namespace TheRiptide
{
    //DOUBLE-SHOT SYSTEM
    public class AttachmentBlacklistConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("put black listed attachments here, see below for all attachments")]
        public List<AttachmentName> BlackList { get; set; } = new List<AttachmentName>();

        [Description("list of all the different attachments (changing this does nothing)")]
        public List<AttachmentName> AllAttachments { get; set; } = new List<AttachmentName>();
    }

    class AttachmentBlacklist
    {
        public static AttachmentBlacklist Singleton { get; private set; }

        AttachmentBlacklistConfig config;

        public AttachmentBlacklist()
        {
            Singleton = this;
        }

        public void Init(AttachmentBlacklistConfig config, Deathmatch plugin)
        {
            this.config = config;
            config.AllAttachments.Clear();
            foreach (AttachmentName name in Enum.GetValues(typeof(AttachmentName)))
                config.AllAttachments.Add(name);
            PluginHandler handler = PluginHandler.Get(plugin);
            handler.SaveConfig(plugin, "attachment_blacklist_config");
        }

        [PluginEvent(ServerEventType.PlayerChangeItem)]
        void OnPlayerChangesItem(Player player, ushort old_item, ushort new_item)
        {
            if(player.ReferenceHub.inventory.UserInventory.Items.ContainsKey(new_item))
            {
                ItemBase item = player.ReferenceHub.inventory.UserInventory.Items[new_item];
                if(item is Firearm firearm)
                {
                    List<Attachment> to_remove = new List<Attachment>();
                    foreach (var a in firearm.Attachments)
                    {
                        if (config.BlackList.Contains(a.Name))
                        {
                            BroadcastOverride.BroadcastLine(player, 1, 3.0f, BroadcastPriority.Medium, translation.AttachmentBanned.Replace("{attachment}", a.Name.ToString()));
                            to_remove.Add(a);
                        }
                    }

                    if (!to_remove.IsEmpty())
                    {
                        firearm.Attachments = firearm.Attachments.Except(to_remove).ToArray();
                        firearm.Status = new FirearmStatus(firearm.Status.Ammo, firearm.Status.Flags, firearm.GetCurrentAttachmentsCode());
                    }
                }
            }
        }

        [PluginEvent(ServerEventType.PlayerShotWeapon)]
        void OnShotWeapon(Player player, Firearm firearm)
        {
            List<Attachment> to_remove = new List<Attachment>();
            foreach (var a in firearm.Attachments)
            {
                if (config.BlackList.Contains(a.Name))
                {
                    BroadcastOverride.BroadcastLine(player, 1, 3.0f, BroadcastPriority.Medium, translation.AttachmentBanned.Replace("{attachment}", a.Name.ToString()));
                    to_remove.Add(a);
                }
            }

            if (!to_remove.IsEmpty())
            {
                firearm.Attachments = firearm.Attachments.Except(to_remove).ToArray();
                firearm.Status = new FirearmStatus(firearm.Status.Ammo, firearm.Status.Flags, firearm.GetCurrentAttachmentsCode());
            }
        }
    }
}
