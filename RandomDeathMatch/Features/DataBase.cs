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
        public class PlayerRecord
        {
            public string PlayerRecordId { get; set; }

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

        public class FetchResult
        {
            public Player player;
            public PlayerRecord record;

            public FetchResult(Player player, PlayerRecord record)
            {
                this.player = player;
                this.record = record;
            }
        }

        static LiteDatabase database = new LiteDatabase(@".config/SCP Secret Laboratory/PluginAPI/plugins/" + ServerStatic.ServerPort.ToString() + "/Deathmatch.db");
        static List<Task<FetchResult>> fetches = new List<Task<FetchResult>>();
        static CoroutineHandle fetch_handle;

        public static void PluginUnload()
        {
            database.Dispose();
        }

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            fetch_handle = Timing.RunCoroutine(_CheckFetches());
        }

        [PluginEvent(ServerEventType.RoundEnd)]
        void OnRoundEnd(RoundSummary.LeadingTeam leadingTeam)
        {
            Timing.KillCoroutines(fetch_handle);
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            Timing.CallDelayed(0.0f, () =>
            {
                if (player.DoNotTrack)
                {
                    player.ReceiveHint("<color=#FF0000><size=60>Warning!</color> <color=#43BFF0>Loadout/Preferences cant be saved when you have</color> <color=#FF0000>Do Not Track</color> <color=#43BFF0>enabled.\n you can disable</color> <color=#FF0000>Do Not Track</color> <color=#43BFF0>in you settings</size></color>", 30.0f);
                    Task delete_record = new Task(() =>
                    {
                        var collection = database.GetCollection<PlayerRecord>("players");
                        collection.EnsureIndex(x => x.PlayerRecordId);
                        collection.Delete(player.UserId);
                    });
                    delete_record.Start();
                }
                else
                {
                    fetches.Add(new Task<FetchResult>(() =>
                    {
                        var collection = database.GetCollection<PlayerRecord>("players");
                        collection.EnsureIndex(x => x.PlayerRecordId);
                        PlayerRecord result = collection.FindById(player.UserId);
                        if (result is null)
                        {
                            result = new PlayerRecord();
                            result.PlayerRecordId = player.UserId;
                            collection.Insert(result);
                        }
                        return new FetchResult(player, result);
                    }));
                    fetches.Last().Start();
                }
            });
        }

        public IEnumerator<float> _CheckFetches()
        {
            while(true)
            {
                foreach(Task<FetchResult> task in fetches)
                {
                    if(task.IsCompleted)
                    {
                        FetchResult result = task.Result;
                        Loadouts.Loadout loadout = Loadouts.GetLoadout(result.player);
                        Lobby.Spawn spawn = Lobby.GetSpawn(result.player);
                        Killstreaks.Killstreak killstreak = Killstreaks.GetKillstreak(result.player);
                        loadout.primary = result.record.primary;
                        loadout.secondary = result.record.secondary;
                        loadout.tertiary = result.record.tertiary;
                        loadout.radio = result.record.radio;
                        loadout.rage_mode_enabled = result.record.rage_enabled;
                        spawn.role = result.record.role;
                        killstreak.mode = result.record.killstreak_mode;
                    }
                }
                fetches.RemoveAll(t => t.IsCompleted);

                yield return Timing.WaitForOneFrame;
            }
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            if (Loadouts.player_loadouts.ContainsKey(player.PlayerId))
            {
                if (!player.DoNotTrack)
                {
                    Loadouts.Loadout loadout = Loadouts.GetLoadout(player);
                    PlayerRecord record = new PlayerRecord();
                    record.PlayerRecordId = player.UserId;
                    record.primary = loadout.primary;
                    record.secondary = loadout.secondary;
                    record.tertiary = loadout.tertiary;
                    record.radio = loadout.radio;
                    record.rage_enabled = loadout.rage_mode_enabled;
                    record.role = Lobby.GetSpawn(player).role;
                    record.killstreak_mode = Killstreaks.GetKillstreak(player).mode;

                    Task save = new Task(() =>
                    {
                        var col = database.GetCollection<PlayerRecord>("players");
                        col.EnsureIndex(x => x.PlayerRecordId);
                        var result = col.FindById(player.UserId);
                        if (result is null)
                        {
                            PlayerRecord r = new PlayerRecord();
                            r.PlayerRecordId = player.UserId;
                            col.Insert(r);
                        }
                        col.Update(record);
                    });
                    save.Start();
                }
            }
        }
    }
}
