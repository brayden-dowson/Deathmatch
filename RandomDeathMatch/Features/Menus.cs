using PlayerRoles;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;

using static TheRiptide.Utility;
using static TheRiptide.Translation;

namespace TheRiptide
{
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

        public enum MenuPage { None, Main, GunSlot1, GunSlot2, GunSlot3, GunClass, MtfGun, ChaosGun, KillstreakMode, KillstreakModeSecret, Preference, Role, Stats, DeleteData, LeaderBoard, Debug };

        private DeathmatchMenu() { }

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
                    Lobby.Singleton.TeleportOutOfSpawn(player);
                    return false;
                }),
                new MenuItem(ItemType.KeycardScientist, translation.CustomiseLoadout, (player)=>
                {
                    Killstreaks.Killstreak killstreak = Killstreaks.GetKillstreak(player);
                    if (!Killstreaks.Singleton.IsLoadoutLocked(player))
                    {
                        ItemType armor = Killstreaks.Singleton.ArmorType(player);
                        if(armor == ItemType.None)
                            InventoryMenu.ShowMenu(player, (int)MenuPage.GunSlot1);
                        else if(armor == ItemType.ArmorLight || armor == ItemType.ArmorCombat)
                            InventoryMenu.ShowMenu(player, (int)MenuPage.GunSlot2);
                        else if(armor == ItemType.ArmorHeavy)
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
                    if (loadout.rage_mode_enabled && Killstreaks.Singleton.config.RageEnabled)
                    {
                        InventoryMenu.ShowMenu(player, (int)MenuPage.KillstreakModeSecret);
                        info = InventoryMenu.GetInfo((int)MenuPage.KillstreakModeSecret);
                    }
                    else
                    {
                        InventoryMenu.ShowMenu(player, (int)MenuPage.KillstreakMode);
                        info = InventoryMenu.GetInfo((int)MenuPage.KillstreakMode);
                    }

                    BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, translation.CurrentKillstreakSelected.Replace("{killstreak}", "<color=" + Killstreaks.Singleton.KillstreakColorCode(player) + ">" + killstreak.name + "</color>"));
                    return false;
                }),
                new MenuItem(ItemType.KeycardContainmentEngineer, translation.Role, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Role);
                    return false;
                }),
                new MenuItem(ItemType.KeycardFacilityManager, translation.Preferences, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Preference);
                    return false;
                }),
                //new MenuItem(ItemType.Coin, "<color=#bd8f86>[COIN]</color> = <b><color=#43BFF0>[DEBUG MENU] dont forget to remove!!!</color></b>", (player)=>
                //{
                //    InventoryMenu.ShowMenu(player, (int)MenuPage.Debug);
                //    return false;
                //}),
            });

            InventoryMenu.CreateMenu((int)MenuPage.GunSlot1, translation.GunSlotMenu, new List<MenuItem>
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
                new MenuItem(ItemType.GunE11SR, "", (player)=> { return GunSelected(player, ItemType.GunE11SR);  })
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
                new MenuItem(ItemType.GunRevolver, "", (player)=> { GunSelected(player, ItemType.GunRevolver); return false; })
            });

            List<MenuItem> killstreak_items = new List<MenuItem>()
            {
                new MenuItem(ItemType.KeycardO5, translation.BackToMainMenu, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                })
            };

            foreach(var kvp in Killstreaks.Singleton.config.KillstreakTables)
            {
                if (kvp.Key != "RAGE")
                {
                    killstreak_items.Add(new MenuItem(kvp.Value.MenuItem, kvp.Value.MenuDescription, (player) =>
                    {
                        Killstreaks.GetKillstreak(player).name = kvp.Key;
                        InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                        MenuInfo info = InventoryMenu.GetInfo((int)MenuPage.Main);
                        BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, translation.KillstreakSelected.Replace("{killstreak}", "<color=" + kvp.Value.ColorHex + ">" + kvp.Key + "</color>"));
                        return false;
                    }));
                }
            }

            List<MenuItem> killstreak_items_secret = killstreak_items.ToList();
            if(Killstreaks.Singleton.config.KillstreakTables.ContainsKey("RAGE"))
            {
                KillstreakRewardTable table = Killstreaks.Singleton.config.KillstreakTables["RAGE"];
                killstreak_items_secret.Add(
                new MenuItem(table.MenuItem, table.MenuDescription, (player) =>
                {
                    Killstreaks.GetKillstreak(player).name = "RAGE";
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    MenuInfo info = InventoryMenu.GetInfo((int)MenuPage.Main);
                    BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, translation.KillstreakSelected.Replace("{killstreak}", "<color=" + table.ColorHex + ">RAGE</color>"));
                    return false;
                }));
            }

            InventoryMenu.CreateMenu((int)MenuPage.KillstreakMode, translation.KillstreakRewardMenu, killstreak_items);
            InventoryMenu.CreateMenu((int)MenuPage.KillstreakModeSecret, translation.KillstreakRewardMenu, killstreak_items_secret);

            Func<Player, RoleTypeId, bool> RoleSelected = (player, role) =>
            {
                InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                Lobby.Singleton.SetRole(player, role);
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
                new MenuItem(ItemType.KeycardScientist, translation.Stats, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Stats);
                    Statistics.DisplayStats(player,InventoryMenu.GetInfo((int)MenuPage.Stats).broadcast_lines + 1);
                    return false;
                }),
                new MenuItem(ItemType.Flashlight, translation.Spectator, (player)=>
                {
                    Lobby.Singleton.SetSpectatorMode(player, true);
                    return false;
                }),
                new MenuItem(ItemType.Coin, translation.EnableRage, (player)=>
                {
                    Loadouts.GetLoadout(player).rage_mode_enabled = true;
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
                    return false;
                }),
                new MenuItem(ItemType.KeycardJanitor, translation.DeleteData, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.DeleteData);
                    return false;
                }),
                new MenuItem(ItemType.SCP1576, translation.LeaderBoard, (player)=>
                {
                    InventoryMenu.ShowMenu(player, (int)MenuPage.LeaderBoard);
                    LeaderBoard.Singleton.EnableLeaderBoardMode(player, LeaderBoardType.Rank);
                    return false;
                })
            });

            InventoryMenu.CreateMenu((int)MenuPage.Stats, translation.StatsMenu, new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, translation.BackToPreferences, (player)=>
                {
                    BroadcastOverride.ClearLines(player, BroadcastPriority.Highest);
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Preference);
                    return false;
                })
            });

            InventoryMenu.CreateMenu((int)MenuPage.DeleteData, translation.DeleteDataMenu, new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, translation.BackToPreferences, (player)=>
                {
                    BroadcastOverride.ClearLines(player, BroadcastPriority.Highest);
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Preference);
                    return false;
                }),
                new MenuItem(ItemType.KeycardJanitor, translation.AreYouSure, (player)=>
                {
                    if(player.DoNotTrack)
                    {
                        Database.Singleton.DeleteData(player);
                        InventoryMenu.ShowMenu(player, (int)MenuPage.Preference);
                        MenuInfo info = InventoryMenu.GetInfo((int)MenuPage.Preference);
                        BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500, BroadcastPriority.High, translation.DeletedData);
                    }
                    else
                        BroadcastOverride.BroadcastLine(player, 3, 1500, BroadcastPriority.High, translation.FailedToDeleteData);
                    return false;
                })
            });

            InventoryMenu.CreateMenu((int)MenuPage.LeaderBoard, translation.LeaderBoardMenu, new List<MenuItem>
            {
                new MenuItem(ItemType.KeycardO5, translation.BackToPreferences, (player)=>
                {
                    BroadcastOverride.ClearLines(player, BroadcastPriority.Highest);
                    InventoryMenu.ShowMenu(player, (int)MenuPage.Preference);
                    LeaderBoard.Singleton.DisableLeaderBoardMode(player);
                    return false;
                })
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
                    DmRound.GameStarted = true;
                    BroadcastOverride.BroadcastLine(player, 7, 3.0f, BroadcastPriority.High, "start");
                    //Rooms.ExpandFacility(1);
                    return false;
                }),
                new MenuItem(ItemType.KeycardNTFCommander, "end game", (player)=>
                {
                    DmRound.GameStarted = false;
                    BroadcastOverride.BroadcastLine(player, 7, 3.0f, BroadcastPriority.High, "end");
                    //Rooms.ShrinkFacility(1);
                    return false;
                })
            });
        }

        public void ClearMenus()
        {
            InventoryMenu.Clear();
        }
    }
}
