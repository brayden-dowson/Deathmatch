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
using static TheRiptide.Translation;
using RemoteAdmin;

namespace TheRiptide
{
    public class RankConfig
    {
        public bool IsEnabled { get; set; } = true;

        public string BadgeFormat { get; set; } = "Rank: {0}\n";

        [Description("players start unranked. unranked players cannot influence placement/ranked players. once the MinXpForPlacement is achieved they will progress to placement")]
        public string UnrankedTag { get; set; } = "Rank: --/--/--\n";
        public string UnrankedColor { get; set; } = "nickel";
        public Experiences.XP MinXpForPlacement { get; set; } = new Experiences.XP { value = 0, level = 0, stage = 2, tier = 0 };

        [Description("placement players rank is influenced by other placement players and ranked players but ranked players are not influenced by placement players")]
        public string PlacementTag { get; set; } = "Rank: ?\n";
        public string PlacementColor { get; set; } = "magenta";
        [Description("matches referes to kill/deaths against placement and ranked players. this is how many until you become ranked")]
        public int PlacementMatches { get; set; } = 300;
        [Description("glicko-2 params set when a player start placement")]
        public float Rating { get; set; } = 1500;
        public float RatingDeviation { get; set; } = 350;
        public float RatingVolatility { get; set; } = 0.06f;

        [Description("ranks must be in order of least rating to most rating and colors must be a valid servergroup color see https://en.scpslgame.com/index.php/Docs:Permissions")]
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

        public List<PlayerPermissions> RankCmdPermissions = new List<PlayerPermissions>()
        {
            PlayerPermissions.ServerConsoleCommands
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
        private Dictionary<string, GlickoPlayer> player_glikco = new Dictionary<string, GlickoPlayer>();
        private Dictionary<string, List<GlickoOpponent>> player_matches = new Dictionary<string, List<GlickoOpponent>>();

        public Ranks()
        {
            Singleton = this;
        }

        public void Init(RankConfig config)
        {
            this.config = config;
        }

        [PluginEvent(ServerEventType.MapGenerated)]
        void OnMapGenerated()
        {
            player_ranks.Clear();
            player_glikco.Clear();
            player_matches.Clear();
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            if (!player.DoNotTrack)
            {
                int id = player.PlayerId;
                if (!player_ranks.ContainsKey(id))
                {
                    var duplicates = player_ranks.Where((r) => r.Value.UserId == player.UserId);
                    foreach (var d in duplicates.ToList())
                        player_ranks.Remove(d.Key);
                    player_ranks.Add(id, new Database.Rank { UserId = player.UserId });
                    Database.Singleton.LoadRank(player);
                }
            }
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDeath(Player victim, Player killer, DamageHandlerBase damage)
        {
            if (killer != null && victim != killer && player_glikco.ContainsKey(victim.UserId) && player_glikco.ContainsKey(killer.UserId))
            {
                Database.Rank victim_rank = player_ranks[victim.PlayerId];
                Database.Rank killer_rank = player_ranks[killer.PlayerId];

                if (!(victim_rank.state == Database.RankState.Ranked && killer_rank.state == Database.RankState.Placement))
                    player_matches[victim.UserId].Add(new GlickoOpponent(player_glikco[killer.UserId], 0));
                if (!(killer_rank.state == Database.RankState.Ranked && victim_rank.state == Database.RankState.Placement))
                    player_matches[killer.UserId].Add(new GlickoOpponent(player_glikco[victim.UserId], 1));

                if (victim_rank.state == Database.RankState.Placement)
                    victim_rank.placement_matches++;
                if (killer_rank.state == Database.RankState.Placement)
                    killer_rank.placement_matches++;
            }
        }

        public void RankLoaded(Player player)
        {
            Database.Rank rank = player_ranks[player.PlayerId];
            if (rank.state != Database.RankState.Unranked && !player_glikco.ContainsKey(player.UserId))
            {
                player_glikco.Add(player.UserId, new GlickoPlayer(rank.rating, rank.rd, rank.rv));
                player_matches.Add(player.UserId, new List<GlickoOpponent>());
            }
            SetBadge(player, rank);
            ShowRankHint(player, rank, 10.0f);
            HintOverride.Refresh(player);
        }

        public void CalculateAndSaveRanks()
        {
            foreach (var id in player_ranks.Keys.ToList())
            {
                Database.Rank rank = player_ranks[id];
                Player player = Player.Get(id);
                if (rank.state == Database.RankState.Unranked)
                {
                    if (player != null)
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
                        rank.state = Database.RankState.Ranked;
                }
                if (player_glikco.ContainsKey(rank.UserId))
                {
                    try
                    {
                        GlickoPlayer new_rank = GlickoCalculator.CalculateRanking(new GlickoPlayer(rank.rating, rank.rd, rank.rv), player_matches[rank.UserId]);
                        rank.rating = (float)new_rank.Rating;
                        rank.rd = (float)new_rank.RatingDeviation;
                        rank.rv = (float)new_rank.Volatility;
                    }
                    catch(Exception ex)
                    {
                        Log.Error("glicko error: " + ex.ToString());
                    }
                }
                if (player != null)
                {
                    RankInfo info = GetInfo(rank.rating);
                    ShowRankHint(player, rank, 30.0f);
                    SetBadge(player, rank);
                }
                Database.Singleton.SaveRank(rank);
            }
        }

        public void SetRank(Player player, float rating)
        {
            if (player_glikco.ContainsKey(player.UserId))
                player_glikco[player.UserId].Rating = rating;
        }

        public Database.Rank GetRank(Player player)
        {
            return player_ranks[player.PlayerId];
        }

        private RankInfo GetInfo(float rating)
        {
            int index = config.Ranks.Count - 1;
            while (index > 0 && rating < config.Ranks[index].Rating)
                index--;
            return config.Ranks[index];
        }

        private void SetBadge(Player player, Database.Rank rank)
        {
            string tag = config.UnrankedTag;
            string color = config.UnrankedColor;
            if (rank.state == Database.RankState.Placement)
            {
                tag = config.PlacementTag;
                color = config.PlacementColor;
            }
            else if (rank.state == Database.RankState.Ranked)
            {
                RankInfo info = GetInfo(rank.rating);
                tag = string.Format(config.BadgeFormat, info.Tag);
                color = info.Color;
            }
            BadgeOverride.Singleton.SetBadge(player, 0, tag);
            BadgeOverride.Singleton.SetBadgeColor(player, color);
        }

        private void ShowRankHint(Player player, Database.Rank rank, float duration)
        {
            string tag = config.UnrankedTag;
            string color = config.UnrankedColor;
            if (rank.state == Database.RankState.Placement)
            {
                tag = config.PlacementTag;
                color = config.PlacementColor;
            }
            else if (rank.state == Database.RankState.Ranked)
            {
                RankInfo info = GetInfo(rank.rating);
                tag = string.Format(config.BadgeFormat, info.Tag);
                color = info.Color;
            }

            if (!BadgeOverride.ColorNameToHex.ContainsKey(color))
            {
                Log.Error("invalid rank color: " + color);
                return;
            }
            string rank_hint = translation.RankMsg.Replace("{color}", BadgeOverride.ColorNameToHex[color]).Replace("{rank}", tag);
            HintOverride.Add(player, 0, rank_hint, duration);
        }

        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        public class DmSetRank : ICommand
        {
            public string Command { get; } = "dmsetrank";

            public string[] Aliases { get; } = new string[] { "dmr"};

            public string Description { get; } = "set players rating. usage: dmsetrank [playerid] [rating]";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (sender is PlayerCommandSender sender1 && !sender1.CheckPermission(Singleton.config.RankCmdPermissions.ToArray(), out response))
                    return false;

                if (arguments.Count != 2)
                {
                    response = "usage: dmsetrank [playerid] [rating]";
                    return false;
                }

                int id;
                Player target = null;
                if (!int.TryParse(arguments.ElementAt(0), out id) || !Player.TryGet(id, out target))
                {
                    response = "failed invalid id: " + arguments.ElementAt(0);
                    return false;
                }

                int rating;
                if (!int.TryParse(arguments.ElementAt(1), out rating))
                {
                    response = "failed invalid rating: " + arguments.ElementAt(1);
                    return false;
                }

                Singleton.SetRank(target, rating);
                response = "success";
                return true;
            }
        }

        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        public class DmGetRank : ICommand
        {
            public string Command { get; } = "dmgetrank";

            public string[] Aliases { get; } = new string[] { "dmgr" };

            public string Description { get; } = "set players rating. usage: dmsetrank [player_id] [rating]";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (sender is PlayerCommandSender sender1 && !sender1.CheckPermission(Singleton.config.RankCmdPermissions.ToArray(), out response))
                    return false;

                if (arguments.Count != 1)
                {
                    response = "usage: dmgetrank [playerid]";
                    return false;
                }

                int id;
                Player target = null;
                if (!int.TryParse(arguments.ElementAt(0), out id) || !Player.TryGet(id, out target))
                {
                    response = "failed invalid id: " + arguments.ElementAt(0);
                    return false;
                }

                response = target.Nickname + " rating is " + Singleton.GetRank(target).rating.ToString("0");
                return true;
            }
        }

        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        public class DmSetRankState : ICommand
        {
            public string Command { get; } = "dmsetrankstate";

            public string[] Aliases { get; } = new string[] { "dmrs" };

            public string Description { get; } = "set players rank state. usage: dmsetrankstate [player_id] [state]";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (sender is PlayerCommandSender sender1 && !sender1.CheckPermission(Singleton.config.RankCmdPermissions.ToArray(), out response))
                    return false;

                if(arguments.Count != 2)
                {
                    response = "usage: dmsetrankstate [playerid] [state]. states: 0 = Unranked 1 = Placement 2 = Ranked";
                    return false;
                }

                int id;
                Player target = null;
                if (!int.TryParse(arguments.ElementAt(0), out id) || !Player.TryGet(id, out target))
                {
                    response = "failed - invalid id: " + arguments.ElementAt(0);
                    return false;
                }

                int state;
                if (!int.TryParse(arguments.ElementAt(1), out state))
                {
                    response = "failed - invalid state: " + arguments.ElementAt(1);
                    return false;
                }

                if (!Enum.IsDefined(typeof(Database.RankState), state))
                {
                    response = "failed - invalid state: valid states are 0 = Unranked 1 = Placement 2 = Ranked";
                    return false;
                }

                Singleton.GetRank(target).state = (Database.RankState)state;
                response = "success";
                return true;
            }
        }
    }
}
