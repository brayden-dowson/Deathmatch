using MEC;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheRiptide
{
    public class VoiceChatConfig
    {
        public bool IsEnabled { get; set; } = true;
        public global::VoiceChat.VoiceChatChannel AliveOverrideChannel { get; set; } = global::VoiceChat.VoiceChatChannel.Spectator;
        public global::VoiceChat.VoiceChatChannel DeadOverrideChannel { get; set; } = global::VoiceChat.VoiceChatChannel.Spectator;
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
                Timing.CallDelayed(1.0f, () =>
                {
                    player.VoiceModule.CurrentChannel = config.AliveOverrideChannel;
                });
            }
            else
            {
                Timing.CallDelayed(1.0f, () =>
                {
                    player.VoiceModule.CurrentChannel = config.DeadOverrideChannel;
                });
            }
        }
    }
}
