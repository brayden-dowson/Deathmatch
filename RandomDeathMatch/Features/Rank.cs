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

        public string BadgeFormat { get; set; } = "Rank: {name}\n";
        public string LeaderBoardFormat { get; set; } = "{tag}";

        public string DntName { get; set; } = "Do Not Track";
        public string DntTag { get; set; } = "DNT";
        public string DntColor { get; set; } = "nickel";

        [Description("players start unranked. unranked players cannot influence placement/ranked players. once the MinXpForPlacement is achieved they will progress to placement")]
        public string UnrankedName { get; set; } = "Unranked";
        public string UnrankedTag { get; set; } = "----";
        public string UnrankedColor { get; set; } = "nickel";
        [Description("minimum rank before players obtain placement rank status. values start at 0 not 1. e.g. level 0 = L:1 level 1 = L2")]
        public Experiences.XP MinXpForPlacement { get; set; } = new Experiences.XP { value = 0, level = 0, stage = 1, tier = 0 };

        [Description("placement players rank is influenced by other placement players and ranked players but ranked players are not influenced by placement players")]
        public string PlacementName { get; set; } = "Placement";
        public string PlacementTag { get; set; } = "?";
        public string PlacementColor { get; set; } = "magenta";
        [Description("matches referes to kill/deaths against placement and ranked players. this is how many until you become ranked")]
        public int PlacementMatches { get; set; } = 150;

        //[Description("glicko-2 params set when a player start placement")]
        //public float Rating { get; set; } = 1500;
        //public float RatingDeviation { get; set; } = 350;
        //public float RatingVolatility { get; set; } = 0.06f;

        [Description("ranks must be in order of least rating to most rating and colors must be a valid servergroup colors see https://en.scpslgame.com/index.php/Docs:Permissions \n#All players start out with a 1500 rating when they enter placement. Ranks should be scaled around the 1500 mark.")]
        public List<RankInfo> Ranks { get; set; } = new List<RankInfo>
        {
            new RankInfo{ Name = "Silver I",                        Tag = "S1",     Rating = -150,     Color = "nickel" },
            new RankInfo{ Name = "Silver II",                       Tag = "S2",     Rating = 0,        Color = "nickel" },
            new RankInfo{ Name = "Silver III",                      Tag = "S3",     Rating = 150,      Color = "nickel" },
            new RankInfo{ Name = "Silver IV",                       Tag = "S4",     Rating = 300,      Color = "nickel" },
            new RankInfo{ Name = "Silver Elite",                    Tag = "SE",     Rating = 550,      Color = "silver" },
            new RankInfo{ Name = "Silver Elite Master",             Tag = "SEM",    Rating = 700,      Color = "silver" },
            new RankInfo{ Name = "Gold Nova I",                     Tag = "GN1",    Rating = 950,      Color = "cyan" },
            new RankInfo{ Name = "Gold Nova II",                    Tag = "GN2",    Rating = 1100,     Color = "cyan" },
            new RankInfo{ Name = "Gold Nova III",                   Tag = "GN3",    Rating = 1350,     Color = "cyan" },
            new RankInfo{ Name = "Gold Nova Master",                Tag = "GNM",    Rating = 1500,     Color = "aqua" },
            new RankInfo{ Name = "Master Guardian I",               Tag = "MG1",    Rating = 1650,     Color = "blue_green" },
            new RankInfo{ Name = "Master Gaurdian II",              Tag = "MG2",    Rating = 1800,     Color = "blue_green" },
            new RankInfo{ Name = "Master Gaurdian Elite",           Tag = "MGE",    Rating = 1950,     Color = "emerald" },
            new RankInfo{ Name = "Distinguished Master Gaurdian",   Tag = "DMG",    Rating = 2100,     Color = "mint" },
            new RankInfo{ Name = "Legendary Eagle",                 Tag = "LE",     Rating = 2250,     Color = "yellow" },
            new RankInfo{ Name = "Legendary Eagle Master",          Tag = "LEM",    Rating = 2400,     Color = "yellow" },
            new RankInfo{ Name = "Supreme Master First Class",      Tag = "SMFC",   Rating = 2550,     Color = "orange" },
            new RankInfo{ Name = "Global Elite",                    Tag = "GE",     Rating = 2700,     Color = "crimson" },
        };

        public List<PlayerPermissions> RankCmdPermissions { get; set; } = new List<PlayerPermissions>()
        {
            PlayerPermissions.ServerConsoleCommands
        };
    }

    public class RankInfo
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        /// <summary>
        /// min rating for rank
        /// </summary>
        public float Rating { get; set; }
        public string Color { get; set; }
    }

    public class Ranks
    {
        private const float Rating = 1500.0f;
        private const float RatingDeviation = 350.0f;
        private const float RatingVolatility = 0.06f;

        public static Ranks Singleton { get; private set; }
        public RankConfig config;

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

        public void MapGenerated()
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
            else
            {
                BadgeOverride.Singleton.SetBadge(player, 0, config.BadgeFormat.Replace("{name}", config.DntName));
                BadgeOverride.Singleton.SetBadgeColor(player, config.DntColor);
            }
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDeath(Player victim, Player killer, DamageHandlerBase damage)
        {
            if (killer != null && victim != killer && player_glikco.ContainsKey(victim.UserId) && player_glikco.ContainsKey(killer.UserId))
            {
                Database.Rank victim_rank = player_ranks[victim.PlayerId];
                Database.Rank killer_rank = player_ranks[killer.PlayerId];

                //if (!(victim_rank.state == Database.RankState.Ranked && killer_rank.state == Database.RankState.Placement))
                //    player_matches[victim.UserId].Add(new GlickoOpponent(player_glikco[killer.UserId], 0));
                //if (!(killer_rank.state == Database.RankState.Ranked && victim_rank.state == Database.RankState.Placement))
                //    player_matches[killer.UserId].Add(new GlickoOpponent(player_glikco[victim.UserId], 1));

                if (victim_rank.state == Database.RankState.Placement || killer_rank.state == Database.RankState.Ranked)//(ranked/placement) kills placement || ranked kills ranked
                    player_matches[victim.UserId].Add(new GlickoOpponent(player_glikco[killer.UserId], 0));//-1 on victim
                if (killer_rank.state == Database.RankState.Placement || victim_rank.state == Database.RankState.Ranked)//(ranked/placement) kills ranked    || placement kills placement
                    player_matches[killer.UserId].Add(new GlickoOpponent(player_glikco[victim.UserId], 1));//+1 on killer

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
                try
                {
                    Database.Rank rank = player_ranks[id];
                    Player player = null;
                    try { player = Player.Get(id); }
                    catch (Exception) { }
                    if (rank.state == Database.RankState.Unranked)
                    {
                        if (player != null)
                        {
                            Experiences.XP xp = Experiences.Singleton.GetXP(player);
                            Experiences.XP min = config.MinXpForPlacement;
                            if (xp.tier >= min.tier && xp.stage >= min.stage && xp.level >= min.level && xp.value >= min.value)
                            {
                                rank.state = Database.RankState.Placement;
                                rank.rating = Rating;
                                rank.rd = RatingDeviation;
                                rank.rv = RatingVolatility;
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
                        catch (Exception ex)
                        {
                            Log.Error("glicko error: " + ex.ToString());
                        }
                    }
                    Database.Singleton.SaveRank(rank);
                    if (player != null)
                    {
                        ShowRankHint(player, rank, 30.0f);
                        SetBadge(player, rank);
                    }
                }
                catch(System.Exception ex)
                {
                    Log.Error("rank save error: " + ex.ToString());
                }
            }
        }

        public bool SetRank(Player player, float rating)
        {
            if (player_ranks.ContainsKey(player.PlayerId))
            {
                player_ranks[player.PlayerId].rating = rating;
                return true;
            }
            return false;
        }

        public bool ResetRank(Player player)
        {
            if (player_ranks.ContainsKey(player.PlayerId))
            {
                player_ranks[player.PlayerId].rating = Rating;
                player_ranks[player.PlayerId].rd = RatingDeviation;
                player_ranks[player.PlayerId].rv = RatingVolatility;
                return true;
            }
            return false;
        }

        public void DeleteAllRanks()
        {
            Database.Singleton.Async((db)=>
            {
                var ranks = db.GetCollection<Database.Rank>("ranks");
                ranks.DeleteAll();
            });
        }

        public void NormalizeRanks()
        {
            double total = 0.0f;
            int count = 0;
            Database.Singleton.Async((db) =>
            {
                var ranks = db.GetCollection<Database.Rank>("ranks");
                ranks.EnsureIndex(x => x.UserId);
                foreach (var rank in ranks.FindAll())
                {
                    if(rank.state != Database.RankState.Unranked)
                    {
                        total += rank.rating;
                        count++;
                    }
                }
                float delta = (float)(Rating - (total / count));
                foreach (var rank in ranks.FindAll())
                {
                    if(rank.state  != Database.RankState.Unranked)
                    {
                        rank.rating += delta;
                        ranks.Upsert(rank);
                    }
                }
            });
        }

        public Database.Rank GetRank(Player player)
        {
            return player_ranks[player.PlayerId];
        }

        public RankInfo GetInfo(Database.Rank rank)
        {
            RankInfo info = new RankInfo { Name = config.UnrankedName, Tag = config.UnrankedTag, Color = config.UnrankedColor };
            if (rank.state == Database.RankState.Placement)
            {
                info.Name = config.PlacementName;
                info.Tag = config.PlacementTag;
                info.Color = config.PlacementColor;
            }
            else if (rank.state == Database.RankState.Ranked)
            {
                int index = config.Ranks.Count - 1;
                while (index > 0 && rank.rating < config.Ranks[index].Rating)
                    index--;

                info.Name = config.Ranks[index].Name;
                info.Tag = config.Ranks[index].Tag;
                info.Color = config.Ranks[index].Color;
            }
            return info;
        }

        private void SetBadge(Player player, Database.Rank rank)
        {
            RankInfo info = GetInfo(rank);
            BadgeOverride.Singleton.SetBadge(player, 0, config.BadgeFormat.Replace("{name}", info.Name));
            BadgeOverride.Singleton.SetBadgeColor(player, info.Color);
        }

        private void ShowRankHint(Player player, Database.Rank rank, float duration)
        {
            RankInfo info = GetInfo(rank);
            if (!BadgeOverride.ColorNameToHex.ContainsKey(info.Color))
            {
                Log.Error("invalid rank color: " + info.Color);
                return;
            }
            string rank_hint = translation.RankMsg.Replace("{color}", BadgeOverride.ColorNameToHex[info.Color]).Replace("{rank}", config.BadgeFormat.Replace("{name}", info.Name));
            HintOverride.Add(player, 0, rank_hint, duration);
        }

        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        public class DmNormalizeRanks : ICommand
        {
            public bool SanitizeResponse => false;

            public string Command { get; } = "dm_normalize_ranks";

            public string[] Aliases { get; } = new string[] { "dmnr" };

            public string Description { get; } = "normalizes all ranks to be around 1500. usage: dm_normalize_ranks then soft restart";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (sender is PlayerCommandSender sender1 && !sender1.CheckPermission(Singleton.config.RankCmdPermissions.ToArray(), out response))
                    return false;

                Singleton.NormalizeRanks();
                response = "success";
                return true;
            }
        }

        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        public class DmDeleteAllRanks : ICommand
        {
            public bool SanitizeResponse => false;

            public string Command { get; } = "dm_delete_all_ranks";

            public string[] Aliases { get; } = new string[] { "dmdar" };

            public string Description { get; } = "deletes all player ranks. usage: dm_delete_all_ranks";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (sender is PlayerCommandSender sender1 && !sender1.CheckPermission(Singleton.config.RankCmdPermissions.ToArray(), out response))
                    return false;

                Singleton.DeleteAllRanks();
                response = "success";
                return true;
            }
        }

        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        public class DmResetRank : ICommand
        {
            public bool SanitizeResponse => false;

            public string Command { get; } = "dm_reset_rank";

            public string[] Aliases { get; } = new string[] { "dmrr" };

            public string Description { get; } = "reset players rating. usage: dm_reset_rank [playerid]";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (sender is PlayerCommandSender sender1 && !sender1.CheckPermission(Singleton.config.RankCmdPermissions.ToArray(), out response))
                    return false;

                int id;
                Player target = null;
                if (!int.TryParse(arguments.ElementAt(0), out id) || !Player.TryGet(id, out target))
                {
                    response = "failed - invalid id: " + arguments.ElementAt(0);
                    return false;
                }

                if (Singleton.ResetRank(target))
                    response = "success";
                else
                    response = "failed - player does not have a rank yet";
                return true;
            }
        }

        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        public class DmSetRank : ICommand
        {
            public bool SanitizeResponse => false;

            public string Command { get; } = "dm_set_rank";

            public string[] Aliases { get; } = new string[] { "dsr"};

            public string Description { get; } = "set players rating. usage: dm_set_rank [playerid] [rating]";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (sender is PlayerCommandSender sender1 && !sender1.CheckPermission(Singleton.config.RankCmdPermissions.ToArray(), out response))
                    return false;

                if (arguments.Count != 2)
                {
                    response = "usage: dm_set_rank [playerid] [rating]";
                    return false;
                }

                int id;
                Player target = null;
                if (!int.TryParse(arguments.ElementAt(0), out id) || !Player.TryGet(id, out target))
                {
                    response = "failed - invalid id: " + arguments.ElementAt(0);
                    return false;
                }

                int rating;
                if (!int.TryParse(arguments.ElementAt(1), out rating))
                {
                    response = "failed - invalid rating: " + arguments.ElementAt(1);
                    return false;
                }

                if (Singleton.SetRank(target, rating))
                    response = "success";
                else
                    response = "failed - player does not have a rank yet";
                return true;
            }
        }

        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        [CommandHandler(typeof(GameConsoleCommandHandler))]
        public class DmGetRank : ICommand
        {
            public bool SanitizeResponse => false;

            public string Command { get; } = "dm_get_rank";

            public string[] Aliases { get; } = new string[] { "dmgr" };

            public string Description { get; } = "get players rating. usage: dm_get_rank [player_id]";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (sender is PlayerCommandSender sender1 && !sender1.CheckPermission(Singleton.config.RankCmdPermissions.ToArray(), out response))
                    return false;

                if (arguments.Count != 1)
                {
                    response = "usage: dm_get_rank [playerid]";
                    return false;
                }

                int id;
                Player target = null;
                if (!int.TryParse(arguments.ElementAt(0), out id) || !Player.TryGet(id, out target))
                {
                    response = "failed - invalid id: " + arguments.ElementAt(0);
                    return false;
                }

                if (Singleton.player_ranks.ContainsKey(target.PlayerId))
                    response = target.Nickname + " rating is " + Singleton.GetRank(target).rating.ToString("0");
                else
                    response = "failed - player does not have a rank yet";
                return true;
            }
        }

        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        public class DmSetRankState : ICommand
        {
            public bool SanitizeResponse => false;

            public string Command { get; } = "dm_set_rank_state";

            public string[] Aliases { get; } = new string[] { "dmrs" };

            public string Description { get; } = "set players rank state. usage: dm_set_rank_state [player_id] [state], states: 0 = Unranked 1 = Placement 2 = Ranked";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (sender is PlayerCommandSender sender1 && !sender1.CheckPermission(Singleton.config.RankCmdPermissions.ToArray(), out response))
                    return false;

                if(arguments.Count != 2)
                {
                    response = "usage: dm_set_rank_state [playerid] [state], states: 0 = Unranked 1 = Placement 2 = Ranked";
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
