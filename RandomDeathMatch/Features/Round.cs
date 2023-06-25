using LightContainmentZoneDecontamination;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static TheRiptide.Translation;

namespace TheRiptide
{
    public class DmRound
    {
        public static DmRound Singleton { get; private set; }

        private MainConfig config;

        private static bool game_started = false;
        public static bool game_ended = false;
        public static SortedSet<int> players = new SortedSet<int>();
        public Action OnConfigReloaded;
        private CoroutineHandle restart_handle;
        private CoroutineHandle round_timer_handle;
        private CoroutineHandle round_5_minute_warning;
        private CoroutineHandle round_1_minute_warning;

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

        public DmRound()
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
                catch (Exception ex)
                {
                    Log.Error("config override error: " + ex.ToString());
                }
            });
        }

        public void Init(MainConfig config)
        {
            this.config = config;
        }

        public void WaitingForPlayers()
        {
            game_ended = false;
            Database.Singleton.Checkpoint();
        }

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            if (config.RoundTime > 5.0f)
                round_5_minute_warning = Timing.CallDelayed(60.0f * (config.RoundTime - 5.0f), () => { BroadcastOverride.BroadcastLine(1, 30, BroadcastPriority.Medium, "<color=#43BFF0>Round Ends in 5 minutes</color>"); });
            if (config.RoundTime > 1.0f)
                round_1_minute_warning = Timing.CallDelayed(60.0f * (config.RoundTime - 1.0f), () => { BroadcastOverride.BroadcastLine(1, 30, BroadcastPriority.Medium, "<color=#43BFF0>Round Ends in 1 minute</color>"); });
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
                    catch (Exception ex) { Log.Error(ex.ToString()); }
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
                    if (Deathmatch.Singleton.leader_board_config.DisplayEndRoundDelay < config.RoundEndTime)
                    {
                        LeaderBoard.Singleton.EnableTitle = false;
                        Timing.CallDelayed(Deathmatch.Singleton.leader_board_config.DisplayEndRoundDelay, () =>
                        {
                            foreach (var p in Player.GetPlayers())
                                LeaderBoard.Singleton.EnableLeaderBoardMode(p, Enum.IsDefined(typeof(LeaderBoardType), Deathmatch.Singleton.leader_board_config.LeaderBoardType) ? (LeaderBoardType)Deathmatch.Singleton.leader_board_config.LeaderBoardType : (LeaderBoardType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(LeaderBoardType)).Length));
                        });
                    }
                }
                catch (Exception ex)
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

        public void RoundRestart()
        {
            Timing.KillCoroutines(round_timer_handle, restart_handle, round_5_minute_warning, round_1_minute_warning);
            Round.IsLocked = false;
        }

        public void RoundEnd()
        {
            Timing.KillCoroutines(round_timer_handle, restart_handle, round_5_minute_warning, round_1_minute_warning);
            Round.IsLocked = false;
        }
    }
}
