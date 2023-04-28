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
    class DataBase
    {
        //configs collection
        class Config
        {
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
            public byte health { get; set; }
            public byte damage { get; set; }
            public byte hitbox { get; set; }
            public byte weapon { get; set; }
        }

        class Loadout
        {
            public long LoadoutId { get; set; }
            [BsonRef("lives")]
            public Session owner;
            public string killstreak_mode { get; set; } = "Standard";
            public ItemType primary { get; set; } = ItemType.None;
            public int primary_attachment_code;
            public ItemType secondary { get; set; } = ItemType.None;
            public int secondary_attachment_code;
            public ItemType tertiary { get; set; } = ItemType.None;
            public int tertiary_attachment_code;
        }

        class Kill
        {
            public long KillId { get; set; }
            [BsonRef("lives")]
            public Life victim;
            [BsonRef("lives")]
            public Life killer;
            public int time = 0;
            public HitboxType hitbox;
            public ItemType weapon;
            public int attachment_code;
        }

        class Life
        {
            public long LifeId { get; set; }
            [BsonRef("session")]
            public Session session;
            public RoleTypeId role { get; set; } = RoleTypeId.ClassD;
            public int shots = 0;
            public int time = 0;
            [BsonRef("loadouts")]
            public Loadout loadout;
            [BsonRef("kills")]
            public List<Kill> kills = new List<Kill>();
            [BsonRef("hits")]
            public List<Hit> delt = new List<Hit>();
            [BsonRef("hits")]
            public List<Hit> received = new List<Hit>();
            [BsonRef("kills")]
            public Kill death = new Kill();
        }

        class Round
        {
            public long RoundId { get; set; }
            public System.DateTime start;
            public System.DateTime end;
            public int max_players;
            public int average_players;
        }

        class Session
        {
            public long SessionId { get; set; }
            [BsonRef("tracking")]
            public Tracking tracking;
            public string nickname = "unnamed";
            public System.DateTime connect;
            public System.DateTime disconnect;
            [BsonRef("rounds")]
            public Round round;
            [BsonRef("lives")]
            public List<Life> lives = new List<Life>();
        }

        class Tracking
        {
            public long TrackingId { get; set; }
            [BsonRef("users")]
            public User user { get; set; }
            [BsonRef("sessions")]
            public List<Session> sessions { get; set; } = new List<Session>();
        }

        class User
        {
            public string UserId { get; set; }
            [BsonRef("tracking")]
            public Tracking tracking { get; set; }
        }

        //ranks collection
        class Rank
        {
            public string UserId { get; set; }
            public float rating { get; set; }
            public float rd { get; set; }
            public float rv { get; set; }
        }

        //experience collecion
        class Experience
        {
            public string UserId { get; set; }
            public int value { get; set; } = 0;
            public int stage { get; set; } = 0;
            public int tier { get; set; } = 0;
        }

        public static DataBase Singleton { get; private set; }

        private LiteDatabase db;

        public DataBase()
        {
            Singleton = this;
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
            db = new LiteDatabase(@".config/SCP Secret Laboratory/PluginAPI/plugins/" + ServerStatic.ServerPort.ToString() + "/Deathmatch.db");
        }

        public void UnLoad()
        {
            db.Dispose();
        }

        public void LoadConfig(Player player)
        {
            Loadouts.Loadout loadout = Loadouts.GetLoadout(player);
            Lobby.Spawn spawn = Lobby.GetSpawn(player);
            Killstreaks.Killstreak killstreak = Killstreaks.GetKillstreak(player);
            new Task(() =>
            {
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
            }).Start();

        }

        public void SaveConfig(Player player)
        {
            Loadouts.Loadout loadout = Loadouts.GetLoadout(player);
            Lobby.Spawn spawn = Lobby.GetSpawn(player);
            Killstreaks.Killstreak killstreak = Killstreaks.GetKillstreak(player);
            new Task(() =>
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
            }).Start();
        }

        public void LoadRank(Player player)
        {
            Ranks.Rank player_rank = Ranks.Singleton.GetRank(player);
            new Task(() =>
            {
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
            }).Start();
        }

        public void SaveRank(string user_id, float rating, float rd, float rv)
        {
            new Task(() =>
            {
                var ranks = db.GetCollection<Rank>("ranks");
                ranks.EnsureIndex(x => x.UserId);
                Rank rank = new Rank { UserId = user_id };
                rank.rating = rating;
                rank.rd = rd;
                rank.rv = rv;
                ranks.Upsert(rank);
            }).Start();
        }

        public void LoadExperience(Player player)
        {
            Experiences.XP player_xp = Experiences.Singleton.GetXP(player);
            new Task(() =>
            {
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
                            player_xp.stage = xp.stage;
                            player_xp.tier = xp.tier;
                        });
                    }
                }
            }).Start();
        }

        public void SaveExperience(Player player)
        {
            Experiences.XP player_xp = Experiences.Singleton.GetXP(player);
            new Task(() =>
            {
                if (!player.DoNotTrack)
                {
                    var experiences = db.GetCollection<Experience>("experiences");
                    experiences.EnsureIndex(x => x.UserId);
                    Experience xp = new Experience { UserId = player.UserId };
                    xp.value = player_xp.value;
                    xp.stage = player_xp.stage;
                    xp.tier = player_xp.tier;
                    experiences.Upsert(xp);
                }
            }).Start();
        }


        //public class PlayerRecord
        //{
        //    public string PlayerRecordId { get; set; }

        //    //loadout
        //    public ItemType primary { get; set; } = ItemType.None;
        //    public ItemType secondary { get; set; } = ItemType.None;
        //    public ItemType tertiary { get; set; } = ItemType.None;
        //    public bool radio { get; set; } = true;
        //    public bool rage_enabled { get; set; } = false;

        //    //spawn
        //    public RoleTypeId role { get; set; } = RoleTypeId.ClassD;

        //    //killstreak
        //    public Killstreaks.KillstreakMode killstreak_mode { get; set; } = Killstreaks.KillstreakMode.Standard;

        //}

        //public class FetchResult
        //{
        //    public Player player;
        //    public PlayerRecord record;

        //    public FetchResult(Player player, PlayerRecord record)
        //    {
        //        this.player = player;
        //        this.record = record;
        //    }
        //}

        //static LiteDatabase database = new LiteDatabase(@".config/SCP Secret Laboratory/PluginAPI/plugins/" + ServerStatic.ServerPort.ToString() + "/Deathmatch.db");
        //static List<Task<FetchResult>> fetches = new List<Task<FetchResult>>();
        //static CoroutineHandle fetch_handle;

        //public static void PluginUnload()
        //{
        //    database.Dispose();
        //}

        //[PluginEvent(ServerEventType.RoundStart)]
        //void OnRoundStart()
        //{
        //    fetch_handle = Timing.RunCoroutine(_CheckFetches());
        //}

        //[PluginEvent(ServerEventType.RoundEnd)]
        //void OnRoundEnd(RoundSummary.LeadingTeam leadingTeam)
        //{
        //    Timing.KillCoroutines(fetch_handle);
        //}

        //[PluginEvent(ServerEventType.PlayerJoined)]
        //void OnPlayerJoined(Player player)
        //{
        //    Timing.CallDelayed(0.0f, () =>
        //    {
        //        if (player.DoNotTrack)
        //        {
        //            player.ReceiveHint("<color=#FF0000><size=60>Warning!</color> <color=#43BFF0>Loadout/Preferences cant be saved when you have</color> <color=#FF0000>Do Not Track</color> <color=#43BFF0>enabled.\n you can disable</color> <color=#FF0000>Do Not Track</color> <color=#43BFF0>in you settings</size></color>", 30.0f);
        //            Task delete_record = new Task(() =>
        //            {
        //                var collection = database.GetCollection<PlayerRecord>("players");
        //                collection.EnsureIndex(x => x.PlayerRecordId);
        //                collection.Delete(player.UserId);
        //            });
        //            delete_record.Start();
        //        }
        //        else
        //        {
        //            fetches.Add(new Task<FetchResult>(() =>
        //            {
        //                var collection = database.GetCollection<PlayerRecord>("players");
        //                collection.EnsureIndex(x => x.PlayerRecordId);
        //                PlayerRecord result = collection.FindById(player.UserId);
        //                if (result is null)
        //                {
        //                    result = new PlayerRecord();
        //                    result.PlayerRecordId = player.UserId;
        //                    collection.Insert(result);
        //                }
        //                return new FetchResult(player, result);
        //            }));
        //            fetches.Last().Start();
        //        }
        //    });
        //}

        //public IEnumerator<float> _CheckFetches()
        //{
        //    while(true)
        //    {
        //        foreach(Task<FetchResult> task in fetches)
        //        {
        //            if(task.IsCompleted)
        //            {
        //                FetchResult result = task.Result;
        //                Loadouts.Loadout loadout = Loadouts.GetLoadout(result.player);
        //                Lobby.Spawn spawn = Lobby.GetSpawn(result.player);
        //                Killstreaks.Killstreak killstreak = Killstreaks.GetKillstreak(result.player);
        //                loadout.primary = result.record.primary;
        //                loadout.secondary = result.record.secondary;
        //                loadout.tertiary = result.record.tertiary;
        //                loadout.radio = result.record.radio;
        //                loadout.rage_mode_enabled = result.record.rage_enabled;
        //                spawn.role = result.record.role;
        //                killstreak.mode = result.record.killstreak_mode;
        //            }
        //        }
        //        fetches.RemoveAll(t => t.IsCompleted);

        //        yield return Timing.WaitForOneFrame;
        //    }
        //}

        //[PluginEvent(ServerEventType.PlayerLeft)]
        //void OnPlayerLeft(Player player)
        //{
        //    if (Loadouts.player_loadouts.ContainsKey(player.PlayerId))
        //    {
        //        if (!player.DoNotTrack)
        //        {
        //            Loadouts.Loadout loadout = Loadouts.GetLoadout(player);
        //            PlayerRecord record = new PlayerRecord();
        //            record.PlayerRecordId = player.UserId;
        //            record.primary = loadout.primary;
        //            record.secondary = loadout.secondary;
        //            record.tertiary = loadout.tertiary;
        //            record.radio = loadout.radio;
        //            record.rage_enabled = loadout.rage_mode_enabled;
        //            record.role = Lobby.GetSpawn(player).role;
        //            record.killstreak_mode = Killstreaks.GetKillstreak(player).mode;

        //            Task save = new Task(() =>
        //            {
        //                var col = database.GetCollection<PlayerRecord>("players");
        //                col.EnsureIndex(x => x.PlayerRecordId);
        //                var result = col.FindById(player.UserId);
        //                if (result is null)
        //                {
        //                    PlayerRecord r = new PlayerRecord();
        //                    r.PlayerRecordId = player.UserId;
        //                    col.Insert(r);
        //                }
        //                col.Update(record);
        //            });
        //            save.Start();
        //        }
        //    }
        //}
    }
}
