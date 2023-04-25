using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

using static TheRiptide.Utility;

namespace TheRiptide
{
    public class MenuConfig
    {
        public bool RageEnabled { get; set; } = true;
    }

    public class DeathmatchMenu
    {
        public static DeathmatchMenu Singleton { get; private set; }

        public MenuConfig config;

        public enum MenuPage { None, Main, GunSlot2, GunSlot3, GunClass, MtfGun, ChaosGun, KillstreakMode, KillstreakModeSecret, Preference, Role, Stats, Debug };

        public DeathmatchMenu()
        {
            Singleton = this;
            config = Deathmatch.Singleton.menu_config;
        }

        public static void SetupMenus()
        {
            InventoryMenu.CreateMenu((int)MenuPage.None, "", new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, "", (player)=>
                {
                    if(Loadouts.CustomiseLoadout(player))
                        InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                })
            });

            InventoryMenu.CreateMenu((int)MenuPage.Main, "<b><color=#43BFF0>[MAIN MENU]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b><color=#5900ff>Save and Exit</color></b>", (player)=>
                {
                    Loadouts.Loadout loadout = Loadouts.GetLoadout(player);
                    loadout.customising = false;
                    InventoryMenu.SetMenu(player, (int)MenuPage.None);
                    BroadcastOverride.ClearLines(player, BroadcastPriority.High);
                    Killfeeds.SetBroadcastKillfeedLayout(player);

                    player.ClearInventory();
                    Loadouts.AddLoadoutStartItems(player);
                    Lobby.TeleportOutOfSpawn(player);
                    return false;
                }),
                new MenuItem(ItemType.KeycardScientist, "<color=#e7d77b>[SCIENTIST]</color> = <b><color=#FF0000>Customise Loadout - </color><color=#43BFF0>[GUN SLOT]</color></b>", (player)=>
                {
                    Killstreaks.Killstreak killstreak = Killstreaks.GetKillstreak(player);
                    if (killstreak.mode != Killstreaks.KillstreakMode.Rage)
                    {
                        if (killstreak.mode == Killstreaks.KillstreakMode.Expert || killstreak.mode == Killstreaks.KillstreakMode.Standard)
                            InventoryMenu.ShowMenu(player, (int)MenuPage.GunSlot2);
                        else
                            InventoryMenu.ShowMenu(player, (int)MenuPage.GunSlot3);
                    }
                    else
                    {
                        BroadcastOverride.BroadcastLine(player, 3, 1, BroadcastPriority.High, "<b>[DATA EXPUNGED]</b>");
                        RemoveItem(player, ItemType.KeycardScientist);
                    }
                    return false;
                }),
                new MenuItem(ItemType.KeycardResearchCoordinator, "<color=#e1ab21>[RESEARCH SUPERVISOR]</color> = <b><color=#43BFF0>[KILLSTREAK REWARD SYSTEM]</color></b>", (player)=>
                {
                    Killstreaks.Killstreak killstreak = Killstreaks.GetKillstreak(player);
                    Loadouts.Loadout loadout = Loadouts.GetLoadout(player);
                    MenuInfo info;
                    if (loadout.rage_mode_enabled && Singleton.config.RageEnabled)
                    {
                        InventoryMenu.ShowMenu(player, (int)MenuPage.KillstreakModeSecret);
                        info = InventoryMenu.GetInfo((int)MenuPage.KillstreakModeSecret);
                    }
                    else
                    {
                        InventoryMenu.ShowMenu(player, (int)MenuPage.KillstreakMode);
                        info = InventoryMenu.GetInfo((int)MenuPage.KillstreakMode);
                    }

                    BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, "Current killstreak reward system selected: " + Killstreaks.KillstreakColorCode(player) + killstreak.mode.ToString() + "</color>");
                    return false;
                }),
                new MenuItem(ItemType.KeycardContainmentEngineer, "<color=#bd8f86>[CONTAINMENT ENGINEER]</color> = <b><color=#43BFF0>[PREFERENCES]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Preference);
                    return false;
                }),
                new MenuItem(ItemType.KeycardFacilityManager, "<color=#bd1a4a>[FACILITY MANAGER]</color> = <b><color=#43BFF0>[ROLE]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Role);
                    return false;
                }),
                //new MenuItem(ItemType.Coin, "<color=#bd8f86>[COIN]</color> = <b><color=#43BFF0>[DEBUG MENU] dont forget to remove!!!</color></b>", (player)=>
                //{
                //    InventoryMenu.ShowMenu(player, (int)MenuPage.Debug);
                //    return false;
                //}),
                new MenuItem(ItemType.Radio,"",(p)=>{ return false; })
            });

            InventoryMenu.CreateMenu((int)MenuPage.GunSlot2, "<b><color=#43BFF0>[GUN SLOT]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.KeycardFacilityManager, "<color=#bd1a4a>[FACILITY MANAGER]</color> = <b><color=#FF0000>Primary - </color><color=#43BFF0>[GUN CLASS]</color></b>", (player)=>
                {
                    Loadouts.GetLoadout(player).slot = Loadouts.GunSlot.Primary;
                    InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
                    return false;
                }),
                new MenuItem(ItemType.KeycardZoneManager, "<color=#217b7b>[ZONE MANAGER]</color> = <b>Secondary - <color=#43BFF0>[GUN CLASS]</color></b>", (player)=>
                {
                    Loadouts.GetLoadout(player).slot = Loadouts.GunSlot.Secondary;
                    InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
                    return false;
                }),
                new MenuItem(ItemType.Radio,"",(p)=>{ return false; })
            });

            InventoryMenu.CreateMenu((int)MenuPage.GunSlot3, "<b><color=#43BFF0>[GUN SLOT]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.KeycardNTFCommander, "<color=#1b43cb>[CAPTAIN]</color> = <b><color=#FF0000>Primary - </color><color=#43BFF0>[GUN CLASS]</color></b>", (player)=>
                {
                    Loadouts.GetLoadout(player).slot = Loadouts.GunSlot.Primary;
                    InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
                    return false;
                }),
                new MenuItem(ItemType.KeycardNTFLieutenant, "<color=#177dde>[SERGEANT]</color> = <b>Secondary - <color=#43BFF0>[GUN CLASS]</color></b>", (player)=>
                {
                    Loadouts.GetLoadout(player).slot = Loadouts.GunSlot.Secondary;
                    InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
                    return false;
                }),
                new MenuItem(ItemType.KeycardNTFOfficer, "<color=#accfe1>[PRIVATE]</color> = <b>Tertiary - <color=#43BFF0>[GUN CLASS]</color></b>", (player)=>
                {
                    Loadouts.GetLoadout(player).slot = Loadouts.GunSlot.Tertiary;
                    InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
                    return false;
                }),
                new MenuItem(ItemType.Radio,"",(p)=>{ return false; })
            });

            InventoryMenu.CreateMenu((int)MenuPage.GunClass, "<b><color=#43BFF0>[GUN CLASS]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.KeycardNTFCommander, "<color=#1b43cb>[CAPTAIN]</color> = <b><color=#1b43cb>[MTF GUNS]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.MtfGun);
                    return false;
                }),
                new MenuItem(ItemType.KeycardChaosInsurgency, "<color=#008f1c>[CHAOS]</color> = <b><color=#008f1c>[CHAOS GUNS]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.ChaosGun);
                    return false;
                }),
                new MenuItem(ItemType.Radio,"",(p)=>{ return false; })
            });

            Func<Player, ItemType, bool> GunSelected = (player, gun) =>
            {
                Loadouts.Loadout loadout = Loadouts.GetLoadout(player);
                Loadouts.SetGun(player, gun);
                InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                MenuInfo info = InventoryMenu.GetInfo((int)MenuPage.Main);
                string gun_name = Enum.GetName(typeof(ItemType), gun).Substring(3);
                BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, "<color=#43BFF0>" + gun_name + "</color> added to your loadout as the <color=#FF0000>" + loadout.slot.ToString() + "</color> weapon");
                return false;
            };

            InventoryMenu.CreateMenu((int)MenuPage.MtfGun, "<b><color=#1b43cb>[MTF GUNS]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.GunCOM15, "", (player)=> { return GunSelected(player, ItemType.GunCOM15); }),
                new MenuItem(ItemType.GunCOM18, "", (player)=> { return GunSelected(player, ItemType.GunCOM18);  }),
                new MenuItem(ItemType.GunFSP9, "", (player)=> { return GunSelected(player, ItemType.GunFSP9);  }),
                new MenuItem(ItemType.GunCrossvec, "", (player)=> { return GunSelected(player, ItemType.GunCrossvec); }),
                new MenuItem(ItemType.GunE11SR, "", (player)=> { return GunSelected(player, ItemType.GunE11SR);  }),
                new MenuItem(ItemType.Radio,"",(p)=>{ return false; })
            });

            InventoryMenu.CreateMenu((int)MenuPage.ChaosGun, "<b><color=#008f1c>[CHAOS GUNS]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.GunAK, "", (player)=> { GunSelected(player, ItemType.GunAK); return false; }),
                new MenuItem(ItemType.GunLogicer, "", (player)=> { GunSelected(player, ItemType.GunLogicer); return false; }),
                new MenuItem(ItemType.GunShotgun, "", (player)=> { GunSelected(player, ItemType.GunShotgun); return false; }),
                new MenuItem(ItemType.GunRevolver, "", (player)=> { GunSelected(player, ItemType.GunRevolver); return false; }),
                new MenuItem(ItemType.Radio,"",(p)=>{ return false; })
            });

            Func<Player, Killstreaks.KillstreakMode, bool> KillstreakModeSelected = (player, mode) =>
            {
                Killstreaks.Killstreak killstreak = Killstreaks.GetKillstreak(player);
                killstreak.mode = mode;
                InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                MenuInfo info = InventoryMenu.GetInfo((int)MenuPage.Main);
                BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, Killstreaks.KillstreakColorCode(player) + Enum.GetName(typeof(Killstreaks.KillstreakMode), killstreak.mode) + "</color> selected as your killstreak reward system");
                return false;
            };

            List<MenuItem> killstreak_items = new List<MenuItem>()
            {
                new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.ArmorHeavy, "<color=#eb0d47>[HEAVY ARMOR]</color> = <b><color=#5900ff>Easy</color>: low risk low reward, good loadout but bad killstreak rewards</b>", (player)=>
                {
                    return KillstreakModeSelected(player, Killstreaks.KillstreakMode.Easy);
                }),
                new MenuItem(ItemType.ArmorCombat, "<color=#eb0d47>[COMBAT ARMOR]</color> = <b><color=#43BFF0>Standard</color>: medium risk medium reward, ok loadout and ok killsteak rewards</b>", (player)=>
                {
                    return KillstreakModeSelected(player, Killstreaks.KillstreakMode.Standard);
                }),
                new MenuItem(ItemType.ArmorLight, "<color=#eb0d47>[LIGHT ARMOR]</color> = <b><color=#36a832>Expert</color>: high risk high reward, bad loadout but good killstreak rewards </b>", (player)=>
                {
                    return KillstreakModeSelected(player, Killstreaks.KillstreakMode.Expert);
                })
            };

            List<MenuItem> killstreak_items_secret = killstreak_items.ToList();
            killstreak_items_secret.Add(
            new MenuItem(ItemType.GunCom45, "<color=#eb0d47>[COM 45]</color> = <b><color=#FF0000>RAGE</color> - [DATA EXPUNGED]</b>", (player) =>
            {
                return KillstreakModeSelected(player, Killstreaks.KillstreakMode.Rage);
            }));

            killstreak_items.Add(new MenuItem(ItemType.Radio, "", (p) => { return false; }));
            killstreak_items_secret.Add(new MenuItem(ItemType.Radio, "", (p) => { return false; }));

            InventoryMenu.CreateMenu((int)MenuPage.KillstreakMode, "<b><color=#43BFF0>[KILLSTREAK REWARD SYSTEM]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", killstreak_items);
            InventoryMenu.CreateMenu((int)MenuPage.KillstreakModeSecret, "<b><color=#43BFF0>[KILLSTREAK REWARD SYSTEM]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", killstreak_items_secret);

            Func<Player, RoleTypeId, bool> RoleSelected = (player, role) =>
            {
                InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                Lobby.SetRole(player, role);
                MenuInfo info = InventoryMenu.GetInfo((int)MenuPage.Main);
                BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, role.ToString() + " selected as role");
                return false;
            };

            InventoryMenu.CreateMenu((int)MenuPage.Role, "<b><color=#43BFF0>[ROLE]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.KeycardJanitor, "<color=#bdafe4>[JANITOR]</color> = <b><color=#FF8E00>Class-D</color></b>", (player)=>
                {
                    return RoleSelected(player, RoleTypeId.ClassD);
                }),
                new MenuItem(ItemType.KeycardScientist, "<color=#e7d77b>[SCIENTIST]</color> = <b><color=#FFFF7C>Scientist</color></b>", (player)=>
                {
                    return RoleSelected(player, RoleTypeId.Scientist);
                }),
                new MenuItem(ItemType.KeycardGuard, "<color=#5B6370>[GUARD]</color> = <b><color=#5B6370>Facility Guard</color></b>", (player)=>
                {
                    return RoleSelected(player, RoleTypeId.FacilityGuard);
                }),
                new MenuItem(ItemType.KeycardNTFOfficer, "<color=#accfe1>[PRIVATE]</color> = <b><color=#accfe1>NTF Private</color></b>", (player)=>
                {
                    return RoleSelected(player, RoleTypeId.NtfPrivate);
                }),
                new MenuItem(ItemType.KeycardNTFLieutenant, "<color=#177dde>[SERGEANT]</color> = <b><color=#177dde>NTF Sergeant</color></b>", (player)=>
                {
                    return RoleSelected(player, RoleTypeId.NtfSergeant);
                }),
                new MenuItem(ItemType.KeycardNTFCommander, "<color=#1b43cb>[CAPTAIN]</color> = <b><color=#1b43cb>NTF Captain</color></b>", (player)=>
                {
                    return RoleSelected(player, RoleTypeId.NtfCaptain);
                }),
                new MenuItem(ItemType.KeycardChaosInsurgency, "<color=#008f1c>[CHAOS]</color> = <b><color=#008f1c>Chaos</color></b>", (player)=>
                {
                    return RoleSelected(player, RoleTypeId.ChaosConscript);
                })
            });

            InventoryMenu.CreateMenu((int)MenuPage.Preference, "<b><color=#43BFF0>[PREFERENCES]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.KeycardGuard, "<color=#eb0d47>[GUARD]</color> = <b>Toggle Loadout Radio</b>", (player)=>
                {
                    Loadouts.Loadout loadout = Loadouts.GetLoadout(player);
                    loadout.radio = !loadout.radio;
                    MenuInfo info = InventoryMenu.GetInfo((int)MenuPage.Preference);
                    BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, "<b><color=#43BFF0>Loadout Radio: </color></b>" + (loadout.radio ? "Enabled" : "Disabled"));
                    return false;
                }),
                new MenuItem(ItemType.KeycardScientist, "<color=#eb0d47>[SCIENTIST]</color> = <b><color=#43BFF0>[STATS]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Stats);
                    Statistics.DisplayStats(player,InventoryMenu.GetInfo((int)MenuPage.Stats).broadcast_lines + 1);
                    return false;
                }),
                new MenuItem(ItemType.Flashlight, "<color=#eb0d47>[FLASH LIGHT]</color> = <b>Enable spectator mode</b>", (player)=>
                {
                    Lobby.SetSpectatorMode(player, true);
                    return false;
                }),
                new MenuItem(ItemType.Coin, "<color=#eb0d47>[COIN]</color> = <b>Enable [DATA EXPUNGED]</b>", (player)=>
                {
                    Loadouts.GetLoadout(player).rage_mode_enabled = true;
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.Radio,"",(p)=>{ return false; })
            });

            InventoryMenu.CreateMenu((int)MenuPage.Stats, "<b><color=#43BFF0>[STATS]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[PREFERENCES]</color></b>", (player)=>
                {
                    BroadcastOverride.ClearLines(player, BroadcastPriority.Highest);
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Preference);
                    return false;
                }),
                new MenuItem(ItemType.Radio,"",(p)=>{ return false; })
            });

            InventoryMenu.CreateMenu((int)MenuPage.Debug, "<b><color=#43BFF0>[DEBUG]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.KeycardGuard, "close all rooms", (player)=>
                {
                    BroadcastOverride.BroadcastLine(player, 7, 3.0f, BroadcastPriority.High, "closed all rooms");
                    //Rooms.LockdownFacility();
                    return false;
                }),
                new MenuItem(ItemType.KeycardNTFOfficer, "open all rooms", (player)=>
                {
                    BroadcastOverride.BroadcastLine(player, 7, 3.0f, BroadcastPriority.High, "opened all rooms");
                    //Rooms.UnlockFacility();
                    return false;
                }),
                new MenuItem(ItemType.KeycardNTFLieutenant, "start game", (player)=>
                {
                    Deathmatch.GameStarted = true;
                    BroadcastOverride.BroadcastLine(player, 7, 3.0f, BroadcastPriority.High, "start");
                    //Rooms.ExpandFacility(1);
                    return false;
                }),
                new MenuItem(ItemType.KeycardNTFCommander, "end game", (player)=>
                {
                    Deathmatch.GameStarted = false;
                    BroadcastOverride.BroadcastLine(player, 7, 3.0f, BroadcastPriority.High, "end");
                    //Rooms.ShrinkFacility(1);
                    return false;
                }),
                new MenuItem(ItemType.Radio,"",(p)=>{ return false; })
            });
        }
    }
}
