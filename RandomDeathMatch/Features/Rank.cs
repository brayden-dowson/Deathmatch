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
using CommandSystem;
using System.ComponentModel;

namespace TheRiptide
{
    public class RankConfig
    {
        public bool IsEnabled { get; set; } = true;

        public string BadgeFormat { get; set; } = "Rank: {0}\n";

        [Description("players start unranked. unranked players cannot influence placement/ranked players. once the MinXpForPlacement is achieved they will progress to placement")]
        public string UnrankedTag { get; set; } = "Rank : --/--/--\n";
        public string UnrankedColor { get; set; } = "nickel";
        public Experiences.XP MinXpForPlacement { get; set; } = new Experiences.XP { value = 0, level = 0, stage = 1, tier = 0 };

        [Description("placement players rank is influenced by other placement players and ranked players but ranked players are not influenced by placement players")]
        public string PlacementTag { get; set; } = "Rank : ?\n";
        public string PlacementColor { get; set; } = "magenta";
        [Description("matches referes to kill/deaths against placement and ranked players. this is how many until you become ranked")]
        public int PlacementMatches { get; set; } = 1000;
        [Description("glicko-2 params set when a player start placement")]
        public float Rating { get; set; } = 1500;
        public float RatingDeviation { get; set; } = 350;
        public float RatingVolatility { get; set; } = 0.06f;

        public List<RankInfo> Ranks { get; set; } = new List<RankInfo>
        {
            new RankInfo{ Tag = "Silver I",                         Rating = -500,     Color = "nickel" },
            new RankInfo{ Tag = "Silver II",                        Rating = -250,     Color = "nickel" },
            new RankInfo{ Tag = "Silver III",                       Rating = 0,        Color = "nickel" },
            new RankInfo{ Tag = "Silver IV",                        Rating = 250,      Color = "nickel" },
            new RankInfo{ Tag = "Silver Elite",                     Rating = 500,      Color = "silver" },
            new RankInfo{ Tag = "Silver Elite Master",              Rating = 750,      Color = "silver" },
            new RankInfo{ Tag = "Gold Nova I",                      Rating = 1000,     Color = "cyan" },
            new RankInfo{ Tag = "Gold Nova II",                     Rating = 1250,     Color = "cyan" },
            new RankInfo{ Tag = "Gold Nova III",                    Rating = 1500,     Color = "cyan" },
            new RankInfo{ Tag = "Gold Nova Master",                 Rating = 1750,     Color = "aqua" },
            new RankInfo{ Tag = "Master Guardian I",                Rating = 2000,     Color = "blue_green" },
            new RankInfo{ Tag = "Master Gaurdian II",               Rating = 2250,     Color = "blue_green" },
            new RankInfo{ Tag = "Master Gaurdian Elite",            Rating = 2500,     Color = "emerald" },
            new RankInfo{ Tag = "Distinguished Master Gaurdian",    Rating = 2750,     Color = "mint" },
            new RankInfo{ Tag = "Legendary Eagle",                  Rating = 3000,     Color = "yellow" },
            new RankInfo{ Tag = "Legendary Eagle Master",           Rating = 3250,     Color = "yellow" },
            new RankInfo{ Tag = "Supreme Master First Class",       Rating = 3500,     Color = "orange" },
            new RankInfo{ Tag = "Global Elite",                     Rating = 3750,     Color = "crimson" },
        };
    }

    public class RankInfo
    {
        public string Tag { get; set; }
        /// <summary>
        /// min rating for rank
        /// </summary>
        public float Rating { get; set; }
        public string Color { get; set; }
    }

    public class Ranks
    {
        public static Ranks Singleton { get; private set; }
        private RankConfig config;

        private Dictionary<int, Database.Rank> player_ranks = new Dictionary<int, Database.Rank>();
        private Dictionary<int, GlickoPlayer> player_glikco = new Dictionary<int, GlickoPlayer>();
        private Dictionary<int, List<GlickoOpponent>> player_matches = new Dictionary<int, List<GlickoOpponent>>();

        public Ranks()
        {
            Singleton = this;
        }

        public void Init(RankConfig config)
        {
            this.config = config;
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            if (!player.DoNotTrack)
            {
                int id = player.PlayerId;
                if (!player_ranks.ContainsKey(id))
                    player_ranks.Add(id, new Database.Rank { UserId = player.UserId });
                Database.Singleton.LoadRank(player);
            }
        }

        //[PluginEvent(ServerEventType.PlayerLeft)]
        //void OnPlayerLeft(Player player)
        //{
        //    int id = player.PlayerId;
        //    if (player_ranks.ContainsKey(id))
        //        player_ranks.Remove(id);
        //}

        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDeath(Player victim, Player killer, DamageHandlerBase damage)
        {
            if (killer != null && victim != killer && player_glikco.ContainsKey(victim.PlayerId) && player_glikco.ContainsKey(killer.PlayerId))
            {
                Database.Rank victim_rank = player_ranks[victim.PlayerId];
                Database.Rank killer_rank = player_ranks[killer.PlayerId];

                if (!(victim_rank.state == Database.RankState.Ranked && killer_rank.state == Database.RankState.Placement))
                    player_matches[victim.PlayerId].Add(new GlickoOpponent(player_glikco[killer.PlayerId], 0));
                if (!(killer_rank.state == Database.RankState.Ranked && victim_rank.state == Database.RankState.Placement))
                    player_matches[killer.PlayerId].Add(new GlickoOpponent(player_glikco[victim.PlayerId], 1));

                if (victim_rank.state == Database.RankState.Placement)
                    victim_rank.placement_matches++;
                if (killer_rank.state == Database.RankState.Placement)
                    killer_rank.placement_matches++;
            }
        }

        public void RankLoaded(Player player)
        {
            Database.Rank rank = player_ranks[player.PlayerId];
            if (rank.state != Database.RankState.Unranked)
            {
                player_glikco.Add(player.PlayerId, new GlickoPlayer(rank.rating, rank.rd, rank.rv));
                player_glikco[player.PlayerId].Name = player.UserId;
                player_matches.Add(player.PlayerId, new List<GlickoOpponent>());
            }
            string tag = config.UnrankedTag;
            string color = config.UnrankedColor;
            if (rank.state == Database.RankState.Placement)
            {
                tag = config.PlacementTag;
                color = config.PlacementColor;
            }
            else if (rank.state == Database.RankState.Ranked)
            {
                RankInfo info = config.Ranks.First();
                int index = 1;
                while (rank.rating > info.Rating && index < config.Ranks.Count)
                {
                    info = config.Ranks[index];
                    index++;
                }
                tag = string.Format(config.BadgeFormat, info.Tag);
                color = info.Color;
            }
            BadgeOverride.Singleton.SetBadge(player, 0, tag);
            BadgeOverride.Singleton.SetBadgeColor(player, color);

            if (rank.state == Database.RankState.Placement || rank.state == Database.RankState.Ranked)
            {
                player_glikco.Add(player.PlayerId, new GlickoPlayer(rank.rating, rank.rd, rank.rv));
                player_matches.Add(player.PlayerId, new List<GlickoOpponent>());
            }
        }

        public void CalculateAndSaveRanks()
        {
            foreach (var id in player_ranks.Keys.ToList())
            {
                Database.Rank rank = player_ranks[id];
                if (rank.state == Database.RankState.Unranked)
                {
                    Player player = null;
                    if (Player.TryGet(id, out player))
                    {
                        Experiences.XP xp = Experiences.Singleton.GetXP(player);
                        Experiences.XP min = config.MinXpForPlacement;
                        if (xp.tier >= min.tier && xp.stage >= min.stage && xp.level >= min.level && xp.value >= min.value)
                        {
                            rank.state = Database.RankState.Placement;
                            rank.rating = config.Rating;
                            rank.rd = config.RatingDeviation;
                            rank.rv = config.RatingVolatility;
                        }
                    }
                }
                if (rank.state == Database.RankState.Placement)
                {
                    if (rank.placement_matches >= config.PlacementMatches)
                    {
                        rank.state = Database.RankState.Ranked;
                    }
                }
                if (player_glikco.ContainsKey(id))
                {
                    GlickoPlayer new_rank = GlickoCalculator.CalculateRanking(player_glikco[id], player_matches[id]);
                    rank.rating = (float)new_rank.Rating;
                    rank.rd = (float)new_rank.RatingDeviation;
                    rank.rv = (float)new_rank.Volatility;
                }
                Database.Singleton.SaveRank(rank);
            }
        }

        public Database.Rank GetRank(Player player)
        {
            return player_ranks[player.PlayerId];
        }

        //public class Rank
        //{
        //    public float rating;
        //    public float rd;
        //    public float rv;
        //}

        //private Dictionary<int, Rank> player_ranks = new Dictionary<int, Rank>();
        //private Dictionary<int, GlickoPlayer> player_glikco = new Dictionary<int, GlickoPlayer>();
        //private Dictionary<int, List<GlickoOpponent>> player_results = new Dictionary<int, List<GlickoOpponent>>();

        //public Ranks()
        //{
        //    Singleton = this;
        //}

        //public void Init(RankConfig config)
        //{
        //    this.config = config;
        //}

        //[PluginEvent(ServerEventType.PlayerJoined)]
        //void OnPlayerJoined(Player player)
        //{
        //    int id = player.PlayerId;
        //    if (!player_ranks.ContainsKey(id))
        //        player_ranks.Add(id, new Rank());
        //    Database.Singleton.LoadRank(player);
        //}

        //[PluginEvent(ServerEventType.PlayerLeft)]
        //void OnPlayerLeft(Player player)
        //{
        //    int id = player.PlayerId;
        //    if (player_ranks.ContainsKey(id))
        //        player_ranks.Remove(id);
        //}

        //[PluginEvent(ServerEventType.PlayerDeath)]
        //void OnPlayerDeath(Player target, Player killer, DamageHandlerBase damage)
        //{
        //    if(killer != null && target != killer && player_ranks[target.PlayerId].loaded && player_ranks[killer.PlayerId].loaded)
        //    {
        //        Rank target_rank = player_ranks[target.PlayerId];
        //        if (!player_glikco.ContainsKey(target.PlayerId))
        //        {
        //            player_glikco.Add(target.PlayerId, new GlickoPlayer(target_rank.rating, target_rank.rd, target_rank.rv));
        //            player_glikco[target.PlayerId].Name = target.UserId;
        //            player_results.Add(target.PlayerId, new List<GlickoOpponent>());
        //        }
        //        Rank killer_rank = player_ranks[killer.PlayerId];
        //        if (!player_glikco.ContainsKey(killer.PlayerId))
        //        {
        //            player_glikco.Add(killer.PlayerId, new GlickoPlayer(killer_rank.rating, killer_rank.rd, killer_rank.rv));
        //            player_glikco[killer.PlayerId].Name = killer.UserId;
        //            player_results.Add(killer.PlayerId, new List<GlickoOpponent>());
        //        }

        //        player_results[target.PlayerId].Add(new GlickoOpponent(player_glikco[killer.PlayerId], 0));
        //        player_results[killer.PlayerId].Add(new GlickoOpponent(player_glikco[target.PlayerId], 1));
        //    }
        //}

        //public Rank GetRank(Player player)
        //{
        //    return player_ranks[player.PlayerId];
        //}

        //public void CalculateAndSaveRanks()
        //{
        //    foreach(var id in player_glikco.Keys.ToList())
        //    {
        //        GlickoPlayer new_rank = GlickoCalculator.CalculateRanking(player_glikco[id], player_results[id]);
        //        Database.Singleton.SaveRank(player_glikco[id].Name, (float)new_rank.Rating, (float)new_rank.RatingDeviation, (float)new_rank.Volatility);
        //    }
        //    player_glikco.Clear();
        //    player_results.Clear();
        //}

        //public void RankLoaded(Player player)
        //{
        //    RankInfo info = config.Ranks.First();
        //    Rank rank = player_ranks[player.PlayerId];
        //    int index = 1;
        //    while(rank.rating > info.Elo && index < config.Ranks.Count)
        //    {
        //        info = config.Ranks[index];
        //        index++;
        //    }
        //    BadgeOverride.Singleton.SetBadge(player, 0, string.Format(config.BadgeFormat, info.Tag));
        //    BadgeOverride.Singleton.SetBadgeColor(player, info.Color);
        //}
    }

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class SetRank : ICommand
    {
        public string Command { get; } = "setrank";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "set rank on self";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player;
            if (Player.TryGet(sender, out player))
            {
                int rating;
                if (!int.TryParse(arguments.ElementAt(0), out rating))
                {
                    response = "failed";
                    return false;
                }
                Ranks.Singleton.GetRank(player).rating = rating;
                response = "success";
                return true;
            }
            response = "failed";
            return false;
        }
    }
}
