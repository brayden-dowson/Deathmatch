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
    public class DataBase
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
            public bool radio { get; set; } = true;
            public bool rage_enabled { get; set; } = false;

            //spawn
            public RoleTypeId role { get; set; } = RoleTypeId.ClassD;

            //killstreak
            public Killstreaks.KillstreakMode killstreak_mode { get; set; } = Killstreaks.KillstreakMode.Standard;
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

        public class Loadout
        {
            public long LoadoutId { get; set; }
            public string killstreak_mode { get; set; } = Killstreaks.KillstreakMode.Standard.ToString();
            public ItemType primary { get; set; } = ItemType.None;
            public uint primary_attachment_code { get; set; } = 0;
            public ItemType secondary { get; set; } = ItemType.None;
            public uint secondary_attachment_code { get; set; } = 0;
            public ItemType tertiary { get; set; } = ItemType.None;
            public uint tertiary_attachment_code { get; set; } = 0;
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
            public int average_players { get; set; } = 0;
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
        class Rank
        {
            [BsonId]
            public string UserId { get; set; }
            public float rating { get; set; }
            public float rd { get; set; }
            public float rv { get; set; }
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

        private static DataBase instance = null;
        public static DataBase Singleton
        { 
            get 
            {
                if (instance == null)
                    instance = new DataBase();
                return instance;
            }
        }

        private LiteDatabase db;

        private DataBase()
        {
            BsonMapper.Global.RegisterType
            (
                serialize: (Hit h) =>
                {
                    BsonDocument doc = new BsonDocument();
                    doc["hit_id"] = h.HitId;
                    doc["data"] = System.BitConverter.ToInt32(new byte[] { h.health, h.damage, h.hitbox, h.weapon }, 0);
                    return doc;
                },
                deserialize: (BsonValue value) =>
                {
                    BsonDocument doc = value.AsDocument;
                    Hit h = new Hit();
                    h.HitId = doc["hit_id"];
                    byte[] data = System.BitConverter.GetBytes(doc["data"].AsInt32);
                    h.health = data[0];
                    h.damage = data[1];
                    h.hitbox = data[2];
                    h.weapon = data[3];
                    return h;
                }
            );
        }

        public void Load()
        {
            db = new LiteDatabase(@".config/SCP Secret Laboratory/PluginAPI/plugins/" + ServerStatic.ServerPort.ToString() + "/Deathmatch/Deathmatch.db");
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
                Lobby.Spawn spawn = Lobby.GetSpawn(player);
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
                            loadout.radio = config.radio;
                            loadout.rage_mode_enabled = config.rage_enabled;
                            spawn.role = config.role;
                            killstreak.mode = config.killstreak_mode;
                        });
                    }
                }
                else
                {
                    configs.Delete(player.UserId);
                }
            });
        }

        public void SaveConfig(Player player)
        {
            Loadouts.Loadout loadout = Loadouts.GetLoadout(player);
            Lobby.Spawn spawn = Lobby.GetSpawn(player);
            Killstreaks.Killstreak killstreak = Killstreaks.GetKillstreak(player);

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
                    config.radio = loadout.radio;
                    config.rage_enabled = loadout.rage_mode_enabled;
                    config.role = spawn.role;
                    config.killstreak_mode = killstreak.mode;
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
                Ranks.Rank player_rank = Ranks.Singleton.GetRank(player);
                var ranks = db.GetCollection<Rank>("ranks");
                ranks.EnsureIndex(x => x.UserId);
                Rank rank = ranks.FindById(player.UserId);
                Timing.CallDelayed(0.0f, () =>
                {
                    if (rank != null)
                    {
                        player_rank.rating = rank.rating;
                        player_rank.rd = rank.rd;
                        player_rank.rv = rank.rv;
                    }
                    player_rank.loaded = true;
                });
            });
        }

        public void SaveRank(string user_id, float rating, float rd, float rv)
        {
            DbAsync(() =>
            {
                var ranks = db.GetCollection<Rank>("ranks");
                ranks.EnsureIndex(x => x.UserId);
                Rank rank = new Rank { UserId = user_id };
                rank.rating = rating;
                rank.rd = rd;
                rank.rv = rv;
                ranks.Upsert(rank);
            });
        }

        public void LoadExperience(Player player)
        {
            DbDelayedAsync(() =>
            {
                Experiences.XP player_xp = Experiences.Singleton.GetXP(player);
                if (!player.DoNotTrack)
                {
                    var experiences = db.GetCollection<Experience>("experiences");
                    experiences.EnsureIndex(x => x.UserId);
                    Experience xp = experiences.FindById(player.UserId);
                    if (xp != null)
                    {
                        Timing.CallDelayed(0.0f, () =>
                        {
                            player_xp.value = xp.value;
                            player_xp.level = xp.level;
                            player_xp.stage = xp.stage;
                            player_xp.tier = xp.tier;
                        });
                    }
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
