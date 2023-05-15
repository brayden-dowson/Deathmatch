using CommandSystem;
using InventorySystem.Items;
using MEC;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static TheRiptide.Translation;

namespace TheRiptide
{
    public class ExperienceConfig
    {
        public bool IsEnabled { get; set; } = true;
        [Description("log how much xp and the reason to the recievers gameconsole")]
        public bool GameConsoleLog { get; set; } = true;
        [Description("xp rewards. xp is only saved at the end of the round, if a player leaves before the round ends they will not receive any xp")]
        public int XpPerKill { get; set; } = 50;
        public int XpPerMinute { get; set; } = 20;
        public int XpOn100Damage { get; set; } = 25;
        public int XpOn500Damage { get; set; } = 250;
        public int XpOn2500Damage { get; set; } = 2500;
        public int XpOn10000Damage { get; set; } = 20000;
        public int XpOn5Killstreak { get; set; } = 250;
        public int XpOn10Killstreak { get; set; } = 1000;
        public int XpOn15Killstreak { get; set; } = 2000;
        public int XpOn20Killstreak { get; set; } = 4000;
        public int XpOn25Killstreak { get; set; } = 10000;
        [Description("xp granted 30 seconds into the round to reward players that stay an entire round")]
        public int XpOnRoundStart { get; set; } = 1200;

        [Description("see the global reference config for all item types")]
        public Dictionary<ItemType, int> XpOnItemUse { get; set; } = new Dictionary<ItemType, int>
        {
            {ItemType.Painkillers,5 },
            {ItemType.Medkit, 5 },
            {ItemType.SCP330, 10 },
            {ItemType.Adrenaline, 15 },
            {ItemType.GrenadeFlash, 30 },
            {ItemType.SCP244a, 50 },
            {ItemType.SCP244b, 50 },
            {ItemType.GrenadeHE, 60 },
            {ItemType.SCP207, 100 },
            {ItemType.SCP1853, 100 },
            {ItemType.SCP500, 150 },
            {ItemType.SCP2176, 150 },
            {ItemType.SCP268, 300 },

        };

        [Description("see the global reference config for all item types")]
        public Dictionary<ItemType, int> XpOnItemThrown { get; set; } = new Dictionary<ItemType, int>
        {
            {ItemType.SCP244a, 50 },
            {ItemType.SCP244b, 50 },
            {ItemType.SCP2176, 150 },
            {ItemType.SCP018, 300 },
        };

        [Description("xp leveling - to fine tune these values use this calculator https://www.desmos.com/calculator/1dqftattpd")]
        public int BaseXpLevel { get; set; } = 250;
        public float LevelExponent { get; set; } = 1.1f;
        public float StageExponent { get; set; } = 1.25f;
        public float TierExponent { get; set; } = 1.5f;

        public int XpRounding { get; set; } = 5;
        [Description("{0} = tier {1} = stage {2} = level {3} = XpToNextLevelFormat")]
        public string BadgeFormat { get; set; } = "{0} | {1} | {2} | {3}";
        [Description("{0} = xp {1} = max_xp")]
        public string XpToNextLevelFormat { get; set; } = "XP: {0}/{1}";
        public List<string> LevelTags { get; set; } = new List<string>
        {
            "Level: 1",
            "Level: 2",
            "Level: 3",
            "Level: 4",
            "Level: 5",
            "Level: 6",
            "Level: 7",
            "Level: 8",
            "Level: 9",
            "Level: 10",
            "Level: 11",
            "Level: 12",
            "Level: 13",
            "Level: 14",
            "Level: 15",
            "Level: 16",
            "Level: 17",
            "Level: 18",
            "Level: 19",
            "Level: 20",
            "Level: 21",
            "Level: 22",
            "Level: 23",
            "Level: 24",
            "Level: 25",
        };
        public List<string> StageTags { get; set; } = new List<string>
        {
            "Stage: 1",
            "Stage: 2",
            "Stage: 3",
            "Stage: 4",
            "Stage: 5",
            "Stage: 6",
            "Stage: 7",
        };
        public List<string> TierTags { get; set; } = new List<string>
        {
            "Tier: 1",
            "Tier: 2",
            "Tier: 3",
            "Tier: 4",
            "Tier: 5",
            "Tier: 6",
            "Tier: 7",
        };
    }

    public class Experiences
    {
        public static Experiences Singleton { get; private set; }
        private ExperienceConfig config;

        class Tracking
        {
            public float damage = 0.0f;
            public int killstreak = 0;
            public float connect_time = UnityEngine.Time.time;
        }

        public class XP
        {
            public int value { get; set; } = 0;
            public int level { get; set; } = 0;
            public int stage { get; set; } = 0;
            public int tier { get; set; } = 0;
        }

        private Dictionary<int, XP> previous_xp = new Dictionary<int, XP>();
        private Dictionary<int, XP> player_xp = new Dictionary<int, XP>();
        private Dictionary<int, Tracking> player_tracking = new Dictionary<int, Tracking>();

        public Experiences()
        {
            Singleton = this;
        }

        public void Init(ExperienceConfig config)
        {
            this.config = config;
        }

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            Timing.CallDelayed(30.0f, ()=>
            {
                foreach (var player in Player.GetPlayers())
                    if (player != null && player_xp.ContainsKey(player.PlayerId))
                        RewardXp(player, config.XpOnRoundStart, translation.RewardXpRoundStart);
            });
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            int id = player.PlayerId;
            if (!player_xp.ContainsKey(id))
            {
                player_xp.Add(id, new XP());
                Database.Singleton.LoadExperience(player);
            }

            if (!player_tracking.ContainsKey(id))
                player_tracking.Add(id, new Tracking());
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            int id = player.PlayerId;
            if (player_xp.ContainsKey(id))
                player_xp.Remove(id);

            if (player_tracking.ContainsKey(id))
                player_tracking.Remove(id);

            if (previous_xp.ContainsKey(id))
                previous_xp.Remove(id);
        }

        [PluginEvent(ServerEventType.PlayerUsedItem)]
        void OnPlayerUsedItem(Player player, ItemBase item)
        {
            if (player != null && config.XpOnItemUse.ContainsKey(item.ItemTypeId))
                RewardXp(player, config.XpOnItemUse[item.ItemTypeId], translation.RewardXpItemUsed.Replace("{item}", item.ItemTypeId.ToString()));
        }

        [PluginEvent(ServerEventType.PlayerThrowItem)]
        void OnThrowItem(Player player, ItemBase item, Rigidbody rb)
        {
            if (player != null && config.XpOnItemThrown.ContainsKey(item.ItemTypeId))
                RewardXp(player, config.XpOnItemThrown[item.ItemTypeId], translation.RewardXpItemThrown.Replace("{item}", item.ItemTypeId.ToString()));
        }

        [PluginEvent(ServerEventType.PlayerDamage)]
        void OnPlayerDamage(Player victim, Player attacker, DamageHandlerBase damage)
        {
            if (victim != null && attacker != null && player_xp.ContainsKey(victim.PlayerId) && player_xp.ContainsKey(attacker.PlayerId))
            {
                if (damage is StandardDamageHandler standard)
                {
                    Tracking t = player_tracking[attacker.PlayerId];
                    float new_damage = t.damage + standard.Damage;
                    if (t.damage < 100 && 100 < new_damage)
                        RewardXp(attacker, config.XpOn100Damage, translation.RewardXp100Damage);
                    else if (t.damage < 500 && 500 < new_damage)
                        RewardXp(attacker, config.XpOn500Damage, translation.RewardXp500Damage);
                    else if (t.damage < 2500 && 2500 < new_damage)
                        RewardXp(attacker, config.XpOn2500Damage, translation.RewardXp2500Damage);
                    else if(t.damage < 10000 && 10000 < new_damage)
                        RewardXp(attacker, config.XpOn10000Damage, translation.RewardXp10000Damage);
                    t.damage = new_damage;
                }
            }
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDeath(Player victim, Player killer, DamageHandlerBase damage)
        {
            if (victim != null && killer != null && player_xp.ContainsKey(victim.PlayerId) && player_xp.ContainsKey(killer.PlayerId) && victim != killer)
            {
                RewardXp(killer, config.XpPerKill, translation.RewardXpKill);
                player_tracking[killer.PlayerId].killstreak++;
                switch(player_tracking[killer.PlayerId].killstreak)
                {
                    case 5:
                        RewardXp(killer, config.XpOn5Killstreak, translation.RewardXp5Killstreak);
                        break;
                    case 10:
                        RewardXp(killer, config.XpOn10Killstreak, translation.RewardXp10Killstreak);
                        break;
                    case 15:
                        RewardXp(killer, config.XpOn15Killstreak, translation.RewardXp15Killstreak);
                        break;
                    case 20:
                        RewardXp(killer, config.XpOn20Killstreak, translation.RewardXp20Killstreak);
                        break;
                    case 25:
                        RewardXp(killer, config.XpOn25Killstreak, translation.RewardXp25Killstreak);
                        break;
                }
            }
            if (victim != null && player_tracking.ContainsKey(victim.PlayerId))
                player_tracking[victim.PlayerId].killstreak = 0;
        }

        public void XpLoaded(Player player)
        {
            XP xp = player_xp[player.PlayerId];
            previous_xp.Add(player.PlayerId, new XP { tier = xp.tier, level = xp.level, stage = xp.stage, value = xp.value });
            BadgeOverride.Singleton.SetBadge(player, 1, XpString(xp));
            ShowXpHint(player, xp, 10.0f);
            HintOverride.Refresh(player);
        }

        public void RewardXp(Player player, int amount, string reason)
        {
            player_xp[player.PlayerId].value += amount;
            if (config.GameConsoleLog)
                player.SendConsoleMessage(reason.Replace("{xp}", amount.ToString()), "cyan");
        }

        public XP GetXP(Player player)
        {
            return player_xp[player.PlayerId];
        }

        public void SaveExperiences()
        {
            foreach (Player p in Player.GetPlayers())
            {
                if (player_xp.ContainsKey(p.PlayerId))
                {
                    int minutes = Mathf.RoundToInt((Time.time - player_tracking[p.PlayerId].connect_time) / 60.0f);
                    RewardXp(p, config.XpPerMinute * minutes, translation.RewardXpMinute.Replace("{time}", minutes.ToString()));

                    XP xp = player_xp[p.PlayerId];
                    bool maxed_tier = xp.tier >= config.TierTags.Count - 1;
                    bool maxed_stage = xp.stage >= config.StageTags.Count - 1;
                    bool maxed_level = xp.level >= config.LevelTags.Count - 1;
                    int next = XpToNextLevel(xp);
                    while (xp.value > next)
                    {
                        if (!(maxed_tier && maxed_stage && maxed_level))
                        {
                            xp.value -= next;
                            xp.level++;
                            if (xp.level > config.LevelTags.Count - 1)
                            {
                                if (!(maxed_tier && maxed_stage))
                                {
                                    xp.level = 0;
                                    xp.stage++;
                                    if (xp.stage > config.StageTags.Count - 1)
                                    {
                                        if (!maxed_tier)
                                        {
                                            xp.stage = 0;
                                            xp.tier++;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            xp.value = next;
                            break;
                        }
                        maxed_tier = xp.tier >= config.TierTags.Count - 1;
                        maxed_stage = xp.stage >= config.StageTags.Count - 1;
                        maxed_level = xp.level >= config.LevelTags.Count - 1;
                        next = XpToNextLevel(xp);
                    }
                    Database.Singleton.SaveExperience(p);
                    ShowXpHint(p, xp, 30.0f);
                    BadgeOverride.Singleton.SetBadge(p, 1, XpString(xp));
                }
            }
        }

        private string XpString(XP xp)
        {
            string tier = config.TierTags[Mathf.Min(config.TierTags.Count - 1, xp.tier)];
            string stage = config.StageTags[Mathf.Min(config.StageTags.Count - 1, xp.stage)];
            string level = config.LevelTags[Mathf.Min(config.LevelTags.Count - 1, xp.level)];
            return string.Format(config.BadgeFormat, tier, stage, level, string.Format(config.XpToNextLevelFormat, xp.value, XpToNextLevel(xp)));
        }

        private void ShowXpHint(Player player, XP xp, float duration)
        {
            string xp_hint = translation.XpMsg.Replace("{xp}", XpString(xp));
            HintOverride.Add(player, 1, xp_hint, duration);
        }

        private int XpToNextLevel(XP xp)
        {
            float exact_value = config.BaseXpLevel * Mathf.Pow(config.LevelExponent, (float)xp.level) * Mathf.Pow(config.StageExponent, (float)xp.stage) * Mathf.Pow(config.TierExponent, (float)xp.tier);
            return Mathf.RoundToInt(exact_value / config.XpRounding) * config.XpRounding;
        }
    }

    //[CommandHandler(typeof(RemoteAdminCommandHandler))]
    //public class DmSetXp : ICommand
    //{
    //    public string Command { get; } = "dmsetxp";

    //    public string[] Aliases { get; } = new string[] { "dxp" };

    //    public string Description { get; } = "set xp on player. '[player_id] [value] [level] [stage] [tier]', -1 = palceholder, -1 id = self";

    //    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    //    {
    //        Player player;
    //        if (Player.TryGet(sender, out player))
    //        {
    //            int id = -1;
    //            int value = -1;
    //            int level = -1;
    //            int stage = -1;
    //            int tier = -1;
    //            if (!int.TryParse(arguments.ElementAt(0), out id))
    //            {
    //                response = "failed to parse id: " + arguments.ElementAt(0);
    //                return false;
    //            }
    //            if (arguments.Count >= 2)
    //            {
    //                if (!int.TryParse(arguments.ElementAt(1), out value))
    //                {
    //                    response = "failed to parse value: " + arguments.ElementAt(1);
    //                    return false;
    //                }
    //            }
    //            if (arguments.Count >= 3)
    //            {
    //                if (!int.TryParse(arguments.ElementAt(2), out level))
    //                {
    //                    response = "failed to parse level: " + arguments.ElementAt(2);
    //                    return false;
    //                }
    //            }
    //            if (arguments.Count >= 4)
    //            {
    //                if (!int.TryParse(arguments.ElementAt(3), out stage))
    //                {
    //                    response = "failed to parse stage: " + arguments.ElementAt(3);
    //                    return false;
    //                }
    //            }
    //            if (arguments.Count >= 5)
    //            {
    //                if (!int.TryParse(arguments.ElementAt(4), out tier))
    //                {
    //                    response = "failed to parse tier: " + arguments.ElementAt(4);
    //                    return false;
    //                }
    //            }
    //            Experiences.XP xp = Experiences.Singleton.GetXP(player);
    //            if (id != -1)
    //            {
    //                Player target = null;
    //                if (!Player.TryGet(id, out target))
    //                {
    //                    response = "failed to get player with id: " + id;
    //                    return false;
    //                }
    //                xp = Experiences.Singleton.GetXP(target);
    //            }
    //            if (value != -1)
    //                xp.value = value;
    //            if (level != -1)
    //                xp.level = level;
    //            if (stage != -1)
    //                xp.stage = stage;
    //            if (tier != -1)
    //                xp.tier = tier;
    //            response = "successfully set xp: " + xp.value + " level: " + xp.level + " stage: " + xp.stage + " tier: " + xp.tier;
    //            return true;
    //        }
    //        response = "failed";
    //        return false;
    //    }
    //}
}
