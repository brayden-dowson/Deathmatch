using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Radio;
using MapGeneration;
using Interactables.Interobjects;
using Mirror;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using static TheRiptide.Utility;
using static TheRiptide.Translation;

namespace TheRiptide
{
    public class Loadouts
    {
        public enum GunSlot { Primary, Secondary, Tertiary };

        public class Loadout
        {
            public ItemType primary = ItemType.None;
            public ItemType secondary = ItemType.None;
            public ItemType tertiary = ItemType.None;
            public GunSlot slot = GunSlot.Primary;

            public bool locked = false;
            public bool customising = false;
            public bool rage_mode_enabled = false;
            public bool radio = true;
        }

        public static Dictionary<int, Loadout> player_loadouts = new Dictionary<int, Loadout>();

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            Scp330Interobject scp330 = RoomIdentifier.AllRoomIdentifiers.Where((r => r.Name == RoomName.Lcz330)).First().GetComponentInChildren<Scp330Interobject>();
            NetworkServer.UnSpawn(scp330.gameObject);
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            if (!player_loadouts.ContainsKey(player.PlayerId))
                player_loadouts.Add(player.PlayerId, new Loadout());
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            if (player_loadouts.ContainsKey(player.PlayerId))
            {
                Database.Singleton.SaveConfigLoadout(player);
                player_loadouts.Remove(player.PlayerId);
            }
        }

        [PluginEvent(ServerEventType.PlayerDropItem)]
        bool OnPlayerDropitem(Player player, ItemBase item)
        {
            bool drop_allowed = false;
            Loadout loadout = player_loadouts[player.PlayerId];
            if (InventoryMenu.GetPlayerMenuID(player) == (int)DeathmatchMenu.MenuPage.None)
            {
                if (IsGun(item.ItemTypeId))
                {
                    if (!loadout.locked)
                    {
                        if (item.ItemTypeId == loadout.primary)
                        {
                            loadout.primary = ItemType.None;
                            RemoveItem(player, item.ItemTypeId);
                        }
                        else if (item.ItemTypeId == loadout.secondary)
                        {
                            loadout.secondary = ItemType.None;
                            RemoveItem(player, item.ItemTypeId);
                        }
                        else if (item.ItemTypeId == loadout.tertiary)
                        {
                            loadout.tertiary = ItemType.None;
                            RemoveItem(player, item.ItemTypeId);
                        }

                        if (IsLoadoutEmpty(player))
                        {
                            BroadcastOverride.ClearLines(player, BroadcastPriority.High);
                            BroadcastOverride.BroadcastLine(player, 1, 300, BroadcastPriority.High, translation.CustomisationHint);
                            if (Lobby.InSpawn(player))
                            {
                                Lobby.CancelTeleport(player);
                                BroadcastOverride.BroadcastLine(player, 2, 300, BroadcastPriority.High, translation.Teleport);
                            }
                        }
                    }
                    else
                    {
                        BroadcastOverride.ClearLines(player, BroadcastPriority.High);
                        BroadcastOverride.BroadcastLines(player, 1, 3, BroadcastPriority.High, translation.CustomisationDenied);
                    }
                }
                else if (item.Category != ItemCategory.Armor)
                {
                    if (item.ItemTypeId == ItemType.Radio)
                    {
                        RemoveItem(player, ItemType.Radio);
                        BroadcastOverride.BroadcastLine(player, 1, 5, BroadcastPriority.High, translation.RadioDisableHint);
                    }
                    else if (loadout.locked)
                        drop_allowed = true;
                }
            }
            BroadcastOverride.UpdateIfDirty(player);
            return drop_allowed;
        }

        [PluginEvent(ServerEventType.PlayerDropAmmo)]
        bool OnPlayerDropAmmo(Player player, ItemType ammo, int amount)
        {
            return false;
        }

        [PluginEvent(ServerEventType.PlayerShotWeapon)]
        void OnPlayerShotWeapon(Player player, Firearm firearm)
        {
            player_loadouts[player.PlayerId].locked = true;
        }

        [PluginEvent(ServerEventType.PlayerUsedItem)]
        void OnPlayerUsedItem(Player player, ItemBase item)
        {
            player_loadouts[player.PlayerId].locked = true;
        }

        [PluginEvent(ServerEventType.PlayerDying)]
        void OnPlayerDying(Player target, Player killer, DamageHandlerBase damage)
        {
            target.ClearInventory();
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDeath(Player target, Player killer, DamageHandlerBase damage)
        {
            if (killer != null && killer.IsAlive)
            {
                if (killer.CurrentItem.Category == ItemCategory.Firearm)
                {
                    ItemType ammo = (killer.CurrentItem as Firearm).AmmoType;
                    killer.SetAmmo(ammo, (ushort)math.min(killer.GetAmmo(ammo) + (ushort)(killer.GetAmmoLimit(ammo) / 5), killer.GetAmmoLimit(ammo)));
                }
            }
        }

        [PluginEvent(ServerEventType.PlayerSpawn)]
        void OnPlayerSpawn(Player player, RoleTypeId role)
        {
            if (player == null || !player_loadouts.ContainsKey(player.PlayerId))
                return;

            Loadout loadout = player_loadouts[player.PlayerId];

            if (Lobby.GetSpawn(player).role == role)
            {
                loadout.locked = false;

                Timing.CallDelayed(0.0f, () =>
                {
                    if (player == null || !player_loadouts.ContainsKey(player.PlayerId))
                        return;
                    player.ClearInventory();
                    AddLoadoutStartItems(player);
                });
            }
        }

        [PluginEvent(ServerEventType.PlayerUsingRadio)]
        void OnPlayerUsingRadio(Player player, RadioItem radio, float drain)
        {
            radio.BatteryPercent = 100;
        }

        public static bool ValidateLoadout(Player player)
        {
            if (IsLoadoutEmpty(player))
            {
                BroadcastOverride.BroadcastLine(player, 1, 300, BroadcastPriority.High, translation.CustomisationHint);
                return false;
            }
            else
                return true;
        }

        public static Loadout GetLoadout(Player player)
        {
            return player_loadouts[player.PlayerId];
        }

        public static bool CustomiseLoadout(Player player)
        {
            Loadout loadout = GetLoadout(player);
            if (!loadout.locked)
            {
                loadout.customising = true;
                Lobby.CancelTeleport(player);
                return true;
            }
            else
            {
                BroadcastOverride.ClearLines(player, BroadcastPriority.High);
                BroadcastOverride.BroadcastLines(player, 1, 3, BroadcastPriority.High, translation.CustomisationDenied);
                return false;
            }
        }

        public static bool IsLoadoutEmpty(Player player)
        {
            Loadout loadout = player_loadouts[player.PlayerId];
            Killstreaks.Killstreak killstreak = Killstreaks.GetKillstreak(player);
            if (killstreak.mode == Killstreaks.KillstreakMode.Rage)
                return false;
            else if (killstreak.mode == Killstreaks.KillstreakMode.Easy)
                return loadout.primary == ItemType.None && loadout.secondary == ItemType.None && loadout.tertiary == ItemType.None;
            else
                return loadout.primary == ItemType.None && loadout.secondary == ItemType.None;
        }

        public static void AddLoadoutStartItems(Player player)
        {
            Loadout loadout = player_loadouts[player.PlayerId];
            Killstreaks.Killstreak killstreak = Killstreaks.GetKillstreak(player);

            player.AddItem(ItemType.KeycardO5);
            if (loadout.radio)
            {
                RadioItem radio = player.AddItem(ItemType.Radio) as RadioItem;
                radio._rangeId = (byte)(radio.Ranges.Length - 1);
            }

            if (!IsLoadoutEmpty(player))
            {
                if (killstreak.mode == Killstreaks.KillstreakMode.Easy)
                    player.AddItem(ItemType.ArmorHeavy);
                else if (killstreak.mode == Killstreaks.KillstreakMode.Standard)
                    player.AddItem(ItemType.ArmorCombat);
                else if (killstreak.mode == Killstreaks.KillstreakMode.Expert)
                    player.AddItem(ItemType.ArmorLight);

                if (killstreak.mode != Killstreaks.KillstreakMode.Rage)
                {
                    AddFirearm(player, loadout.primary, true);
                    AddFirearm(player, loadout.secondary, GunAmmoType(loadout.primary) != GunAmmoType(loadout.secondary));
                    if (killstreak.mode == Killstreaks.KillstreakMode.Easy)
                        AddFirearm(player, loadout.tertiary, GunAmmoType(loadout.primary) != GunAmmoType(loadout.tertiary) && GunAmmoType(loadout.secondary) != GunAmmoType(loadout.tertiary));
                }
                Killstreaks.Singleton.AddKillstreakStartItems(player);
            }
        }

        public static void SetGun(Player player, ItemType gun)
        {
            Loadout loadout = player_loadouts[player.PlayerId];

            if (loadout.slot == GunSlot.Primary)
                loadout.primary = gun;
            else if (loadout.slot == GunSlot.Secondary)
                loadout.secondary = gun;
            else if (loadout.slot == GunSlot.Tertiary)
                loadout.tertiary = gun;
        }
    }
}
