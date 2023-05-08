using PlayerRoles;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;

using static TheRiptide.Utility;
using static TheRiptide.Translation;

namespace TheRiptide
{
    public class MenuConfig
    {
        public bool RageEnabled { get; set; } = true;
    }

    public class DeathmatchMenu
    {
        private static DeathmatchMenu instance = null;
        public static DeathmatchMenu Singleton 
        {
            get
            {
                if (instance == null)
                    instance = new DeathmatchMenu();
                return instance;
            }
        }

        public MenuConfig config;

        public enum MenuPage { None, Main, GunSlot2, GunSlot3, GunClass, MtfGun, ChaosGun, KillstreakMode, KillstreakModeSecret, Preference, Role, Stats, Debug };

        private DeathmatchMenu() { }

        public void Init(MenuConfig config)
        {
            this.config = config;
        }

        public void SetupMenus()
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

            InventoryMenu.CreateMenu((int)MenuPage.Main, translation.MainMenu, new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, translation.SaveAndExit, (player)=>
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
                new MenuItem(ItemType.KeycardScientist, translation.CustomiseLoadout, (player)=>
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
                new MenuItem(ItemType.KeycardResearchCoordinator, translation.KillstreakRewardSystem, (player)=>
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

                    BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, translation.CurrentKillstreakSelected.Replace("{killstreak}", Killstreaks.KillstreakColorCode(player) + killstreak.mode.ToString()));
                    return false;
                }),
                new MenuItem(ItemType.KeycardContainmentEngineer, translation.Preferences, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Preference);
                    return false;
                }),
                new MenuItem(ItemType.KeycardFacilityManager, translation.Role, (player)=>
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

            InventoryMenu.CreateMenu((int)MenuPage.GunSlot2, translation.GunSlotMenu, new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, translation.BackToMainMenu, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.KeycardFacilityManager, translation.Primary, (player)=>
                {
                    Loadouts.GetLoadout(player).slot = Loadouts.GunSlot.Primary;
                    InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
                    return false;
                }),
                new MenuItem(ItemType.KeycardZoneManager, translation.Secondary, (player)=>
                {
                    Loadouts.GetLoadout(player).slot = Loadouts.GunSlot.Secondary;
                    InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
                    return false;
                }),
                new MenuItem(ItemType.Radio,"",(p)=>{ return false; })
            });

            InventoryMenu.CreateMenu((int)MenuPage.GunSlot3, translation.GunSlotMenu, new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, translation.BackToMainMenu, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.KeycardNTFCommander, translation.HeavyPrimary, (player)=>
                {
                    Loadouts.GetLoadout(player).slot = Loadouts.GunSlot.Primary;
                    InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
                    return false;
                }),
                new MenuItem(ItemType.KeycardNTFLieutenant, translation.HeavySecondary, (player)=>
                {
                    Loadouts.GetLoadout(player).slot = Loadouts.GunSlot.Secondary;
                    InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
                    return false;
                }),
                new MenuItem(ItemType.KeycardNTFOfficer, translation.HeavyTertiary, (player)=>
                {
                    Loadouts.GetLoadout(player).slot = Loadouts.GunSlot.Tertiary;
                    InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
                    return false;
                }),
                new MenuItem(ItemType.Radio,"",(p)=>{ return false; })
            });

            InventoryMenu.CreateMenu((int)MenuPage.GunClass, translation.GunClassMenu, new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, translation.BackToMainMenu, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.KeycardNTFCommander, translation.MtfGuns, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.MtfGun);
                    return false;
                }),
                new MenuItem(ItemType.KeycardChaosInsurgency, translation.ChaosGuns, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.ChaosGun);
                    return false;
                }),
                new MenuItem(ItemType.Radio,"",(p)=>{ return false; })
            });

            Func<Player, ItemType, bool> GunSelected = (player, gun) =>
            {
                Loadouts.Loadout loadout = Loadouts.GetLoadout(player);
                if (Loadouts.Singleton.SetGun(player, gun))
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    MenuInfo info = InventoryMenu.GetInfo((int)MenuPage.Main);
                    string gun_name = Enum.GetName(typeof(ItemType), gun).Substring(3);
                    BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, translation.GunSelected.Replace("{gun}", gun_name).Replace("{slot}", loadout.slot.ToString()));
                }
                return false;
            };


            InventoryMenu.CreateMenu((int)MenuPage.MtfGun, translation.MtfGunMenu, new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, translation.BackToMainMenu, (player)=>
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

            InventoryMenu.CreateMenu((int)MenuPage.ChaosGun, translation.ChaosGunMenu, new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, translation.BackToMainMenu, (player)=>
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
                BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, translation.KillstreakSelected.Replace("{killstreak}", Killstreaks.KillstreakColorCode(player) + Enum.GetName(typeof(Killstreaks.KillstreakMode), killstreak.mode)));
                return false;
            };

            List<MenuItem> killstreak_items = new List<MenuItem>()
            { 
                new MenuItem(ItemType.KeycardO5, translation.BackToMainMenu, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.ArmorHeavy, translation.Easy, (player)=>
                {
                    return KillstreakModeSelected(player, Killstreaks.KillstreakMode.Easy);
                }),
                new MenuItem(ItemType.ArmorCombat, translation.Standard, (player)=>
                {
                    return KillstreakModeSelected(player, Killstreaks.KillstreakMode.Standard);
                }),
                new MenuItem(ItemType.ArmorLight, translation.Expert, (player)=>
                {
                    return KillstreakModeSelected(player, Killstreaks.KillstreakMode.Expert);
                })
            };

            List<MenuItem> killstreak_items_secret = killstreak_items.ToList();
            killstreak_items_secret.Add(
            new MenuItem(ItemType.GunCom45, translation.Rage, (player) =>
            {
                return KillstreakModeSelected(player, Killstreaks.KillstreakMode.Rage);
            }));

            killstreak_items.Add(new MenuItem(ItemType.Radio, "", (p) => { return false; }));
            killstreak_items_secret.Add(new MenuItem(ItemType.Radio, "", (p) => { return false; }));

            InventoryMenu.CreateMenu((int)MenuPage.KillstreakMode, translation.KillstreakRewardMenu, killstreak_items);
            InventoryMenu.CreateMenu((int)MenuPage.KillstreakModeSecret, translation.KillstreakRewardMenu, killstreak_items_secret);

            Func<Player, RoleTypeId, bool> RoleSelected = (player, role) =>
            {
                InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                Lobby.SetRole(player, role);
                MenuInfo info = InventoryMenu.GetInfo((int)MenuPage.Main);
                BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, translation.RoleSelected.Replace("{role}", role.ToString()));
                return false;
            };

            InventoryMenu.CreateMenu((int)MenuPage.Role, translation.RoleMenu, new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, translation.BackToMainMenu, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.KeycardJanitor, translation.ClassD, (player)=>
                {
                    return RoleSelected(player, RoleTypeId.ClassD);
                }),
                new MenuItem(ItemType.KeycardScientist, translation.Scientist, (player)=>
                {
                    return RoleSelected(player, RoleTypeId.Scientist);
                }),
                new MenuItem(ItemType.KeycardGuard, translation.Guard, (player)=>
                {
                    return RoleSelected(player, RoleTypeId.FacilityGuard);
                }),
                new MenuItem(ItemType.KeycardNTFOfficer, translation.Private, (player)=>
                {
                    return RoleSelected(player, RoleTypeId.NtfPrivate);
                }),
                new MenuItem(ItemType.KeycardNTFLieutenant, translation.Sergeant, (player)=>
                {
                    return RoleSelected(player, RoleTypeId.NtfSergeant);
                }),
                new MenuItem(ItemType.KeycardNTFCommander, translation.Captain, (player)=>
                {
                    return RoleSelected(player, RoleTypeId.NtfCaptain);
                }),
                new MenuItem(ItemType.KeycardChaosInsurgency, translation.Chaos, (player)=>
                {
                    return RoleSelected(player, RoleTypeId.ChaosConscript);
                })
            });

            InventoryMenu.CreateMenu((int)MenuPage.Preference, translation.PreferencesMenu, new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, translation.BackToMainMenu, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.KeycardGuard, translation.ToggleRadio, (player)=>
                {
                    Loadouts.Loadout loadout = Loadouts.GetLoadout(player);
                    loadout.radio = !loadout.radio;
                    MenuInfo info = InventoryMenu.GetInfo((int)MenuPage.Preference);
                    BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, translation.RadioToggled.Replace("{state}", (loadout.radio ? "Enabled" : "Disabled")));
                    return false;
                }),
                new MenuItem(ItemType.KeycardScientist, translation.Stats, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Stats);
                    Statistics.DisplayStats(player,InventoryMenu.GetInfo((int)MenuPage.Stats).broadcast_lines + 1);
                    return false;
                }),
                new MenuItem(ItemType.Flashlight, translation.Spectator, (player)=>
                {
                    Lobby.SetSpectatorMode(player, true);
                    return false;
                }),
                new MenuItem(ItemType.Coin, translation.EnableRage, (player)=>
                {
                    Loadouts.GetLoadout(player).rage_mode_enabled = true;
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.Radio, "", (p) => { return false; })
            });

            InventoryMenu.CreateMenu((int)MenuPage.Stats, translation.StatsMenu, new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, translation.BackToPreferences, (player)=>
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

        public void ClearMenus()
        {
            InventoryMenu.Clear();
        }
    }
}
