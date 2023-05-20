using MEC;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerRoles.Spectating;

namespace TheRiptide
{
    public class VoiceChatConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool IsSpectatorChatGlobal { get; set; } = false;
    }

    class VoiceChat
    {
        public static VoiceChat Singleton { get; private set; }
        VoiceChatConfig config;
        public VoiceChat()
        {
            Singleton = this;
        }
        public void Init(VoiceChatConfig config)
        {
            this.config = config;
        }
        [PluginEvent(ServerEventType.PlayerSpawn)]
        void OnPlayerSpawn(Player player, RoleTypeId role)
        {
            if (role.GetTeam() != Team.Dead)
            {
                VoiceChatOverride.Singleton.SetSendValidator(player, (channel) =>
                {
                    return global::VoiceChat.VoiceChatChannel.RoundSummary;
                });
            }
            else
            {
                if (config.IsSpectatorChatGlobal)
                {
                    VoiceChatOverride.Singleton.SetSendValidator(player, (channel) =>
                    {
                        return global::VoiceChat.VoiceChatChannel.RoundSummary;
                    });
                }
                else
                    VoiceChatOverride.Singleton.ResetSendValidator(player);
            }
        }
    }
}
