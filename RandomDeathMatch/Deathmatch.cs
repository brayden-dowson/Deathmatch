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
using CustomPlayerEffects;
using InventorySystem.Items.Firearms.Attachments;
using LightContainmentZoneDecontamination;
using PlayerStatsSystem;
using static TheRiptide.Translation;
using static TheRiptide.EventSubscriber;

//todo voice and spectate cmd
namespace TheRiptide
{
    public class MainConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("round time in minutes")]
        public float RoundTime { get; set; } = 20.0f;
        [Description("round end in seconds")]
        public float RoundEndTime { get; set; } = 30.0f;

        public string DummyPlayerName { get; set; } = "[THE RIPTIDE]";
    }

    public class GlobalReferenceConfig
    {
        [Description("[AUTO GENERATED FILE] may contain types which no longer work. A reference list of types to be used in other configs (do not edit)")]
        public List<ItemType> AllItems { get; set; } = new List<ItemType>();
        public List<string> AllEffects { get; set; } = new List<string>();
        public List<AttachmentName> AllAttachments { get; set; } = new List<AttachmentName>();
    }

    public class Deathmatch
    {
        public static Deathmatch Singleton { get; private set; }

        [PluginConfig("main_config.yml")]
        public MainConfig config;

        [PluginConfig("global_reference_config.yml")]
        public GlobalReferenceConfig global_reference_config;

        [PluginConfig("rooms_config.yml")]
        public RoomsConfig rooms_config;

        [PluginConfig("killstreak_config.yml")]
        public KillstreakConfig killstreak_config;

        [PluginConfig("loadout_config.yml")]
        public LoadoutConfig loadout_config;

        [PluginConfig("lobby_config.yml")]
        public LobbyConfig lobby_config;

        [PluginConfig("experience_config.yml")]
        public ExperienceConfig experience_config;

        [PluginConfig("rank_config.yml")]
        public RankConfig rank_config;

        [PluginConfig("tracking_config.yml")]
        public TrackingConfig tracking_config;

        [PluginConfig("translation_config.yml")]
        public TranslationConfig translation_config;

        [PluginConfig("attachment_blacklist_config.yml")]
        public AttachmentBlacklistConfig attachment_blacklist_config;

        [PluginConfig("voice_chat_config.yml")]
        public VoiceChatConfig voice_chat_config;

        [PluginConfig("cleanup_config.yml")]
        public CleanupConfig cleanup_config;

        [PluginConfig("leader_board.yml")]
        public LeaderBoardConfig leader_board_config;

        private static bool game_started = false;
        public static bool game_ended = false;
        public static SortedSet<int> players = new SortedSet<int>();
        private Action OnConfigReloaded;
        private CoroutineHandle restart_handle;
        private CoroutineHandle round_timer_handle;


        public static bool GameStarted
        {
            get => game_started;
            set
            {
                if (value == true)
                {
                    foreach (var player in Player.GetPlayers())
                        if (player.IsAlive)
                            Killstreaks.Singleton.AddKillstreakStartEffects(player);
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

            OnConfigReloaded = new Action(() =>
            {
                try
                {
                    ServerConsole.FriendlyFire = true;
                    FriendlyFireConfig.PauseDetector = true;
                    ServerConsole.HeavilyModdedServerConfig = true;
                    ServerConsole.CustomGamemodeServerConfig = true;
                }
                catch(Exception ex)
                {
                    Log.Error("config override error: " + ex.ToString());
                }
            });
        }

        public void Start()
        {
            Database.Singleton.Load(PluginHandler.Get(this).MainConfigPath);

            EventManager.RegisterEvents(this);
            //dependencies
            EventManager.RegisterEvents<InventoryMenu>(this);
            EventManager.RegisterEvents<BroadcastOverride>(this);
            EventManager.RegisterEvents<FacilityManager>(this);
            EventManager.RegisterEvents<BadgeOverride>(this);
            EventManager.RegisterEvents<HintOverride>(this);
            BadgeOverride.Singleton.Init(2);

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
            if (attachment_blacklist_config.IsEnabled)
                EventManager.RegisterEvents<AttachmentBlacklist>(this);
            if (voice_chat_config.IsEnabled)
                EventManager.RegisterEvents<VoiceChat>(this);
            if (cleanup_config.IsEnabled)
                EventManager.RegisterEvents<Cleanup>(this);
            if (leader_board_config.IsEnabled)
                EventManager.RegisterEvents<LeaderBoard>(this);


            Statistics.Init();
            Rooms.Singleton.Init(rooms_config);
            Killstreaks.Singleton.Init(killstreak_config);
            Loadouts.Singleton.Init(loadout_config);
            Lobby.Singleton.Init(lobby_config);
            if (rank_config.IsEnabled)
                Ranks.Singleton.Init(rank_config);
            if (experience_config.IsEnabled)
                Experiences.Singleton.Init(experience_config);
            if (tracking_config.IsEnabled)
                Tracking.Singleton.Init(tracking_config);
            if (attachment_blacklist_config.IsEnabled)
                AttachmentBlacklist.Singleton.Init(attachment_blacklist_config);
            if (voice_chat_config.IsEnabled)
                VoiceChat.Singleton.Init(voice_chat_config);
            if (cleanup_config.IsEnabled)
                Cleanup.Singleton.Init(cleanup_config);
            if (leader_board_config.IsEnabled)
                LeaderBoard.Singleton.Init(leader_board_config);

            translation = translation_config;
            DeathmatchMenu.Singleton.SetupMenus();

            SubscribeOnConfigReloaded(OnConfigReloaded);
        }

        public void Stop()
        {
            Database.Singleton.UnLoad();

            //features
            EventManager.UnregisterEvents<LeaderBoard>(this);
            EventManager.UnregisterEvents<Cleanup>(this);
            EventManager.UnregisterEvents<VoiceChat>(this);
            EventManager.UnregisterEvents<AttachmentBlacklist>(this);
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
            EventManager.UnregisterEvents<HintOverride>(this);
            EventManager.UnregisterEvents<BadgeOverride>(this);
            EventManager.UnregisterEvents<FacilityManager>(this);
            EventManager.UnregisterEvents<BroadcastOverride>(this);
            EventManager.UnregisterEvents<InventoryMenu>(this);

            EventManager.UnregisterEvents(this);

            DeathmatchMenu.Singleton.ClearMenus();

            UnsubscribeOnConfigReloaded(OnConfigReloaded);
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
            game_ended = false;
            GenerateGlobalReferenceConfig();
            Database.Singleton.Checkpoint();
        }

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            if (config.RoundTime > 5.0f)
                Timing.CallDelayed(60.0f * (config.RoundTime - 5.0f), () => { BroadcastOverride.BroadcastLine(1, 30, BroadcastPriority.Medium, "<color=#43BFF0>Round Ends in 5 minutes</color>"); });
            if (config.RoundTime > 1.0f)
                Timing.CallDelayed(60.0f * (config.RoundTime - 1.0f), () => { BroadcastOverride.BroadcastLine(1, 30, BroadcastPriority.Medium, "<color=#43BFF0>Round Ends in 1 minute</color>"); });
            round_timer_handle = Timing.CallDelayed(60.0f * config.RoundTime, () => 
            {
                try
                {
                    restart_handle = Timing.CallDelayed(config.RoundEndTime, () => Round.Restart(false));
                    Timing.CallPeriodically(config.RoundEndTime, 0.2f, () =>
                    {
                        foreach (var p in Player.GetPlayers())
                            p.IsGodModeEnabled = true;
                    });
                    try { Statistics.DisplayRoundStats(); }
                    catch(Exception ex) { Log.Error(ex.ToString()); }
                    try { Experiences.Singleton.SaveExperiences(); }
                    catch (Exception ex) { Log.Error(ex.ToString()); }
                    try { Ranks.Singleton.CalculateAndSaveRanks(); }
                    catch (Exception ex) { Log.Error(ex.ToString()); }
                    HintOverride.Refresh();
                    VoiceChat.Singleton.ForceGlobalTalkGlobalReceive();
                    Server.Instance.SetRole(RoleTypeId.Spectator);
                    game_ended = true;
                    Tracking.Singleton.UpdateLeaderBoard();
                    LeaderBoard.Singleton.ReloadLeaderBoard();
                    if (leader_board_config.DisplayEndRoundDelay < config.RoundEndTime)
                    {
                        LeaderBoard.Singleton.EnableTitle = false;
                        Timing.CallDelayed(leader_board_config.DisplayEndRoundDelay,()=>
                        {
                            foreach (var p in Player.GetPlayers())
                                LeaderBoard.Singleton.EnableLeaderBoardMode(p, Enum.IsDefined(typeof(LeaderBoardType), leader_board_config.LeaderBoardType) ? (LeaderBoardType)leader_board_config.LeaderBoardType : (LeaderBoardType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(LeaderBoardType)).Length));
                        });
                    }
                }
                catch(Exception ex)
                {
                    Log.Error("round end Error: " + ex.ToString());
                }
            });

            Server.Instance.SetRole(RoleTypeId.Scp939);
            Server.Instance.ReferenceHub.nicknameSync.SetNick(config.DummyPlayerName);
            Server.Instance.Position = new Vector3(128.8f, 994.0f, 18.0f);
            Round.IsLocked = true;
            Warhead.IsLocked = true;
            DecontaminationController.Singleton.NetworkDecontaminationOverride = DecontaminationController.DecontaminationStatus.Disabled;
            AttackerDamageHandler._ffMultiplier = 1.0f;
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            players.Add(player.PlayerId);
            if (!player.DoNotTrack)
                Database.Singleton.LoadConfig(player);
            else
                Timing.CallDelayed(1.0f, () => { HintOverride.Add(player, 0, translation.DntMsg, 30.0f); HintOverride.Refresh(player); });
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
            Timing.KillCoroutines(round_timer_handle);
            Timing.KillCoroutines(restart_handle);
        }

        public static bool IsPlayerValid(Player player)
        {
            return players.Contains(player.PlayerId);
        }

        private void GenerateGlobalReferenceConfig()
        {
            global_reference_config.AllItems.Clear();
            foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
                global_reference_config.AllItems.Add(item);

            global_reference_config.AllEffects.Clear();
            foreach (StatusEffectBase effect in Server.Instance.GameObject.GetComponentsInChildren<StatusEffectBase>(true))
                global_reference_config.AllEffects.Add(effect.name);
            global_reference_config.AllAttachments.Clear();
            foreach (AttachmentName name in Enum.GetValues(typeof(AttachmentName)))
                global_reference_config.AllAttachments.Add(name);
            PluginHandler.Get(this).SaveConfig(this, nameof(global_reference_config));
        }
    }
}
