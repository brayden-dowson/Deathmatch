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
using static RoundSummary;

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

        public Deathmatch()
        {
            Singleton = this;
        }

        public void Start()
        {
            Database.Singleton.Load(PluginHandler.Get(this).MainConfigPath);

            if (config.IsEnabled)
                EventManager.RegisterEvents(this);
            EventManager.RegisterEvents<DmRound>(this);
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

            DmRound.Singleton.Init(config);
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

            GameCore.ConfigFile.OnConfigReloaded += DmRound.Singleton.OnConfigReloaded;
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
            EventManager.UnregisterEvents<DmRound>(this);

            DeathmatchMenu.Singleton.ClearMenus();

            GameCore.ConfigFile.OnConfigReloaded -= DmRound.Singleton.OnConfigReloaded;
        }

        [PluginEntryPoint("Deathmatch", "1.0", "needs no explanation", "The Riptide")]
        void EntryPoint()
        {
            if (!config.IsEnabled)
                return;
            Start();
        }

        [PluginUnload]
        void Unload()
        {
            Stop();
        }

        [PluginEvent(ServerEventType.MapGenerated)]
        public void OnMapGenerated()
        {
            FacilityManager.MapGenerated();
            Lobby.Singleton.MapGenerated();
            if (rank_config.IsEnabled)
                Ranks.Singleton.MapGenerated();
            if (leader_board_config.IsEnabled)
                LeaderBoard.Singleton.MapGenerated();
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        public void OnWaitingForPlayers()
        {
            GenerateGlobalReferenceConfig();
            DmRound.Singleton.WaitingForPlayers();
            if (tracking_config.IsEnabled)
                Tracking.Singleton.WaitingForPlayers();
            if (voice_chat_config.IsEnabled)
                VoiceChat.Singleton.WaitingForPlayers();
        }

        [PluginEvent(ServerEventType.RoundEnd)]
        public void OnRoundEnd(LeadingTeam team)
        {
            DmRound.Singleton.RoundEnd();
        }

        [PluginEvent(ServerEventType.RoundRestart)]
        public void OnRoundRestart()
        {
            DmRound.Singleton.RoundRestart();
            FacilityManager.RoundRestart();
            Rooms.Singleton.RoundRestart();
            if (cleanup_config.IsEnabled)
                Cleanup.Singleton.RoundRestart();
        }

        public static bool IsPlayerValid(Player player)
        {
            return DmRound.players.Contains(player.PlayerId);
        }

        private void GenerateGlobalReferenceConfig()
        {
            try
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
            catch(Exception e)
            {
                Log.Error("Global reference config error delete config if this error is common\n " + e.ToString(), "NW API ERROR");
            }
        }
    }
}
