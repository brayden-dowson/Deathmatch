using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glicko2;

namespace TheRiptide
{
    public class Ranks
    {
        public static Ranks Singleton { get; private set; }

        public class Rank
        {
            public bool loaded = false;
            public float rating = 1500.0f;
            public float rd = 350.0f;
            public float rv = 0.06f;
        }

        private Dictionary<int, Rank> player_ranks = new Dictionary<int, Rank>();
        private Dictionary<int, GlickoPlayer> player_glikco = new Dictionary<int, GlickoPlayer>();
        private Dictionary<int, List<GlickoOpponent>> player_results = new Dictionary<int, List<GlickoOpponent>>();

        public Ranks()
        {
            Singleton = this;
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            int id = player.PlayerId;
            if (!player_ranks.ContainsKey(id))
                player_ranks.Add(id, new Rank());
            DataBase.Singleton.LoadRank(player);
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            int id = player.PlayerId;
            if (player_ranks.ContainsKey(id))
                player_ranks.Remove(id);
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDeath(Player target, Player killer, DamageHandlerBase damage)
        {
            if(killer != null && target != killer && player_ranks[target.PlayerId].loaded && player_ranks[killer.PlayerId].loaded)
            {
                Rank target_rank = player_ranks[target.PlayerId];
                if (!player_glikco.ContainsKey(target.PlayerId))
                {
                    player_glikco.Add(target.PlayerId, new GlickoPlayer(target_rank.rating, target_rank.rd, target_rank.rv));
                    player_glikco[target.PlayerId].Name = target.UserId;
                    player_results.Add(target.PlayerId, new List<GlickoOpponent>());
                }
                Rank killer_rank = player_ranks[killer.PlayerId];
                if (!player_glikco.ContainsKey(killer.PlayerId))
                {
                    player_glikco.Add(killer.PlayerId, new GlickoPlayer(killer_rank.rating, killer_rank.rd, killer_rank.rv));
                    player_glikco[killer.PlayerId].Name = killer.UserId;
                    player_results.Add(killer.PlayerId, new List<GlickoOpponent>());
                }

                player_results[target.PlayerId].Add(new GlickoOpponent(player_glikco[killer.PlayerId], 0));
                player_results[killer.PlayerId].Add(new GlickoOpponent(player_glikco[target.PlayerId], 1));
            }
        }

        public Rank GetRank(Player player)
        {
            return player_ranks[player.PlayerId];
        }

        public void CalculateAndSaveRanks()
        {
            foreach(var id in player_glikco.Keys)
            {
                GlickoPlayer new_rank = GlickoCalculator.CalculateRanking(player_glikco[id], player_results[id]);
                DataBase.Singleton.SaveRank(player_glikco[id].Name, (float)new_rank.Rating, (float)new_rank.RatingDeviation, (float)new_rank.Volatility);
            }
            player_glikco.Clear();
            player_results.Clear();
        }
    }
}
