using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TheRiptide.Utility;

namespace TheRiptide
{
    public class Tracking
    {
        public static Tracking Singleton { get; private set; }

        private DataBase.Round current_round = new DataBase.Round();
        private Dictionary<int, DataBase.Session> player_sessions = new Dictionary<int, DataBase.Session>();
        private Dictionary<int, DataBase.Life> player_life = new Dictionary<int, DataBase.Life>();

        public Tracking()
        {
            Singleton = this;
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        void WaitingForPlayers()
        {
            current_round = null;
        }

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            current_round = new DataBase.Round();
            foreach (var ids in player_sessions.Keys.ToList())
                player_sessions[ids].round = current_round;
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            int id = player.PlayerId;
            if (!player_sessions.ContainsKey(id))
                player_sessions.Add(id, new DataBase.Session());
            else
                player_sessions[player.PlayerId] = new DataBase.Session();

            DataBase.Session session = player_sessions[player.PlayerId];
            session.nickname = player.Nickname;
            session.round = current_round;
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            int id = player.PlayerId;
            if (player_sessions.ContainsKey(id))
            {
                player_sessions[player.PlayerId].disconnect = System.DateTime.Now;
                DataBase.Singleton.SaveTrackingSession(player);
                player_sessions.Remove(id);
            }

            if (player_life.ContainsKey(id))
                player_life.Remove(id);
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDeath(Player victim, Player killer, DamageHandlerBase damage)
        {
            if(victim != null && killer != null && player_life.ContainsKey(victim.PlayerId) && player_life.ContainsKey(killer.PlayerId))
            {
                DataBase.Life victim_life = player_life[victim.PlayerId];
                DataBase.Life killer_life = player_life[killer.PlayerId];
                DataBase.Kill kill = new DataBase.Kill();
                victim_life.death = kill;
                killer_life.kills.Add(kill);
                //kill.killer = killer_life;
                //kill.victim = victim_life;
                if(damage is StandardDamageHandler standard)
                    kill.hitbox = standard.Hitbox;
                kill.weapon = GetItemFromDamageHandler(damage);
                if (AttachmentsServerHandler.PlayerPreferences[killer.ReferenceHub].ContainsKey(kill.weapon))
                    kill.attachment_code = AttachmentsServerHandler.PlayerPreferences[killer.ReferenceHub][kill.weapon];
            }
        }

        [PluginEvent(ServerEventType.PlayerDamage)]
        void OnPlayerDamage(Player victim, Player attacker, DamageHandlerBase damage)
        {
            if (victim != null && attacker != null && player_life.ContainsKey(victim.PlayerId) && player_life.ContainsKey(attacker.PlayerId))
            {
                if (damage is StandardDamageHandler standard)
                {
                    DataBase.Life victim_life = player_life[victim.PlayerId];
                    DataBase.Life attacker_life = player_life[attacker.PlayerId];
                    DataBase.Hit hit = new DataBase.Hit();
                    victim_life.received.Add(hit);
                    attacker_life.delt.Add(hit);
                    hit.health = (byte)victim.Health;
                    hit.damage = (byte)standard.Damage;
                    hit.hitbox = (byte)standard.Hitbox;
                    hit.weapon = (byte)GetItemFromDamageHandler(damage);
                }
            }
        }

        [PluginEvent(ServerEventType.PlayerShotWeapon)]
        void OnPlayerShotWeapon(Player player, Firearm firearm)
        {
            if(player != null)
            {
                DataBase.Life life = player_life[player.PlayerId];
                if (life != null)
                    life.shots++;
            }
        }

        public void PlayerSpawn(Player player)
        {
            if(player != null && player_sessions.ContainsKey(player.PlayerId))
            {
                DataBase.Session session = player_sessions[player.PlayerId];
                DataBase.Life life = new DataBase.Life();
                DataBase.Loadout loadout = null;
                session.lives.Add(life);
                //life.session = session;
                if (player_life.ContainsKey(player.PlayerId))
                {
                    loadout = player_life[player.PlayerId].loadout;
                    player_life[player.PlayerId] = life;
                }
                else
                    player_life.Add(player.PlayerId, life);
                life.role = Lobby.GetSpawn(player).role;
                if (loadout == null)
                    loadout = new DataBase.Loadout();

                var weapon_attachments = AttachmentsServerHandler.PlayerPreferences[player.ReferenceHub];
                Loadouts.Loadout player_loadout = Loadouts.GetLoadout(player);
                DataBase.Loadout current = new DataBase.Loadout();
                current.LoadoutId = 0;
                //current.owner = null;
                current.killstreak_mode = Killstreaks.GetKillstreak(player).mode.ToString();
                current.primary = player_loadout.primary;
                if (weapon_attachments.ContainsKey(player_loadout.primary))
                    current.primary_attachment_code = weapon_attachments[player_loadout.primary];
                current.secondary = player_loadout.secondary;
                if (weapon_attachments.ContainsKey(player_loadout.secondary))
                    current.secondary_attachment_code = weapon_attachments[player_loadout.secondary];
                current.tertiary = player_loadout.tertiary;
                if (weapon_attachments.ContainsKey(player_loadout.tertiary))
                    current.tertiary_attachment_code = weapon_attachments[player_loadout.tertiary];
                long save_id = loadout.LoadoutId;
                loadout.LoadoutId = 0;
                //loadout.owner = null;

                if (current != loadout)
                    loadout = current;
                else
                    loadout.LoadoutId = save_id;

                life.loadout = loadout;
                //loadout.owner = life;
            }
        }

        public DataBase.Session GetSession(Player player)
        {
            return player_sessions[player.PlayerId];
        }

    }
}
