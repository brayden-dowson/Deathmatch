using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using UnityEngine;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TheRiptide
{
    public class LeaderBoardConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("Sessions before this date are ignored in the leader board, applies to total kills, highest killstreak and total time ")]
        public DateTime BeginEpoch { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
        [Description("Sessions after this date are ignored in the leader board, applies to total kills, highest killstreak and total time ")]
        public DateTime EndEpoch { get; set; } = new DateTime(DateTime.Now.Year, 4, 1);
        [Description("How often to advance the Epoch in months. Triggered when the current date is beyond the EndEpoch")]
        public int AutoIncrementPeriod { get; set; } = 3;

        public int LinesPerPage { get; set; } = 25;
    }

    enum LeaderBoardType { Rank, Experience, Killstreak, Kills, Time }

    class LeaderBoard
    {
        public static LeaderBoard Singleton { get; private set; }

        private LeaderBoardConfig config;

        class PlayerDetails
        {
            public string name = "";
            public float rank_rating = 0.0f;
            public string rank_tag = "";
            public string rank_color = "";
            public ulong xp_total = 0;
            public string xp_tag = "";
            public int total_kills = 0;
            public int highest_killstreak = 0;
            public string killstreak_tag = "";
            public int total_play_time = 0;
            //public float kill_to_death_ratio;
            //public float hit_head_shot_percentage;
            //public float hit_accuracy_percentage;
        }

        private Dictionary<string, int> user_index = new Dictionary<string, int>();
        private List<PlayerDetails> player_details = new List<PlayerDetails>();
        private Dictionary<LeaderBoardType, List<int>> type_order = new Dictionary<LeaderBoardType, List<int>>()
        {
            { LeaderBoardType.Rank,          new List<int>{} },
            { LeaderBoardType.Experience,    new List<int>{} },
            { LeaderBoardType.Killstreak,    new List<int>{} },
            { LeaderBoardType.Kills,         new List<int>{} },
            { LeaderBoardType.Time,          new List<int>{} },
        };

        class State
        {
            public LeaderBoardType type;
            public int page = 0;
            public float cooldown = 0;
        }

        private Dictionary<int, State> player_leader_board_state = new Dictionary<int, State>();
        private CoroutineHandle controller;

        public LeaderBoard()
        {
            Singleton = this;
        }

        public void Init(LeaderBoardConfig config)
        {
            this.config = config;
            controller = Timing.RunCoroutine(_Controller());
        }

        [PluginEvent(ServerEventType.MapGenerated)]
        public void OnMapGenerated()
        {
            bool dirty = false;
            while (DateTime.Now > config.EndEpoch)
            {
                dirty = true;
                config.BeginEpoch = config.BeginEpoch.AddMonths(config.AutoIncrementPeriod);
                config.EndEpoch = config.EndEpoch.AddMonths(config.AutoIncrementPeriod);
            }
            if (dirty)
                RebuildLeaderBoard();

            PluginHandler handler = PluginHandler.Get(Deathmatch.Singleton);
            handler.SaveConfig(Deathmatch.Singleton, nameof(Deathmatch.Singleton.leader_board_config));
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            if (player_leader_board_state.ContainsKey(player.PlayerId))
                player_leader_board_state.Remove(player.PlayerId);
        }

        private int MaxPages(LeaderBoardType type)
        {
            return Mathf.CeilToInt(type_order[type].Count / config.LinesPerPage);
        }

        private IEnumerator<float> _Controller()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                try
                {
                    float delta = (float)stopwatch.Elapsed.TotalSeconds;
                    stopwatch.Restart();
                    foreach (var id in player_leader_board_state.Keys.ToList())
                    {
                        Player player;
                        if (Player.TryGet(id, out player))
                        {
                            State state = player_leader_board_state[id];
                            //BroadcastOverride.BroadcastLine(player,1,1,BroadcastPriority.Highest,state.cooldown.ToString("0.000") + " | " + state.page.ToString() + " | " + state.type.ToString());
                            if (player.Velocity == Vector3.zero)
                                state.cooldown = 0.0f;
                            state.cooldown -= delta;
                            if (state.cooldown <= 0.0f)
                            {
                                bool updated = false;
                                var dir = Quaternion.Inverse(player.GameObject.transform.rotation) * player.Velocity;
                                //BroadcastOverride.BroadcastLine(player, 2, 1, BroadcastPriority.Highest, dir.ToPreciseString());
                                //foward
                                if (dir.z > 3.0f)
                                {
                                    state.page = Mathf.Max(state.page - 1, 0);
                                    updated = true;
                                }
                                //backward
                                if (dir.z < -3.0f)
                                {
                                    state.page = Mathf.Min(state.page + 1, MaxPages(state.type));
                                    updated = true;
                                }
                                //right
                                if (dir.x > 3.0f)
                                {
                                    state.type = (LeaderBoardType)Mathf.Min((int)state.type + 1, Enum.GetValues(typeof(LeaderBoardType)).Length - 1);
                                    state.page = 0;
                                    updated = true;
                                }
                                //left
                                if (dir.x < -3.0f)
                                {
                                    state.type = (LeaderBoardType)Mathf.Max((int)state.type - 1, 0);
                                    state.page = 0;
                                    updated = true;
                                }
                                if (updated)
                                {
                                    state.cooldown = 0.5f;
                                    DisplayLeaderBoard(player, state.type, state.page);
                                }
                            }
                            //BroadcastOverride.UpdateIfDirty(player);
                        }
                        else
                            player_leader_board_state.Remove(id);
                    }
                }
                catch(Exception ex)
                {
                    Log.Error("LeaderBoard _Controller error: " + ex.ToString());
                }
                yield return Timing.WaitForOneFrame;
            }
        }

        public void EnableLeaderBoardMode(Player player, LeaderBoardType type)
        {
            if (!player_leader_board_state.ContainsKey(player.PlayerId))
                player_leader_board_state.Add(player.PlayerId, new State { type = type });
            DisplayLeaderBoard(player, type, 0);
        }

        public void DisableLeaderBoardMode(Player player)
        {
            if (player_leader_board_state.ContainsKey(player.PlayerId))
                player_leader_board_state.Remove(player.PlayerId);
            //HintOverride.Clear(player);
            //HintOverride.Refresh(player);
            player.ReceiveHint("", 1);
        }

        public void DisplayLeaderBoard(Player player, LeaderBoardType type, int page)
        {
            if (player_details.IsEmpty())
                ReloadLeaderBoard();

            int name_space = 18;
            int rank_space = Ranks.Singleton.config.Ranks.Max(x => x.Tag.Length);
            int xp_space = Experiences.Singleton.config.LeaderBoardFormat.Replace("{tier}", "").Replace("{stage}", "").Replace("{level}", "").Length + Experiences.Singleton.config.LeaderBoardTierTags.Max(x => x.Length) + Experiences.Singleton.config.LeaderBoardStageTags.Max(x => x.Length) + Experiences.Singleton.config.LeaderBoardLevelTags.Max(x => x.Length);
            int ks_space = Killstreaks.Singleton.config.KillstreakTables.Keys.Max(x => x.Length);

            int line = page * config.LinesPerPage;
            int max = Mathf.Min((page + 1) * config.LinesPerPage, type_order[type].Count);
            Func<string, bool, string> highlight = new Func<string, bool, string>((s, b) => { return b ? "<b><color=#43BFF0>" + s + "</color></b>" : s; });
            string leader_board_title = "<color=#43BFF0><b><size=64>Leader Board</size></b></color>";
            string leader_board_legend = "Pos" +
                " " + highlight(("Name").PadRight(name_space), type == LeaderBoardType.Rank) +
                " " + highlight(("Rank").PadRight(rank_space), type == LeaderBoardType.Rank) +
                " " + highlight(("Experience").PadRight(xp_space), type == LeaderBoardType.Experience) +
                " " + highlight(("Top Killstreak").PadRight(ks_space + 4), type == LeaderBoardType.Killstreak) +
                " " + highlight(("Kills").PadRight(5), type == LeaderBoardType.Kills) +
                " " + highlight(("Time").PadRight(4), type == LeaderBoardType.Time);
            int width = leader_board_legend.Length - "<b><color=#43BFF0></color></b>".Length;
            string hint = "<voffset=8em>" + leader_board_title + "<size=30><line-height=75%><mspace=0.55em>\n\n" + leader_board_legend + "\n" + new string('—', width);
            for (int i = line; i < max; i++)
            {
                PlayerDetails details = player_details[type_order[type].ElementAt(i)];
                string killstreak_color = "#FFFFFF";
                if (details.killstreak_tag == "")
                    details.killstreak_tag = Killstreaks.Singleton.config.DefaultKillstreak;
                if (Killstreaks.Singleton.config.KillstreakTables.ContainsKey(details.killstreak_tag))
                    killstreak_color = Killstreaks.Singleton.config.KillstreakTables[details.killstreak_tag].ColorHex;
                string bold_tag = "";
                string end_bold_tag = "";
                if (user_index.ContainsKey(player.UserId) && user_index[player.UserId] == type_order[type].ElementAt(i))
                {
                    bold_tag = "<b>";
                    end_bold_tag = "</b>";
                }
                if (details.rank_color == "")
                    details.rank_color = BadgeOverride.ColorNameToHex["nickel"];
                details.name = details.name.Length > name_space ? details.name.Substring(0, name_space) : details.name;
                string name_str = "<noparse>" + details.name.PadRight(name_space).Replace("{", "｛").Replace("}", "｝") + "</noparse>";
                string rank_str = details.rank_tag.PadRight(rank_space);
                string xp_str = details.xp_tag.PadRight(xp_space);
                string ks_str = details.killstreak_tag.PadRight(ks_space);
                string rank_color = "<color=" + details.rank_color + ">";
                string ks_color = "<color=" + killstreak_color + ">";
                hint += "\n" + bold_tag + "|" + (i + 1).ToString().PadLeft(3) + "|" + name_str + "|" + rank_color + rank_str + "</color>|" + xp_str + "|" + ks_color + ks_str + "</color> " + details.highest_killstreak.ToString().PadRight(3) + "|" + details.total_kills.ToString().PadRight(5) + "|" + (details.total_play_time/60).ToString().PadRight(4) + "|" + end_bold_tag;
            }
            hint += "\n" + new string('—', width) + "\n";
            hint += "Page " + (page + 1) + " of " + (MaxPages(type) + 1) + " [" + (line + 1) + " - " + max + "]/" + type_order[type].Count;
            player.SendConsoleMessage(hint);
            player.ReceiveHint("", 300);
            player.ReceiveHint(hint, 300);
        }

        public void ReloadLeaderBoard()
        {
            user_index.Clear();
            player_details.Clear();
            foreach (var key in type_order.Keys.ToList())
                type_order[key].Clear();

            //user
            var user_collection = Database.Singleton.DB.GetCollection<Database.User>("users");
            user_collection.EnsureIndex(x => x.UserId);
            var users = user_collection.Include(x => x.tracking).Include(x => x.tracking.sessions).FindAll();

            foreach (var user in users)
            {
                user_index.Add(user.UserId, player_details.Count);
                player_details.Add(new PlayerDetails());
                player_details.Last().name = user.tracking.sessions.Last().nickname;
            }

            //rank
            var rank_collection = Database.Singleton.DB.GetCollection<Database.Rank>("ranks");
            rank_collection.EnsureIndex(x => x.UserId);
            IEnumerable<Database.Rank> db_ranks = rank_collection.FindAll();

            foreach (var rank in db_ranks)
            {
                PlayerDetails details = player_details[user_index[rank.UserId]];
                RankInfo info = Ranks.Singleton.GetInfo(rank);
                details.rank_rating = rank.rating;
                details.rank_tag = info.Tag;
                details.rank_color = BadgeOverride.ColorNameToHex[info.Color];
            }

            //xp
            var xp_collection = Database.Singleton.DB.GetCollection<Database.Experience>("experiences");
            xp_collection.EnsureIndex(x => x.UserId);
            IEnumerable<Database.Experience> db_xps = xp_collection.FindAll();

            ulong level_stride = (ulong)Experiences.Singleton.MaxLevelXp();
            ulong stage_stride = level_stride * (ulong)Experiences.Singleton.config.StageTags.Count;
            ulong tier_stride = stage_stride * (ulong)Experiences.Singleton.config.TierTags.Count;
            foreach (var xp in db_xps)
            {
                PlayerDetails details = player_details[user_index[xp.UserId]];
                details.xp_total = (ulong)xp.tier * tier_stride + (ulong)xp.stage * stage_stride + (ulong)xp.level * level_stride + (ulong)xp.value;
                details.xp_tag = Experiences.Singleton.LeaderBoardString(new Experiences.XP { tier = xp.tier, stage = xp.stage, level = xp.level, value = xp.value });
            }

            //other
            var leader_board_collection = Database.Singleton.DB.GetCollection<Database.LeaderBoard>("leader_board");
            leader_board_collection.EnsureIndex(x => x.UserId);
            var db_leaderboard = leader_board_collection.FindAll();

            foreach (var record in db_leaderboard)
            {
                PlayerDetails details = player_details[user_index[record.UserId]];
                details.total_kills = record.total_kills;
                details.highest_killstreak = record.highest_killstreak;
                details.killstreak_tag = record.killstreak_tag;
                details.total_play_time = record.total_play_time;
            }

            type_order[LeaderBoardType.Rank] = SortIndex(x => x.rank_rating);
            type_order[LeaderBoardType.Experience] = SortIndex(x => x.xp_total);
            type_order[LeaderBoardType.Kills] = SortIndex(x => x.total_kills);
            type_order[LeaderBoardType.Killstreak] = SortIndex(x => x.highest_killstreak);
            type_order[LeaderBoardType.Time] = SortIndex(x => x.total_play_time);
        }

        public void RebuildLeaderBoard()
        {
            Log.Info("Rebuilding Leader Board");
            var leader_boards = Database.Singleton.DB.GetCollection<Database.LeaderBoard>("leader_board");
            var users = Database.Singleton.DB.GetCollection<Database.User>("users");
            var tracking = Database.Singleton.DB.GetCollection<Database.Tracking>("tracking");
            var sessions = Database.Singleton.DB.GetCollection<Database.Session>("sessions");
            var lives = Database.Singleton.DB.GetCollection<Database.Life>("lives");

            Log.Info("deleted: " + leader_boards.DeleteAll());
            leader_boards.EnsureIndex(x => x.UserId);
            users.EnsureIndex(x => x.UserId);
            tracking.EnsureIndex(x => x.TrackingId);
            sessions.EnsureIndex(x => x.SessionId);
            lives.EnsureIndex(x => x.LifeId);

            var all_users = users.Include(x=>x.tracking).FindAll();
            Log.Info("users: " + all_users.Count());
            foreach (var user in all_users)
            {
                Database.LeaderBoard lb = new Database.LeaderBoard { UserId = user.UserId };
                var tracker = tracking.Include(x => x.sessions).FindById(user.tracking.TrackingId);
                if (tracker != null && tracker.sessions != null)
                {
                    foreach (var session in tracker.sessions)
                    {
                        if (config.BeginEpoch < session.connect && session.connect < config.EndEpoch)
                        {
                            lb.total_play_time += (int)(session.disconnect - session.connect).TotalSeconds;
                            foreach (var life_id in session.lives)
                            {
                                var life = lives.Include(x => x.loadout).FindById(life_id.LifeId);
                                if (life != null && life.kills != null)
                                {
                                    int killstreak = 0;
                                    foreach (var kill in life.kills)
                                    {
                                        if (life.death == null || kill.KillId != life.death.KillId)
                                        {
                                            lb.total_kills++;
                                            killstreak++;
                                        }
                                    }
                                    if (killstreak > lb.highest_killstreak)
                                    {
                                        lb.highest_killstreak = killstreak;
                                        lb.killstreak_tag = life.loadout.killstreak_mode;
                                    }
                                }
                            }
                        }
                    }
                }
                leader_boards.Insert(lb);

                //foreach (var session_ref in tracker.sessions)
                //{
                //    if (config.BeginEpoch < session_ref.connect && session_ref.connect < config.EndEpoch)
                //    {
                //        var session = sessions.Include(x => x.lives).FindById(session_ref.SessionId);
                //        lb.total_play_time += (int)(session.disconnect - session.connect).TotalSeconds;
                //        foreach (var life_ref in session.lives)
                //        {
                //            var life = lives.Include(x => x.kills).Include(x => x.loadout).Include(x => x.death).FindById(life_ref.LifeId);
                //            foreach (var life in session.lives)
                //            {
                //                int killstreak = 0;
                //                foreach (var kill in life.kills)
                //                {
                //                    if (kill.KillId != life.death.KillId)
                //                    {
                //                        lb.total_kills++;
                //                        killstreak++;
                //                    }
                //                }
                //                if (killstreak > lb.highest_killstreak)
                //                {
                //                    lb.highest_killstreak = killstreak;
                //                    lb.killstreak_tag = life.loadout.killstreak_mode;
                //                }
                //            }
                //        }
                //    }
                //}
                //leader_boards.Insert(lb);
            }
        }

        private List<int> SortIndex<T>(Func<PlayerDetails,T> key_selector)
        {
            return player_details.Select((x, i) => new KeyValuePair<PlayerDetails, int>(x, i)).OrderByDescending(x => key_selector(x.Key)).Select(x => x.Value).ToList();
        }

        //private void RefreshRankList()
        //{
        //    var rank_collection = Database.Singleton.DB.GetCollection<Database.Rank>();
        //    rank_collection.EnsureIndex(x => x.rating);
        //    IEnumerable<Database.Rank> db_ranks = rank_collection.Find(Query.All(Query.Descending), limit: 100);

        //    leader_boards[LeaderBoardType.Rank].Clear();
        //    foreach(var rank in db_ranks)
        //    {
        //        leader_boards[LeaderBoardType.Rank].Add(rank.UserId);
        //        PlayerDetails details = GetDetails(rank.UserId);
        //        RankInfo info = Ranks.Singleton.GetInfo(rank.rating);
        //        details.rank_rating = rank.rating;
        //        details.rank_tag = info.Tag;
        //        details.rank_color = info.Color;
        //    }
        //}

        //private void RefreshXpList()
        //{
        //    var xp_collection = Database.Singleton.DB.GetCollection<Database.Experience>();
        //    ulong level_stride = (ulong)Experiences.Singleton.MaxLevelXp();
        //    ulong stage_stride = level_stride * (ulong)Experiences.Singleton.config.StageTags.Count;
        //    ulong tier_stride = stage_stride * (ulong)Experiences.Singleton.config.TierTags.Count;
        //    xp_collection.EnsureIndex(x => (ulong)x.tier * tier_stride + (ulong)x.stage * stage_stride + (ulong)x.level * level_stride + (ulong)x.value);
        //    IEnumerable<Database.Experience> db_xps = xp_collection.Find(Query.All(Query.Descending), limit: 100);

        //    leader_boards[LeaderBoardType.Experience].Clear();
        //    foreach(var xp in db_xps)
        //    {
        //        leader_boards[LeaderBoardType.Experience].Add(xp.UserId);
        //        GetDetails(xp.UserId).xp_tag = Experiences.Singleton.XpString(new Experiences.XP { tier = xp.tier, stage = xp.stage, level = xp.level, value = xp.value });
        //    }
        //}

        //struct UserValue 
        //{
        //    public string id;
        //    public int value;
        //}

        //private void RefreshKillsList()
        //{
        //    //probably as inefficient as it looks
        //    var user_collection = Database.Singleton.DB.GetCollection<Database.User>();
        //    user_collection.Include(x => x.tracking).
        //        Include(x => x.tracking.sessions).
        //        Include(x => x.tracking.sessions[0].lives).
        //        Include(x => x.tracking.sessions[0].lives[0].kills).
        //        EnsureIndex(user => user.tracking.sessions.Sum(session => session.lives.Sum(life => life.kills.Sum(kill => kill == life.death ? 0 : 1))));
        //    IEnumerable<UserValue> db_total_kills = user_collection.
        //        Find(Query.All(Query.Descending), limit: 100).
        //        Select(user => new UserValue { id = user.UserId, value = user.tracking.sessions.Sum(session => session.lives.Sum(life => life.kills.Sum(kill => kill == life.death ? 0 : 1))) });

        //    leader_boards[LeaderBoardType.Kills].Clear();
        //    foreach(var uv in db_total_kills)
        //    {
        //        leader_boards[LeaderBoardType.Kills].Add(uv.id);
        //        GetDetails(uv.id).total_kills = uv.value;
        //    }
        //}

        //struct KillstreakValue : IComparable<KillstreakValue>
        //{
        //    public int killstreak;
        //    public string tag;

        //    public int CompareTo(KillstreakValue other)
        //    {
        //        return killstreak.CompareTo(other.killstreak);
        //    }
        //}

        //struct UserKillstreak
        //{
        //    public string id;
        //    public KillstreakValue value;
        //}

        //private void RefreshKillstreakList()
        //{
        //    var user_collection = Database.Singleton.DB.GetCollection<Database.User>();
        //    user_collection.Include(x => x.tracking).
        //        Include(x => x.tracking.sessions).
        //        Include(x => x.tracking.sessions[0].lives).
        //        Include(x => x.tracking.sessions[0].lives[0].kills).
        //        Include(x => x.tracking.sessions[0].lives[0].loadout).
        //        EnsureIndex(user => user.tracking.sessions.Max(session => session.lives.Max(life => life.kills.Sum(kill => kill == life.death ? 0 : 1))));
        //    IEnumerable<UserKillstreak> db_killstreak = user_collection.
        //        Find(Query.All(Query.Descending), limit: 100).
        //        Select(user => new UserKillstreak { id = user.UserId, value = user.tracking.sessions.Max(session => session.lives.Max(life => new KillstreakValue { killstreak = life.kills.Sum(kill => kill == life.death ? 0 : 1), tag = life.loadout.killstreak_mode })) });

        //    leader_boards[LeaderBoardType.Killstreak].Clear();
        //    foreach(var uv in db_killstreak)
        //    {
        //        leader_boards[LeaderBoardType.Killstreak].Add(uv.id);
        //        PlayerDetails details = GetDetails(uv.id);
        //        details.highest_killstreak = uv.value.killstreak;
        //        details.highest_killstreak_tag = uv.value.tag;
        //    }
        //}

        //private void RefreshTotalPlayTimeList()
        //{
        //    var user_collection = Database.Singleton.DB.GetCollection<Database.User>();
        //    user_collection.Include(x => x.tracking).
        //        Include(x => x.tracking.sessions).
        //        EnsureIndex(user => user.tracking.sessions.Sum(session => (session.disconnect - session.connect).TotalSeconds));
        //    IEnumerable<UserValue> db_playtime = user_collection.
        //        Find(Query.All(Query.Descending), limit: 100).
        //        Select(user => new UserValue { id = user.UserId, value = (int)user.tracking.sessions.Sum(session => (session.disconnect - session.connect).TotalSeconds) });

        //    leader_boards[LeaderBoardType.Time].Clear();
        //    foreach(var uv in db_playtime)
        //    {
        //        leader_boards[LeaderBoardType.Time].Add(uv.id);
        //        GetDetails(uv.id).total_play_time = uv.value;
        //    }
        //}

        //private void RefreshPlayerNames()
        //{
        //    var user_collection = Database.Singleton.DB.GetCollection<Database.User>();
        //    user_collection.EnsureIndex(x => x.UserId);
        //    foreach (var detail in player_details)
        //    {
        //        detail.name = user_collection.Query().
        //            Include(x=>x.tracking).
        //            Include(x=>x.tracking.sessions).
        //            Select(user => user.tracking.sessions.Last().nickname).FirstOrDefault();
        //    }
        //}

        //private PlayerDetails GetDetails(string user_id)
        //{
        //    if (!player_cache.ContainsKey(user_id))
        //        player_cache.Add(user_id, new PlayerDetails());
        //    return player_cache[user_id];
        //}

    }
}
