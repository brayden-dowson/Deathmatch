using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using MEC;
using PlayerRoles;

namespace TheRiptide
{
    public class Database
    {
        //configs collection
        class Config
        {
            [BsonId]
            public string UserId { get; set; }
            //loadout
            public ItemType primary { get; set; } = ItemType.None;
            public ItemType secondary { get; set; } = ItemType.None;
            public ItemType tertiary { get; set; } = ItemType.None;
            public bool rage_enabled { get; set; } = false;

            //spawn
            public RoleTypeId role { get; set; } = RoleTypeId.ClassD;

            //killstreak
            public string killstreak_mode { get; set; } = "";
        }

        //users collection
        public class Hit
        {
            public long HitId { get; set; }
            public byte health { get; set; } = 0;
            public byte damage { get; set; } = 0;
            public byte hitbox { get; set; } = 0;
            public byte weapon { get; set; } = 0;
        }

        public class Loadout : System.IEquatable<Loadout>
        {
            public long LoadoutId { get; set; }
            public string killstreak_mode { get; set; } = "";
            public ItemType primary { get; set; } = ItemType.None;
            public uint primary_attachment_code { get; set; } = 0;
            public ItemType secondary { get; set; } = ItemType.None;
            public uint secondary_attachment_code { get; set; } = 0;
            public ItemType tertiary { get; set; } = ItemType.None;
            public uint tertiary_attachment_code { get; set; } = 0;

            public bool Equals(Loadout other)
            {
                return killstreak_mode == other.killstreak_mode &&
                    primary == other.primary &&
                    primary_attachment_code == other.primary_attachment_code &&
                    secondary == other.secondary &&
                    secondary_attachment_code == other.secondary_attachment_code &&
                    tertiary == other.tertiary &&
                    tertiary_attachment_code == other.tertiary_attachment_code;
            }
        }

        public class Kill
        {
            public long KillId { get; set; }
            public float time { get; set; } = UnityEngine.Time.time;
            public HitboxType hitbox { get; set; } = HitboxType.Body;
            public ItemType weapon { get; set; } = ItemType.None;
            public uint attachment_code { get; set; } = 0;
        }

        public class Life
        {
            public long LifeId { get; set; }
            public RoleTypeId role { get; set; } = RoleTypeId.ClassD;
            public int shots { get; set; } = 0;
            public float time { get; set; } = UnityEngine.Time.time;
            [BsonRef("loadouts")]
            public Loadout loadout { get; set; } = null;
            [BsonRef("kills")]
            public List<Kill> kills { get; set; } = new List<Kill>();
            [BsonRef("hits")]
            public List<Hit> delt { get; set; } = new List<Hit>();
            [BsonRef("hits")]
            public List<Hit> received { get; set; } = new List<Hit>();
            [BsonRef("kills")]
            public Kill death { get; set; } = null;
        }

        public class Round
        {
            public long RoundId { get; set; }
            public System.DateTime start { get; set; } = System.DateTime.Now;
            public System.DateTime end { get; set; } = System.DateTime.Now;
            public int max_players { get; set; } = 0;
        }

        public class Session
        {
            public long SessionId { get; set; }
            public string nickname { get; set; } = "*unconnected";
            public System.DateTime connect { get; set; } = System.DateTime.Now;
            public System.DateTime disconnect { get; set; } = System.DateTime.Now;
            [BsonRef("rounds")]
            public Round round { get; set; } = null;
            [BsonRef("lives")]
            public List<Life> lives { get; set; } = new List<Life>();
        }

        public class Tracking
        {
            public long TrackingId { get; set; }
            [BsonRef("sessions")]
            public List<Session> sessions { get; set; } = new List<Session>();
        }

        public class User
        {
            public string UserId { get; set; }
            [BsonRef("tracking")]
            public Tracking tracking { get; set; } = new Tracking();
        }


        //ranks collection
        public enum RankState { Unranked, Placement, Ranked };
        public class Rank
        {
            [BsonId]
            public string UserId { get; set; }
            public RankState state { get; set; } = RankState.Unranked;
            public int placement_matches { get; set; } = 0;
            public float rating { get; set; } = 0;
            public float rd { get; set; } = 0;
            public float rv { get; set; } = 0;
        }

        //experience collecion
        class Experience
        {
            [BsonId]
            public string UserId { get; set; }
            public int value { get; set; } = 0;
            public int level { get; set; } = 0;
            public int stage { get; set; } = 0;
            public int tier { get; set; } = 0;
        }

        private static Database instance = null;
        public static Database Singleton
        { 
            get 
            {
                if (instance == null)
                    instance = new Database();
                return instance;
            }
        }

        private LiteDatabase db;

        private Database()
        {
            BsonMapper.Global.RegisterType
            (
                serialize: (Hit h) =>
                {
                    BsonValue doc = new BsonDocument();
                    doc["_id"] = h.HitId;
                    doc["data"] = System.BitConverter.ToInt32(new byte[] { h.health, h.damage, h.hitbox, h.weapon }, 0);
                    return doc;
                },
                deserialize: (BsonValue value) =>
                {
                    BsonDocument doc = value.AsDocument;
                    byte[] data = System.BitConverter.GetBytes(doc["data"].AsInt32);
                    return new Hit { HitId = doc["_id"], health = data[0], damage = data[1], hitbox = data[2], weapon = data[4] };
                }
            );
        }

        public void Load(string config_path)
        {
            db = new LiteDatabase(config_path.Replace("config.yml", "") + "Deathmatch.db");
        }

        public void UnLoad()
        {
            db.Dispose();
        }

        public void Checkpoint()
        {
            DbAsync(() => db.Checkpoint());
        }

        public void LoadConfig(Player player)
        {
            DbDelayedAsync(() =>
            {
                Loadouts.Loadout loadout = Loadouts.GetLoadout(player);
                Lobby.Spawn spawn = Lobby.Singleton.GetSpawn(player);
                Killstreaks.Killstreak killstreak = Killstreaks.GetKillstreak(player);

                var configs = db.GetCollection<Config>("configs");
                configs.EnsureIndex(x => x.UserId);
                if (!player.DoNotTrack)
                {
                    Config config = configs.FindById(player.UserId);
                    if (config != null)
                    {
                        Timing.CallDelayed(0.0f, () =>
                        {
                            loadout.primary = config.primary;
                            loadout.secondary = config.secondary;
                            loadout.tertiary = config.tertiary;
                            loadout.rage_mode_enabled = config.rage_enabled;
                            spawn.role = config.role;
                            killstreak.name = config.killstreak_mode;
                            Killstreaks.Singleton.KillstreakLoaded(player);
                        });
                    }
                }
                else
                {
                    configs.Delete(player.UserId);
                }
            });
        }

        class ConfigRef
        {
            public Loadouts.Loadout loadout = null;
            public Lobby.Spawn spawn = null;
            public Killstreaks.Killstreak killstreak = null;

            public bool IsReady { get { return loadout != null && spawn != null && killstreak != null; } }
        }

        Dictionary<int, ConfigRef> config_cache = new Dictionary<int, ConfigRef>();

        public void SaveConfigLoadout(Player player)
        {
            Loadouts.Loadout loadout = Loadouts.GetLoadout(player);
            if (!config_cache.ContainsKey(player.PlayerId))
                config_cache.Add(player.PlayerId, new ConfigRef { loadout = loadout });
            else
                config_cache[player.PlayerId].loadout = loadout;
            if (config_cache[player.PlayerId].IsReady)
                SaveConfig(player);
        }

        public void SaveConfigSpawn(Player player)
        {
            Lobby.Spawn spawn = Lobby.Singleton.GetSpawn(player);
            if (!config_cache.ContainsKey(player.PlayerId))
                config_cache.Add(player.PlayerId, new ConfigRef { spawn = spawn });
            else
                config_cache[player.PlayerId].spawn = spawn;
            if (config_cache[player.PlayerId].IsReady)
                SaveConfig(player);
        }

        public void SaveConfigKillstreak(Player player)
        {
            Killstreaks.Killstreak killstreak = Killstreaks.GetKillstreak(player);
            if (!config_cache.ContainsKey(player.PlayerId))
                config_cache.Add(player.PlayerId, new ConfigRef { killstreak = killstreak });
            else
                config_cache[player.PlayerId].killstreak = killstreak;
            if (config_cache[player.PlayerId].IsReady)
                SaveConfig(player);
        }

        private void SaveConfig(Player player)
        {
            ConfigRef config_ref = config_cache[player.PlayerId];
            config_cache.Remove(player.PlayerId);
            Loadouts.Loadout loadout = config_ref.loadout;
            Lobby.Spawn spawn = config_ref.spawn;
            Killstreaks.Killstreak killstreak = config_ref.killstreak;

            DbAsync(() =>
            {
                var configs = db.GetCollection<Config>("configs");
                configs.EnsureIndex(x => x.UserId);
                if (!player.DoNotTrack)
                {
                    Config config = new Config { UserId = player.UserId };
                    config.primary = loadout.primary;
                    config.secondary = loadout.secondary;
                    config.tertiary = loadout.tertiary;
                    config.rage_enabled = loadout.rage_mode_enabled;
                    config.role = spawn.role;
                    config.killstreak_mode = killstreak.name;
                    configs.Upsert(config);
                }
                else
                {
                    configs.Delete(player.UserId);
                }
            });
        }

        public void LoadRank(Player player)
        {
            DbDelayedAsync(() =>
            {
                if (!player.DoNotTrack)
                {
                    Rank player_rank = Ranks.Singleton.GetRank(player);
                    var ranks = db.GetCollection<Rank>("ranks");
                    ranks.EnsureIndex(x => x.UserId);
                    Rank rank = ranks.FindById(player.UserId);
                    Timing.CallDelayed(0.0f, () =>
                    {
                        try
                        {
                            if (rank != null)
                            {
                                player_rank.UserId = rank.UserId;
                                player_rank.state = rank.state;
                                player_rank.placement_matches = rank.placement_matches;
                                player_rank.rating = rank.rating;
                                player_rank.rd = rank.rd;
                                player_rank.rv = rank.rv;
                            }
                            Ranks.Singleton.RankLoaded(player);
                        }
                        catch (System.Exception ex)
                        {
                            Log.Error("database rank error: " + ex.ToString());
                        }
                    });
                }
            });
        }

        public void SaveRank(Rank rank)
        {
            DbAsync(() =>
            {
                var ranks = db.GetCollection<Rank>("ranks");
                ranks.EnsureIndex(x => x.UserId);
                ranks.Upsert(rank);
            });
        }

        public void LoadExperience(Player player)
        {
            DbDelayedAsync(() =>
            {
                if (!player.DoNotTrack)
                {
                    Experiences.XP player_xp = Experiences.Singleton.GetXP(player);
                    var experiences = db.GetCollection<Experience>("experiences");
                    experiences.EnsureIndex(x => x.UserId);
                    Experience xp = experiences.FindById(player.UserId);
                    Timing.CallDelayed(0.0f, () =>
                    {
                        try
                        {
                            if (xp != null)
                            {
                                player_xp.value = xp.value;
                                player_xp.level = xp.level;
                                player_xp.stage = xp.stage;
                                player_xp.tier = xp.tier;
                            }
                            Experiences.Singleton.XpLoaded(player);
                        }
                        catch(System.Exception ex)
                        {
                            Log.Error("database experience error: " + ex.ToString());
                        }
                    });
                }
            });
        }

        public void SaveExperience(Player player)
        {
            Experiences.XP player_xp = Experiences.Singleton.GetXP(player);
            DbAsync(() =>
            {
                if (!player.DoNotTrack)
                {
                    var experiences = db.GetCollection<Experience>("experiences");
                    experiences.EnsureIndex(x => x.UserId);
                    Experience xp = new Experience { UserId = player.UserId };
                    xp.value = player_xp.value;
                    xp.level = player_xp.level;
                    xp.stage = player_xp.stage;
                    xp.tier = player_xp.tier;
                    experiences.Upsert(xp);
                }
            });
        }

        public void SaveTrackingSession(Player player)
        {
            Session session = TheRiptide.Tracking.Singleton.GetSession(player);
            DbAsync(() =>
            {
                var hits = db.GetCollection<Hit>("hits");
                hits.EnsureIndex(x => x.HitId);
                var kills = db.GetCollection<Kill>("kills");
                kills.EnsureIndex(x => x.KillId);
                var loadouts = db.GetCollection<Loadout>("loadouts");
                loadouts.EnsureIndex(x => x.LoadoutId);
                var lives = db.GetCollection<Life>("lives");
                lives.EnsureIndex(x => x.LifeId);
                var rounds = db.GetCollection<Round>("rounds");
                rounds.EnsureIndex(x => x.RoundId);
                var sessions = db.GetCollection<Session>("sessions");
                sessions.EnsureIndex(x => x.SessionId);
                var tracking = db.GetCollection<Tracking>("tracking");
                tracking.EnsureIndex(x => x.TrackingId);
                var users = db.GetCollection<User>("users");
                users.EnsureIndex(x => x.UserId);

                foreach(Life life in session.lives)
                {
                    foreach (Hit hit in life.delt)
                        hits.Upsert(hit);

                    foreach (Hit hit in life.received)
                        hits.Upsert(hit);

                    foreach (Kill kill in life.kills)
                        kills.Upsert(kill);

                    if (life.death != null)
                        kills.Upsert(life.death);

                    if (life.loadout != null)
                        loadouts.Upsert(life.loadout);

                    lives.Upsert(life);
                }

                if (session.round != null)
                    rounds.Upsert(session.round);

                sessions.Upsert(session);

                if (!player.DoNotTrack)
                {
                    User user = users.Include(x => x.tracking).FindById(player.UserId);
                    if(user == null)
                        user = new User { UserId = player.UserId };
                    user.tracking.sessions.Add(session);
                    tracking.Upsert(user.tracking);
                    users.Upsert(user);
                }
                else
                {
                    Tracking player_tracking = player_tracking = new Tracking();
                    player_tracking.sessions.Add(session);
                    tracking.Upsert(player_tracking);
                }
            });
        }

        public void DeleteData(Player player)
        {
            DbAsync(() =>
            {
                var users = db.GetCollection<User>("users");
                users.Delete(player.UserId);
                var experiences = db.GetCollection<Experience>("experiences");
                experiences.Delete(player.UserId);
                var ranks = db.GetCollection<Rank>("ranks");
                ranks.Delete(player.UserId);
                var configs = db.GetCollection<Config>("configs");
                configs.Delete(player.UserId);
            });
        }

        private void DbAsync(System.Action action)
        {
            new Task(() =>
            {
                try
                {
                    lock (db)
                    {
                        action.Invoke();
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error("Database error: " + ex.ToString());
                }
            }).Start();
        }

        private void DbDelayedAsync(System.Action action)
        {
            Timing.CallDelayed(0.0f, () =>
            {
                new Task(() =>
                {
                    try
                    {
                        lock (db)
                        {
                            action.Invoke();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error("Database error: " + ex.ToString());
                    }
                }).Start();
            });
        }
    }
}
