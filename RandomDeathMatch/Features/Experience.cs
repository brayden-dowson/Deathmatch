using PlayerStatsSystem;
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
    public class ExperienceConfig
    {
        public bool IsEnabled { get; set; } = false;

        public int Kill { get; set; } = 50;
        //public int Minute { get; set; } = 20;
        //public int OnehundredDamage { get; set; } = 25;

        public int XpPerLevel { get; set; } = 250;
        public float LevelExponent { get; set; } = 1.0f;
        public float StageExponent { get; set; } = 1.5f;
        public float TierExponent { get; set; } = 1.0f;

        public string BadgeFormat { get; set; } = "[{0} {1} {2} {3}]";
        public string XpToNextLevelFormat { get; set; } = "XP:{}";
        public List<string> LevelTags { get; set; } = new List<string>
        {
            "L:1",
            "L:2",
            "L:3",
            "L:4",
            "L:5",
            "L:6",
            "L:7",
            "L:8",
            "L:9",
            "L:10",
            "L:11",
            "L:12",
            "L:13",
            "L:14",
            "L:15",
            "L:16",
            "L:17",
            "L:18",
            "L:19",
            "L:20",
            "L:21",
            "L:22",
            "L:23",
            "L:24",
            "L:25",
        };
        public List<string> StageTags { get; set; } = new List<string>
        {
            "S:1",
            "S:2",
            "S:3",
            "S:4",
            "S:5",
            "S:6",
            "S:7",
        };
        public List<string> TierTags { get; set; } = new List<string>
        {
            "T:1",
            "T:2",
            "T:3",
            "T:4",
            "T:5",
            "T:6",
            "T:7",
        };
    }

    public class Experiences
    {
        public static Experiences Singleton { get; private set; }
        private ExperienceConfig config;

        public class XP
        {
            public int value { get; set; } = 0;
            public int level { get; set; } = 0;
            public int stage { get; set; } = 0;
            public int tier { get; set; } = 0;
        }

        private Dictionary<int, XP> player_xp = new Dictionary<int, XP>();

        public Experiences()
        {
            Singleton = this;
        }

        public void Init(ExperienceConfig config)
        {
            this.config = config;
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            int id = player.PlayerId;
            if (!player_xp.ContainsKey(id))
                player_xp.Add(id, new XP());
            DataBase.Singleton.LoadExperience(player);
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            int id = player.PlayerId;
            if (player_xp.ContainsKey(id))
                player_xp.Remove(id);
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDeath(Player target, Player killer, DamageHandlerBase damage)
        {
            if(target != null && killer != null && target != killer)
                player_xp[killer.PlayerId].value += config.Kill;
        }

        public XP GetXP(Player player)
        {
            return player_xp[player.PlayerId];
        }

        public void SaveExperiences()
        {
            foreach (Player p in Player.GetPlayers())
                if (player_xp.ContainsKey(p.PlayerId))
                    DataBase.Singleton.SaveExperience(p);
        }
    }
}
