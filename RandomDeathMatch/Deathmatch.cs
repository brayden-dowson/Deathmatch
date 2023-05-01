using System;
using System.Collections.Generic;

using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using PlayerRoles;
using UnityEngine;
using System.ComponentModel;

//todo voice and spectate cmd
namespace TheRiptide
{
    public class MainConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("round time in minutes")]
        public float RoundTime { get; set; } = 30.0f;

        public string DummyPlayerName { get; set; } = "[THE RIPTIDE]";
    }

    public class Deathmatch
    {
        public static Deathmatch Singleton { get; private set; }

        [PluginConfig("main_config.yml")]
        public MainConfig config;

        [PluginConfig("rooms_config.yml")]
        public RoomsConfig rooms_config;

        [PluginConfig("killstreak_config.yml")]
        public KillstreakConfig killstreak_config;

        [PluginConfig("menu_config.yml")]
        public MenuConfig menu_config;

        [PluginConfig("experience_config.yml")]
        public ExperienceConfig experience_config;

        [PluginConfig("rank_config.yml")]
        public RankConfig rank_config;

        [PluginConfig("tracking_config.yml")]
        public TrackingConfig tracking_config;

        private static bool game_started = false;
        public static SortedSet<int> players = new SortedSet<int>();

        public static bool GameStarted
        {
            get => game_started;
            set
            {
                if (value == true)
                {
                    foreach (var player in Player.GetPlayers())
                        if (player.IsAlive)
                            Killstreaks.Singleton.AddKillstreakEffects(player);
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
            DeathmatchMenu.Singleton.SetupMenus();
            Lobby.Init();
        }

        public void Start()
        {
            DataBase.Singleton.Load();

            EventManager.RegisterEvents(this);
            //dependencies
            EventManager.RegisterEvents<InventoryMenu>(this);
            EventManager.RegisterEvents<BroadcastOverride>(this);
            EventManager.RegisterEvents<FacilityManager>(this);

            //features
            EventManager.RegisterEvents<Statistics>(this);
            EventManager.RegisterEvents<Killfeeds>(this);
            EventManager.RegisterEvents<Killstreaks>(this);
            EventManager.RegisterEvents<Loadouts>(this);
            EventManager.RegisterEvents<Lobby>(this);
            EventManager.RegisterEvents<Rooms>(this);
            if (rank_config.IsEnabled)
                EventManager.RegisterEvents<Ranks>(this);
            if (experience_config.IsEnabled)
                EventManager.RegisterEvents<Experiences>(this);
            if (tracking_config.IsEnabled)
                EventManager.RegisterEvents<Tracking>(this);


            Rooms.Singleton.Init(rooms_config);
            Killstreaks.Singleton.Init(killstreak_config);
            DeathmatchMenu.Singleton.Init(menu_config);
            Experiences.Singleton.Init(experience_config);
            Ranks.Singleton.Init(rank_config);
            Tracking.Singleton.Init(tracking_config);
        }

        public void Stop()
        {
            DataBase.Singleton.UnLoad();

            //features
            EventManager.UnregisterEvents<Tracking>(this);
            EventManager.UnregisterEvents<Experiences>(this);
            EventManager.UnregisterEvents<Ranks>(this);
            EventManager.UnregisterEvents<Rooms>(this);
            EventManager.UnregisterEvents<Lobby>(this);
            EventManager.UnregisterEvents<Loadouts>(this);
            EventManager.UnregisterEvents<Killstreaks>(this);
            EventManager.UnregisterEvents<Killfeeds>(this);
            EventManager.UnregisterEvents<Statistics>(this);

            //dependencies
            EventManager.UnregisterEvents<FacilityManager>(this);
            EventManager.UnregisterEvents<BroadcastOverride>(this);
            EventManager.UnregisterEvents<InventoryMenu>(this);

            EventManager.UnregisterEvents(this);
        }

        [PluginEntryPoint("Deathmatch", "1.0", "needs no explanation", "The Riptide")]
        void EntryPoint()
        {
            if (config.IsEnabled)
                Start();
        }

        [PluginUnload]
        void Unload()
        {
            Stop();
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        void WaitingForPlayers()
        {
            DataBase.Singleton.Checkpoint();
        }

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            Server.Instance.SetRole(RoleTypeId.Scp939);
            Server.Instance.ReferenceHub.nicknameSync.SetNick(config.DummyPlayerName);
            Server.Instance.Position = new Vector3(128.8f, 994.0f, 18.0f);
            Server.FriendlyFire = true;
            FriendlyFireConfig.PauseDetector = true;
            Server.IsHeavilyModded = true;

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
            if (config.RoundTime > 5.0f)
                Timing.CallDelayed(60.0f * (config.RoundTime - 5.0f), () => { BroadcastOverride.BroadcastLine(1, 30, BroadcastPriority.Medium, "<color=#43BFF0>Round Ends in 5 minutes</color>"); });
            if (config.RoundTime > 1.0f)
                Timing.CallDelayed(60.0f * (config.RoundTime - 1.0f), () => { BroadcastOverride.BroadcastLine(1, 30, BroadcastPriority.Medium, "<color=#43BFF0>Round Ends in 1 minute</color>"); });
            Timing.CallDelayed(60.0f * config.RoundTime, () => 
            {
                Ranks.Singleton.CalculateAndSaveRanks();
                Experiences.Singleton.SaveExperiences();
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
            DataBase.Singleton.LoadConfig(player);
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            DataBase.Singleton.SaveConfig(player);
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
