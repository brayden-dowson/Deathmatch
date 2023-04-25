using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using MapGeneration;
using Interactables.Interobjects.DoorUtils;
using Interactables.Interobjects;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;

//todo voice and spectate cmd
namespace TheRiptide
{
    public class Deathmatch
    {
        public static Deathmatch Singleton { get; private set; }

        private static bool game_started = false;
        public static SortedSet<int> players = new SortedSet<int>();
        public static float round_time = 30.0f;

        public static bool GameStarted
        {
            get => game_started;
            set
            {
                if(value == true)
                {
                    foreach (var player in Player.GetPlayers())
                        if (player.IsAlive)
                            Killstreaks.AddKillstreakEffects(player);
                }
                else
                {
                    foreach (var player in Player.GetPlayers())
                        if (player.IsAlive)
                            Lobby.ApplyGameNotStartedEffects(player);
                }
                game_started = value;
            }
        }

        public Deathmatch()
        {
            Singleton = this;
            Killfeeds.Init(2, 5, 20);
            DeathmatchMenu.SetupMenus();
            Lobby.Init();
        }

        [PluginEntryPoint("Deathmatch", "1.0", "needs no explanation", "The Riptide")]
        void EntryPoint()
        {
            EventManager.RegisterEvents(this);
            //dependecies
            EventManager.RegisterEvents<InventoryMenu>(this);
            EventManager.RegisterEvents<BroadcastOverride>(this);
            EventManager.RegisterEvents<FacilityManager>(this);

            //features
            EventManager.RegisterEvents<DataBase>(this);
            EventManager.RegisterEvents<Statistics>(this);
            EventManager.RegisterEvents<Killfeeds>(this);
            EventManager.RegisterEvents<Killstreaks>(this);
            EventManager.RegisterEvents<Loadouts>(this);
            EventManager.RegisterEvents<Lobby>(this);
            EventManager.RegisterEvents<Rooms>(this);
        }

        [PluginReload]
        void Reload()
        {
            EventManager.RegisterEvents(this);
            //dependecies
            EventManager.RegisterEvents<InventoryMenu>(this);
            EventManager.RegisterEvents<BroadcastOverride>(this);
            EventManager.RegisterEvents<FacilityManager>(this);

            //features
            EventManager.RegisterEvents<DataBase>(this);
            EventManager.RegisterEvents<Statistics>(this);
            EventManager.RegisterEvents<Killfeeds>(this);
            EventManager.RegisterEvents<Killstreaks>(this);
            EventManager.RegisterEvents<Loadouts>(this);
            EventManager.RegisterEvents<Lobby>(this);
            EventManager.RegisterEvents<Rooms>(this);
        }

        [PluginUnload]
        void Unload()
        {
            DataBase.PluginUnload();

            //features
            EventManager.UnregisterEvents<Rooms>(this);
            EventManager.UnregisterEvents<Lobby>(this);
            EventManager.UnregisterEvents<Loadouts>(this);
            EventManager.UnregisterEvents<Killstreaks>(this);
            EventManager.UnregisterEvents<Killfeeds>(this);
            EventManager.UnregisterEvents<Statistics>(this);
            EventManager.UnregisterEvents<DataBase>(this);

            //dependecies
            EventManager.UnregisterEvents<FacilityManager>(this);
            EventManager.UnregisterEvents<BroadcastOverride>(this);
            EventManager.UnregisterEvents<InventoryMenu>(this);

            EventManager.UnregisterEvents(this);
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        void WaitingForPlayers()
        {
        }

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            //foreach (var room in RoomIdentifier.AllRoomIdentifiers)
            //{
            //    SafeTeleportPosition stp = room.GetComponentInParent<SafeTeleportPosition>();
            //    if (stp != null && stp.SafePositions != null && !stp.SafePositions.IsEmpty() && stp.SafePositions[0] != null)
            //    {
            //        Transform transform = stp.SafePositions[0];
            //        ServerConsole.AddLog("room " + room.Name.ToString() + " | " + room.Shape.ToString() + " has a SafeTeleportPosition component " + transform.ToString(), ConsoleColor.White);
            //    }
            //    else
            //        ServerConsole.AddLog("room " + room.Name.ToString() + " | " + room.Shape.ToString() + " does not have a SafeTeleportPosition component", ConsoleColor.Red);
            //}

            Server.Instance.SetRole(RoleTypeId.Scp939);
            Server.Instance.ReferenceHub.nicknameSync.SetNick("[THE RIPTIDE]");
            Server.Instance.Position = new Vector3(128.8f, 994.0f, 18.0f);
            Server.FriendlyFire = true;

            Timing.CallDelayed(1.0f, () =>
            {
                try
                {
                    Server.Instance.ReferenceHub.serverRoles.Permissions = (ulong)PlayerPermissions.FacilityManagement;
                    CommandSystem.Commands.RemoteAdmin.Cleanup.ItemsCommand cmd = new CommandSystem.Commands.RemoteAdmin.Cleanup.ItemsCommand();
                    string response = "";
                    string[] empty = { "" };
                    cmd.Execute(new ArraySegment<string>(empty, 0, 0), new RemoteAdmin.PlayerCommandSender(Server.Instance.ReferenceHub), out response);
                    ServerConsole.AddLog(response);
                }
                catch (Exception ex)
                {
                    ServerConsole.AddLog(ex.ToString());
                }
            });
            if (round_time > 5.0f)
                Timing.CallDelayed(60.0f * (round_time - 5.0f), () => { BroadcastOverride.BroadcastLine(1, 30, BroadcastPriority.Medium, "<color=#43BFF0>Round Ends in 5 minutes</color>"); });
            if (round_time > 1.0f)
                Timing.CallDelayed(60.0f * (round_time - 1.0f), () => { BroadcastOverride.BroadcastLine(1, 30, BroadcastPriority.Medium, "<color=#43BFF0>Round Ends in 1 minute</color>"); });
            Timing.CallDelayed(60.0f * round_time, () => 
            {
                Statistics.DisplayRoundStats();
                Timing.CallPeriodically(20.0f, 0.2f, () =>
                {
                    foreach (var p in Player.GetPlayers())
                        p.EffectsManager.ChangeState<CustomPlayerEffects.DamageReduction>(200, 20.0f, false);
                });
                Timing.CallDelayed(20.0f, () => Round.Restart(false));
            });
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            players.Add(player.PlayerId);
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            players.Remove(player.PlayerId);
        }

        [PluginEvent(ServerEventType.RoundEndConditionsCheck)]
        RoundEndConditionsCheckCancellationData OnRoundEndConditionsCheck(bool baseGameConditionsSatisfied)
        {
            return RoundEndConditionsCheckCancellationData.Override(false);
        }
        [PluginEvent(ServerEventType.RoundRestart)]
        void OnRoundRestart()
        {
            Timing.KillCoroutines();
        }

        public static bool IsPlayerValid(Player player)
        {
            return players.Contains(player.PlayerId);
        }
    }
}
