using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using VoiceChat.Networking;
using Mirror;
using PluginAPI.Core;
using VoiceChat;
using PlayerRoles.Voice;
using UnityEngine;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps;
using PluginAPI.Events;
using PluginAPI.Enums;
using PluginAPI.Core.Attributes;
using PlayerRoles;

namespace TheRiptide
{
    //[HarmonyPatch(typeof(ReferenceHub))]
    //public class ReferenceHubPatch
    //{
    //    [HarmonyPatch(nameof(ReferenceHub.AllHubs), MethodType.Getter)]
    //    static void Postfix(ref HashSet<ReferenceHub> __result)
    //    {
    //        __result.ExceptWith(VoiceChatOverride.Singleton.player_proximity.Values);
    //    }
    //}

    class VoiceChatOverride
    {
        public static VoiceChatOverride Singleton { get; private set; }

        private Dictionary<int, Func<VoiceChatChannel, VoiceChatChannel>> player_send_validators = new Dictionary<int, Func<VoiceChatChannel, VoiceChatChannel>>();
        private Dictionary<int, Func<ReferenceHub, VoiceChatChannel, VoiceChatChannel>> player_receive_validators = new Dictionary<int, Func<ReferenceHub, VoiceChatChannel, VoiceChatChannel>>();
        private Dictionary<int, HashSet<int>> player_routing = new Dictionary<int, HashSet<int>>();
        private Dictionary<int, ReferenceHub> player_proximity = new Dictionary<int, ReferenceHub>();

        private Harmony harmony;

        public VoiceChatOverride()
        {
            Singleton = this;
            harmony = new Harmony("the_riptide.voice_chat_override");
            harmony.PatchAll();
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            if (player_proximity.ContainsKey(player.PlayerId))
            {
                NetworkServer.RemovePlayerForConnection(player_proximity[player.PlayerId].netIdentity.connectionToClient, true);
                player_proximity.Remove(player.PlayerId);
            }
        }

        [PluginEvent(ServerEventType.PlayerChangeRole)]
        void OnPlayerChangeRole(Player player, PlayerRoleBase old_role, RoleTypeId new_role, RoleChangeReason reason)
        {
            if (player_proximity.ContainsKey(player.PlayerId))
            {
                NetworkServer.RemovePlayerForConnection(player_proximity[player.PlayerId].netIdentity.connectionToClient, true);
                player_proximity.Remove(player.PlayerId);
            }
        }

        public void SetSendValidator(Player player, Func<VoiceChatChannel, VoiceChatChannel> validate_send)
        {
            if (!player_send_validators.ContainsKey(player.PlayerId))
                player_send_validators.Add(player.PlayerId, validate_send);
            else
                player_send_validators[player.PlayerId] = validate_send;
        }

        public void ResetSendValidator(Player player)
        {
            player_send_validators.Remove(player.PlayerId);
        }

        public void SetReceiveValidator(Player player, Func<ReferenceHub, VoiceChatChannel, VoiceChatChannel> validate_receive)
        {
            if (!player_receive_validators.ContainsKey(player.PlayerId))
                player_receive_validators.Add(player.PlayerId, validate_receive);
            else
                player_receive_validators[player.PlayerId] = validate_receive;
        }

        public void ResetReceiveValidator(Player player)
        {
            player_receive_validators.Remove(player.PlayerId);
        }

        public void SetRouting(Player sender, List<Player> receivers)
        {
            if (!player_routing.ContainsKey(sender.PlayerId))
                player_routing.Add(sender.PlayerId, receivers.ConvertAll((p) => p.PlayerId).ToHashSet());
            else
                player_routing[sender.PlayerId] = receivers.ConvertAll((p) => p.PlayerId).ToHashSet();
        }

        public void ResetRouting(Player sender)
        {
            player_routing.Remove(sender.PlayerId);
        }

        public ReferenceHub GetFakePlayerForProximity(ReferenceHub player)
        {
            try
            {
                if (!player_proximity.ContainsKey(player.PlayerId))
                {
                    GameObject fake_player = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab, player.transform);
                    fake_player.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
                    FakeConnection connection = new FakeConnection(Enumerable.Range(0, int.MaxValue).Except(NetworkServer.connections.Keys).FirstOrDefault());
                    NetworkServer.AddPlayerForConnection(connection, fake_player);
                    ReferenceHub hub = fake_player.GetComponent<ReferenceHub>();
                    hub.roleManager.ServerSetRole(PlayerRoles.RoleTypeId.Tutorial, PlayerRoles.RoleChangeReason.RemoteAdmin, PlayerRoles.RoleSpawnFlags.None);
                    hub.nicknameSync.Network_myNickSync = player.nicknameSync.Network_myNickSync + " proximity";
                    player_proximity.Add(player.PlayerId, hub);
                    return hub;
                }
                else
                    return player_proximity[player.PlayerId];
            }
            catch(Exception ex)
            {
                Log.Error("audio error: " + ex.ToString());
            }
            return new ReferenceHub();
        }

        [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
        public class VoiceTransceiverPatch
        {
            static bool Prefix(NetworkConnection conn, VoiceMessage msg)
            {
                if (msg.SpeakerNull || (int)msg.Speaker.netId != (int)conn.identity.netId || !(msg.Speaker.roleManager.CurrentRole is IVoiceRole currentRole1) || !currentRole1.VoiceModule.CheckRateLimit() || VoiceChatMutes.IsMuted(msg.Speaker))
                    return true;

                VoiceChatChannel channel = Singleton.player_send_validators.ContainsKey(msg.Speaker.PlayerId) ?
                    Singleton.player_send_validators[msg.Speaker.PlayerId](msg.Channel) :
                    currentRole1.VoiceModule.ValidateSend(msg.Channel);

                if (channel == VoiceChatChannel.None)
                    return true;

                if (channel == VoiceChatChannel.Proximity && currentRole1.VoiceModule is StandardScpVoiceModule)
                    msg.Speaker = Singleton.GetFakePlayerForProximity(msg.Speaker);

                currentRole1.VoiceModule.CurrentChannel = channel;
                foreach (ReferenceHub allHub in ReferenceHub.AllHubs)
                {
                    if (!Singleton.player_routing.ContainsKey(msg.Speaker.PlayerId) ||
                        Singleton.player_routing[msg.Speaker.PlayerId].Contains(allHub.PlayerId))
                    {
                        if (allHub.roleManager.CurrentRole is IVoiceRole currentRole2)
                        {
                            VoiceChatChannel voiceChatChannel = Singleton.player_receive_validators.ContainsKey(msg.Speaker.PlayerId) ?
                                Singleton.player_receive_validators[msg.Speaker.PlayerId](msg.Speaker, channel) :
                                currentRole2.VoiceModule.ValidateReceive(msg.Speaker, channel);
                            if (voiceChatChannel != VoiceChatChannel.None)
                            {
                                msg.Channel = voiceChatChannel;
                                allHub.connectionToClient.Send(msg);
                            }
                        }
                    }
                }
                return true;
            }
        }

        public class FakeConnection : NetworkConnectionToClient
        {
            public FakeConnection(int connectionId) : base(connectionId)
            {
            }

            public override string address
            {
                get
                {
                    return "localhost";
                }
            }

            public override void Send(ArraySegment<byte> segment, int channelId = 0)
            {
            }
            public override void Disconnect()
            {
            }
        }
    }
}
