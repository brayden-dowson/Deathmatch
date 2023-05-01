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
        public int Kill { get; set; } = 50;
        //public int Minute { get; set; } = 20;
        //public int OnehundredDamage { get; set; } = 25;
    }

    public class Experiences
    {
        public static Experiences Singleton { get; private set; }
        private ExperienceConfig config;

        public class XP
        {
            public int value { get; set; } = 0;
            public int stage { get; set; } = 0;
            public int tier { get; set; } = 0;
        }

        private Dictionary<int, XP> player_xp = new Dictionary<int, XP>();

        public Experiences()
        {
            Singleton = this;
            config = Deathmatch.Singleton.experience_config;
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
