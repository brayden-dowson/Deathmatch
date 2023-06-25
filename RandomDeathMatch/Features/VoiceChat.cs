using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PlayerRoles.FirstPersonControl;
using System.Collections.Generic;
using HarmonyLib;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using System.Reflection.Emit;
using NorthwoodLib.Pools;
using VoiceChat.Networking;
using Mirror;
using VoiceChat;
using PlayerRoles.Voice;
using static TheRiptide.Translation;
using System.Linq;

namespace TheRiptide
{
    public class VoiceChatConfig
    {
        public bool IsEnabled { get; set; } = true;
    }

    public class VoiceChat
    {
        public static VoiceChat Singleton { get; private set; }
        public VoiceChatConfig config;

        public enum TalkMode { GlobalTalkGlobalReceive, ProximityTalkGlobalReceive, ProximityTalkProximityReceive };
        private Dictionary<int, TalkMode> player_mode = new Dictionary<int, TalkMode>();
        private bool force_mode = false;
        private Harmony harmony;

        public VoiceChat()
        {
            Singleton = this;
            harmony = new Harmony("the_riptide.voice_chat");
            harmony.PatchAll();
        }

        public void Init(VoiceChatConfig config)
        {
            this.config = config;
        }

        public void WaitingForPlayers()
        {
            force_mode = false;
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            if (!player_mode.ContainsKey(player.PlayerId))
                player_mode.Add(player.PlayerId, TalkMode.GlobalTalkGlobalReceive);
            else
                player_mode[player.PlayerId] = TalkMode.GlobalTalkGlobalReceive;
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void onPlayerLeft(Player player)
        {
            if (player_mode.ContainsKey(player.PlayerId))
                player_mode.Remove(player.PlayerId);
        }

        public void ForceGlobalTalkGlobalReceive()
        {
            force_mode = true;
            foreach (var id in player_mode.Keys.ToList())
                player_mode[id] = TalkMode.GlobalTalkGlobalReceive;
        }

        private bool IsGlobalTalk(int id)
        {
            if (player_mode.ContainsKey(id))
                return player_mode[id] == TalkMode.GlobalTalkGlobalReceive ? true : false;
            return false;
        }

        private bool IsGlobalReceive(int id)
        {
            if (player_mode.ContainsKey(id))
                return player_mode[id] == TalkMode.ProximityTalkProximityReceive ? false : true;
            return false;
        }

        [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
        public class VoiceTransceiverPatch
        {
            static bool Prefix(NetworkConnection conn, VoiceMessage msg)
            {
                if (msg.SpeakerNull || (int)msg.Speaker.netId != (int)conn.identity.netId || !(msg.Speaker.roleManager.CurrentRole is IVoiceRole currentRole1) || !currentRole1.VoiceModule.CheckRateLimit() || VoiceChatMutes.IsMuted(msg.Speaker))
                    return false;

                bool global_talk = Singleton.IsGlobalTalk(msg.Speaker.PlayerId);
                bool dead_talk = !msg.Speaker.IsAlive();
                if (!global_talk && dead_talk)
                    return false;

                foreach (ReferenceHub allHub in ReferenceHub.AllHubs)
                {
                    if (msg.Speaker != allHub && allHub.roleManager.CurrentRole is IVoiceRole currentRole2 && Singleton.player_mode.ContainsKey(allHub.PlayerId))
                    {
                        bool global_receive = Singleton.IsGlobalReceive(allHub.PlayerId);
                        bool dead_receive = !allHub.IsAlive();
                        if (global_talk && global_receive)
                        {
                            msg.Channel = VoiceChatChannel.RoundSummary;
                            allHub.connectionToClient.Send(msg);
                        }
                        else if (!dead_talk && !dead_receive && currentRole2.VoiceModule is HumanVoiceModule hvm && hvm.CheckProximity(msg.Speaker))
                        {
                            msg.Channel = VoiceChatChannel.Proximity;
                            allHub.connectionToClient.Send(msg);
                        }
                    }
                }
                return false;
            }
        }

        //ripped from https://github.com/Jesus-QC/ScpChatExtension/blob/master/ScpChatExtension/Patches/NoClipTogglePatch.cs
        public static bool OnPlayerTogglingNoClip(ReferenceHub player)
        {
            try
            {
                if (FpcNoclip.IsPermitted(player))
                    return true;

                if (!Singleton.force_mode)
                {
                    int id = player.PlayerId;
                    if (Singleton.player_mode.ContainsKey(id))
                    {
                        switch (Singleton.player_mode[id])
                        {
                            case TalkMode.GlobalTalkGlobalReceive:
                                Singleton.player_mode[id] = TalkMode.ProximityTalkGlobalReceive;
                                BroadcastOverride.BroadcastLine(Player.Get(player), 1, 5, BroadcastPriority.Low, translation.ProximityTalkGlobalReceive);
                                break;
                            case TalkMode.ProximityTalkGlobalReceive:
                                Singleton.player_mode[id] = TalkMode.ProximityTalkProximityReceive;
                                BroadcastOverride.BroadcastLine(Player.Get(player), 1, 5, BroadcastPriority.Low, translation.ProximityTalkProximityReceive);
                                break;
                            case TalkMode.ProximityTalkProximityReceive:
                                Singleton.player_mode[id] = TalkMode.GlobalTalkGlobalReceive;
                                BroadcastOverride.BroadcastLine(Player.Get(player), 1, 5, BroadcastPriority.Low, translation.GlobalTalkGlobalReceive);
                                break;
                        }
                        BroadcastOverride.UpdateIfDirty(Player.Get(player));
                    }
                }
            }
            catch(System.Exception ex)
            {
                Log.Error("noclip error: " + ex.ToString());
            }
            return false;
        }

        [HarmonyPatch(typeof(FpcNoclipToggleMessage), nameof(FpcNoclipToggleMessage.ProcessMessage))]
        public class NoClipTogglePatch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

                Label ret = generator.DefineLabel();

                newInstructions[newInstructions.Count - 1].labels.Add(ret);

                int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Ret) + 1;

                newInstructions.InsertRange(index, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldloc_0).MoveLabelsFrom(newInstructions[index]),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VoiceChat), nameof(VoiceChat.OnPlayerTogglingNoClip))),
                    new CodeInstruction(OpCodes.Brfalse, ret),
                });

                foreach (CodeInstruction instruction in newInstructions)
                    yield return instruction;

                ListPool<CodeInstruction>.Shared.Return(newInstructions);
            }
        }
    }
}
