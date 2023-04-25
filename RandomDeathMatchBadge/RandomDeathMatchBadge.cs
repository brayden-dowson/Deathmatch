using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using PluginAPI.Enums;

//highest kills - [KILLER]
//highest K/D - [ASSASIN]
//most killsteaks ended - [KILLSTREAK HUNTER]
//highest killstreak - [UNSTOPPABLE]
//highest accuracy - [AIMBOT]
//highest headshots% - [SNIPER]

namespace RandomDeathMatchBadge
{
    public class RandomDeathMatchBadge
    {
        SqliteConnection connection;

        [PluginEntryPoint("Random Death Match Prestige", "1.0", "needs no explanation", "The Riptide")]
        void EntryPoint()
        {
            EventManager.RegisterEvents(this);
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder();
            builder.DataSource = "prestige_database.db";

            try
            {
                connection = new SqliteConnection(builder.ConnectionString);
                connection.Open();
                SqliteCommand command = connection.CreateCommand();
                //command.CommandText = "DROP TABLE players";

                command.CommandText = @"
CREATE TABLE IF NOT EXISTS players(player_id INTERGER PRIMARY KEY AUTOINCREMENT, kills INTERGER, deaths INTERGER, play_time INTERGER);
CREATE TABLE IF NOT EXISTS user_id_to_player_id(user_id TEXT PRIMARY KEY, player_id INTERGER REFERENCES players(player_id));
CREATE TABLE IF NOT EXISTS aliases(alias_id INTERGER PRIMARY KEY AUTOINCREMENT, player_id INTERGER REFERENCES players(player_id), alias TEXT UNIQUE, used_for_unix_time INTERGER);
CREATE TABLE IF NOT EXISTS weapon_names(weapon_name_id INTERGER PRIMARY KEY AUTOINCREMENT, weapon_name TEXT UNIQUE);
CREATE TABLE IF NOT EXISTS weapons(weapon_id INTERGER PRIMARY KEY AUTOINCREMENT, weapon_name_id INTERGER REFERENCES weapon_names(weapon_name_id), aux_data TEXT);
CREATE TABLE IF NOT EXISTS loadouts(loadout_id INTERGER PRIMARY KEY AUTOINCREMENT, primary_weapon_id INTERGER REFERENCES weapons(weapon_id), secondary_weapon_id INTERGER REFERENCES weapons(weapon_id), tertiary_weapon_id INTERGER REFERENCES weapons(weapon_id));
CREATE TABLE IF NOT EXISTS rounds(round_id INTERGER PRIMARY KEY AUTOINCREMENT, round_start_time TEXT, round_length_seconds INTERGER, max_players INTERGER);
CREATE TABLE IF NOT EXISTS sessions(session_id INTERGER PRIMARY KEY AUTOINCREMENT, player_id INTERGER REFERENCES players(player_id), session_start_time TEXT, session_length_seconds INTERGER, alias_id INTERGER REFERENCES aliases(alias_id));
CREATE TABLE IF NOT EXISTS lives(life_id INTERGER PRIMARY KEY AUTOINCREMENT, session_id INTERGER REFERENCES sessions(session_id), round_id INTERGER REFERENCES rounds(round_id), loadout_id INTERGER REFERENCES loadouts(loadout_id));
CREATE TABLE IF NOT EXISTS kills(kill_id INTERGER PRIMARY KEY AUTOINCREMENT, round_seconds INTERGER, killer_id INTERGER REFERENCES lives(life_id), victim_id INTERGER REFERENCES lives(life_id), weapon_id INTERGER REFERENCES weapon(weapon_id));
CREATE TABLE IF NOT EXISTS damage(attacker_id INTERGER REFERENCES lives(life_id), victim_id INTERGER REFERENCES lives(life_id), weapon_id INTERGER REFERENCES weapon(weapon_id), hits INTERGER, damage INTERGER);

";
                command.ExecuteNonQuery();

            }
            catch (SqliteException ex)
            {
                ServerConsole.AddLog(ex.Message);
            }

        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            //player.ReferenceHub.serverRoles.Group.BadgeText = "test";
            ServerConsole.AddLog(player.UserId);
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
        }
    }
}
