using InventorySystem.Items.Firearms;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using CustomPlayerEffects;
using PlayerRoles;

namespace TheRiptide
{
    class Statistics
    {
        public class LifeStats
        {
            public float damage = 0.0f;
            public int head_shots = 0;
            public int body_shots = 0;
            public int limb_shots = 0;
            public int other_hits = 0;
        }

        public class Stats
        {
            public int killstreak = 0;
            public int highest_killstreak = 0;
            public int kills = 0;
            public int headshot_kills = 0;
            public int deaths = 0;
            public int shots = 0;
            public int shots_hit = 0;
            public int headshots = 0;
            public int time_alive = 0;
            public int damage_delt = 0;
            public int damage_recieved = 0;

            public float start_time = 0;
        }

        public static Dictionary<int, Dictionary<int, LifeStats>> attacker_stats = new Dictionary<int, Dictionary<int, LifeStats>>();
        public static Dictionary<int, Dictionary<int, LifeStats>> victim_stats = new Dictionary<int, Dictionary<int, LifeStats>>();

        public static Dictionary<int, Stats> player_stats = new Dictionary<int, Stats>();

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            int id = player.PlayerId;
            if (!player_stats.ContainsKey(id))
                player_stats.Add(id, new Stats());

            if (!attacker_stats.ContainsKey(id))
                attacker_stats.Add(id, new Dictionary<int, LifeStats>());
            if (!victim_stats.ContainsKey(id))
                victim_stats.Add(id, new Dictionary<int, LifeStats>());
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            int id = player.PlayerId;
            if (player_stats.ContainsKey(id))
                player_stats.Remove(id);

            if (attacker_stats.ContainsKey(id))
                attacker_stats.Remove(id);
            if (victim_stats.ContainsKey(id))
                victim_stats.Remove(id);
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDeath(Player target, Player killer, DamageHandlerBase damage)
        {
            if (killer != null && killer.IsAlive && player_stats.ContainsKey(killer.PlayerId))
            {
                Stats stats = player_stats[killer.PlayerId];
                stats.kills++;
                if (damage is StandardDamageHandler standard)
                    if (standard.Hitbox == HitboxType.Headshot)
                        stats.headshot_kills++;
                stats.killstreak++;
                if (stats.killstreak > stats.highest_killstreak)
                    stats.highest_killstreak = stats.killstreak;
                if (stats.killstreak % 5 == 0)
                    BroadcastOverride.BroadcastLine(1, stats.killstreak, BroadcastPriority.Medium, "<b><color=#43BFF0>" + killer.Nickname + "</color></b> is on a <b><color=#FF0000>" + stats.killstreak.ToString() + "</color></b> kill streak");
                else
                    BroadcastOverride.BroadcastLine(killer, 2, 3, BroadcastPriority.Low, "Kill streak <b><color=#FF0000>" + stats.killstreak.ToString() + "</color></b>");
            }
            if (player_stats.ContainsKey(target.PlayerId))
            {
                Stats stats = player_stats[target.PlayerId];
                if (stats.killstreak >= 5)
                    BroadcastOverride.BroadcastLine(2, stats.killstreak, BroadcastPriority.Medium, "<b><color=#43BFF0>" + killer.Nickname + "</color></b> ended <b><color=#43BFF0>" + target.Nickname + "'s </color></b>" + "<b><color=#FF0000>" + stats.killstreak.ToString() + "</color></b> kill streak");

                stats.time_alive += (int)Math.Round(Time.time - stats.start_time);
                stats.killstreak = 0;
                stats.deaths++;
                BroadcastOverride.BroadcastLine(target, 1, 300, BroadcastPriority.Low, "<b><color=#FFFF00>Left/Right click to respawn</color></b>");
                BroadcastOverride.BroadcastLine(target, 2, 300, BroadcastPriority.Low, "<b><color=#FF0000>Tab to edit attachments/presets</color></b>");

                if(killer != null && player_stats.ContainsKey(killer.PlayerId))
                {
                    string hint = "\n<b>" + Killstreaks.KillstreakColorCode(killer) + killer.Nickname + "</color></b>" + " <color=#43BFF0>HP: " + killer.Health.ToString("0") + "</color>";
                    //if (killer.ArtificialHealth != 0)
                    //    hint += " <color=008f1c>AH: " + killer.ArtificialHealth + "</color>";

                    DamageReduction damage_reduction = null;
                    if (killer.EffectsManager.TryGetEffect(out damage_reduction))
                    {
                        if (damage_reduction.IsEnabled && damage_reduction.Intensity != 0)
                            hint += " <color=#5900ff> DR: " + (damage_reduction.Intensity / 2.0f).ToString("0.0") + "%</color>";
                    }

                    BodyshotReduction bodyshot_reduction = null;
                    if(killer.EffectsManager.TryGetEffect(out bodyshot_reduction))
                    {
                        if(bodyshot_reduction.IsEnabled && bodyshot_reduction.Intensity != 0)
                        {
                            float percentage = 0.0f;
                            if (bodyshot_reduction.Intensity == 1)
                                percentage = 5.0f;
                            else if (bodyshot_reduction.Intensity == 2)
                                percentage = 10.0f;
                            else if (bodyshot_reduction.Intensity == 3)
                                percentage = 12.5f;
                            else if (bodyshot_reduction.Intensity >= 4)
                                percentage = 15.0f;
                            hint += " <color=#e7d77b> BSR: " + percentage.ToString("0.0") + "%</color>";
                        }
                    }

                    hint += "\n";

                    if (attacker_stats[target.PlayerId].ContainsKey(killer.PlayerId))
                    {
                        LifeStats life_stats = attacker_stats[target.PlayerId][killer.PlayerId];
                        hint += "<color=#43BFF0> DMG: " + life_stats.damage.ToString("0") + "</color> <color=#FF0000>HS: " + life_stats.head_shots +
                            "</color> <color=#36a832>BS: " + life_stats.body_shots + "</color> <color=#43BFF0>LS: " +
                            life_stats.limb_shots + "</color>";
                        if (life_stats.other_hits != 0)
                            hint += " Other: " + life_stats.other_hits;
                    }

                    target.ReceiveHint(hint, 7.0f);

                }


                attacker_stats[target.PlayerId].Clear();
                foreach (var attacker_stats in victim_stats.Values)
                    attacker_stats.Remove(target.PlayerId);
            }
        }

        [PluginEvent(ServerEventType.PlayerDamage)]
        void OnPlayerDamage(Player victim, Player attacker, DamageHandlerBase damage)
        {
            bool valid_attacker = attacker != null && player_stats.ContainsKey(attacker.PlayerId);
            if (valid_attacker)
            {
                Stats stats = player_stats[attacker.PlayerId];
                stats.shots_hit++;
                if (damage is StandardDamageHandler standard)
                {
                    if (standard.Hitbox == HitboxType.Headshot)
                        stats.headshots++;
                    stats.damage_delt += (int)math.round(standard.Damage);
                }
            }

            bool valid_victim = victim != null && player_stats.ContainsKey(victim.PlayerId);
            if (valid_victim)
            {
                Stats stats = player_stats[victim.PlayerId];
                if (damage is StandardDamageHandler standard)
                {
                    stats.damage_recieved += (int)math.round(standard.Damage);
                }
            }

            if(valid_attacker && valid_victim)
            {
                int vid = victim.PlayerId;
                int aid = attacker.PlayerId;

                LifeStats life_stats = new LifeStats();
                if (!attacker_stats[aid].ContainsKey(vid))
                    attacker_stats[aid].Add(vid, life_stats);
                else
                    life_stats = attacker_stats[aid][vid];

                if (!victim_stats[vid].ContainsKey(aid))
                    victim_stats[vid].Add(aid, life_stats);
                else
                    life_stats = victim_stats[vid][aid];

                if (damage is FirearmDamageHandler firearm)
                {
                    life_stats.damage += firearm.Damage;
                    if (firearm.Hitbox == HitboxType.Headshot)
                        life_stats.head_shots++;
                    else if (firearm.Hitbox == HitboxType.Body)
                        life_stats.body_shots++;
                    else if (firearm.Hitbox == HitboxType.Limb)
                        life_stats.limb_shots++;
                    else
                        life_stats.other_hits++;
                }
                else if(damage is StandardDamageHandler standard)
                {
                    life_stats.damage += standard.DealtHealthDamage;
                    life_stats.other_hits++;
                }
            }
        }

        [PluginEvent(ServerEventType.PlayerShotWeapon)]
        void OnPlayerShotWeapon(Player player, Firearm firearm)
        {
            player_stats[player.PlayerId].shots++;
        }

        public static Stats GetStats(Player player)
        {
            return player_stats[player.PlayerId];
        }

        public static void SetPlayerStartTime(Player player, float time)
        {
            player_stats[player.PlayerId].start_time = time;
        }

        public static void DisplayStats(Player player, int line)
        {
            Stats stats = player_stats[player.PlayerId];
            float mins_alive = 60.0f / math.max(stats.time_alive, 300);
            float kd = (float)stats.kills / stats.deaths;
            float HsK = (float)stats.headshot_kills / stats.kills;
            float accuracy = (float)stats.shots_hit / stats.shots;
            float score = kd * HsK * accuracy / mins_alive;

            string stats_msg_1 = "<color=#76b8b5>Kills:</color> <color=#FF0000>" + stats.kills + "</color>    <color=#76b8b5>Deaths:</color> <color=#FF0000>" + stats.deaths + "</color>    <color=#76b8b5>K/D:</color> <color=#FF0000>" + kd.ToString("0.0") + "</color>    <color=#76b8b5>Highest Killstreak:</color> <color=#FF0000>" + stats.highest_killstreak + "</color>" + "</color>    <color=#76b8b5>Score:</color> <color=#FF0000>" + score.ToString("0.0") + "</color>";
            string stats_msg_2 = "<color=#76b8b5>Hs Kills:</color> <color=#FF0000>" + (HsK * 100.0f).ToString("0") + "%</color>    <color=#76b8b5>Hs:</color> <color=#FF0000>" + (((float)stats.headshots / stats.shots_hit) * 100.0f).ToString("0.0") + "%</color>    <color=#76b8b5>Accuracy:</color> <color=#FF0000>" + (accuracy * 100.0f).ToString("0") + "%</color>    <color=#76b8b5>Dmg Delt:</color> <color=#FF0000>" + stats.damage_delt + "</color>    <color=#76b8b5>Dmg Taken:</color> <color=#FF0000>" + stats.damage_recieved + "</color>";
            BroadcastOverride.BroadcastLine(player, line, 300, BroadcastPriority.Highest, stats_msg_1);
            BroadcastOverride.BroadcastLine(player, line + 1, 300, BroadcastPriority.Highest, stats_msg_2);
        }

        public static void DisplayRoundStats()
        {
            int highest_killstreak = 0;
            string highest_killstreak_name = "N/A";

            int most_kills = 0;
            string most_kills_name = "N/A";

            float most_score = 0.0f;
            string best_player_name = "N/A";

            foreach (Player player in Player.GetPlayers())
            {
                Stats stats = player_stats[player.PlayerId];
                if (stats.highest_killstreak > highest_killstreak)
                {
                    highest_killstreak = stats.highest_killstreak;
                    highest_killstreak_name = player.Nickname;
                }

                if (stats.kills > most_kills)
                {
                    most_kills = stats.kills;
                    most_kills_name = player.Nickname;
                }

                float mins_alive = 60.0f / math.max(stats.time_alive, 300);
                float kd = (float)stats.kills / stats.deaths;
                float HsK = (float)stats.headshot_kills / stats.kills;
                float accuracy = (float)stats.shots_hit / stats.shots;
                float score = kd * HsK * accuracy / mins_alive;

                if (score > most_score)
                {
                    most_score = score;
                    best_player_name = player.Nickname;
                }
            }


            string highest_killstreak_msg = "<b><color=#43BFF0>" + highest_killstreak_name + "</color></b> <color=#d4af37>had the highest killstreak of</color> <b><color=#FF0000>" + highest_killstreak.ToString() + "</color></b>";
            string most_kills_msg = "<b><color=#43BFF0>" + most_kills_name + "</color></b> <color=#c0c0c0>had the most kills</color> <b><color=#FF0000>" + most_kills.ToString() + "</color></b>";
            string highest_score_msg = "<b><color=#43BFF0>" + best_player_name + "</color></b> <color=#a97142> was the best player with a score of </color> <b><color=#FF0000>" + most_score.ToString("0.0") + "</color></b>";

            foreach (Player player in Player.GetPlayers())
                BroadcastOverride.SetEvenLineSizes(player, 5);

            BroadcastOverride.BroadcastLine(1, 300, BroadcastPriority.Highest, highest_killstreak_msg);
            BroadcastOverride.BroadcastLine(2, 300, BroadcastPriority.Highest, most_kills_msg);
            BroadcastOverride.BroadcastLine(3, 300, BroadcastPriority.Highest, highest_score_msg);

            foreach (Player player in Player.GetPlayers())
                DisplayStats(player, 4);
            BroadcastOverride.UpdateAllDirty();
        }
    }
}
