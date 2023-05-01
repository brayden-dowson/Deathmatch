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
    class BadgeOverride
    {
        public static BadgeOverride Singleton { get; private set; }

        public class Badge
        {
            public List<string> badges = new List<string>();

            public string BadgeText()
            {
                string text = "";
                foreach (var badge in badges)
                    text += badge;
                return text;
            }
        }

        private int slots = 0;
        private Dictionary<int, Badge> player_badges = new Dictionary<int, Badge>();

        public BadgeOverride()
        {
            Singleton = this;
        }

        public void Init(int slots)
        {
            this.slots = slots;
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            int id = player.PlayerId;
            Badge badge = new Badge();
            for (int i = 0; i < slots; i++)
                badge.badges.Add("");
            if (!player_badges.ContainsKey(id))
                player_badges.Add(id, badge);
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            int id = player.PlayerId;
            if (player_badges.ContainsKey(id))
                player_badges.Remove(id);
        }

        public void SetBadge(Player player, int slot, string badge)
        {
            player_badges[player.PlayerId].badges[slot] = badge;
            player.ReferenceHub.serverRoles.Group.BadgeText = player_badges[player.PlayerId].BadgeText();
        }

    }
}
