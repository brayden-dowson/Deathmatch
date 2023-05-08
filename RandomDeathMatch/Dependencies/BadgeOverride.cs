using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using System.Collections.Generic;

namespace TheRiptide
{
    class BadgeOverride
    {
        public static readonly Dictionary<string, string> ColorNameToHex = new Dictionary<string, string>
        {
            {"pink", "#FF96DE"},
            {"red", "#C50000"},
            {"brown", "#944710"},
            {"silver", "#A0A0A0"},
            {"light_green", "#32CD32"},
            {"crimson", "#DC143C"},
            {"cyan", "#00B7EB"},
            {"aqua", "#00FFFF"},
            {"deep_pink", "#FF1493"},
            {"tomato", "#FF6448"},
            {"yellow", "#FAFF86"},
            {"magenta", "#FF0090"},
            {"blue_green", "#4DFFB8"},
            {"orange", "#FF9966"},
            {"lime", "#BFFF00"},
            {"green", "#228B22"},
            {"emerald", "#50C878"},
            {"carmine", "#960018"},
            {"nickel", "#727472"},
            {"mint", "#98FB98"},
            {"army_green", "#4B5320"},
            {"pumpkin", "#EE7600"},
            {"gold", "#EFC01A"},
            {"teal", "#008080"},
            {"blue", "#005EBC"},
            {"purple", "#8137CE"},
            {"light_red", "#FD8272"},
            {"silver_blue", "#666699"},
            {"police_blue", "#002DB3"}

        };
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
            player.ReferenceHub.serverRoles.Network_myText = player_badges[player.PlayerId].BadgeText();
        }

        public void SetBadgeColor(Player player, string color)
        {
            player.ReferenceHub.serverRoles.Network_myColor = color;
        }

    }
}
