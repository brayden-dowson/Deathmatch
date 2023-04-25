using InventorySystem.Items;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Items;
using PluginAPI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using static TheRiptide.Utility;

namespace TheRiptide
{
    class Killstreaks
    {
        public enum KillstreakMode { Easy, Standard, Expert, Rage }
        public class Killstreak
        {
            public KillstreakMode mode = KillstreakMode.Standard;
            public int count = 0;
        }

        class Effect
        {
            public string name;
            public byte intensity;

            public Effect(string name, byte intensity)
            {
                this.name = name;
                this.intensity = intensity;
            }
        }

        struct EffectCompare : IEqualityComparer<Effect>
        {
            public bool Equals(Effect x, Effect y)
            {
                return x.name == y.name;
            }

            public int GetHashCode(Effect obj)
            {
                return obj.name.GetHashCode();
            }
        }

        static List<List<ItemType>> easy_kill_streak_table = new List<List<ItemType>>
            {
                new List<ItemType>{ItemType.Painkillers},                                                                               //1
                new List<ItemType>{},                                                                                                   //2
                new List<ItemType>{ItemType.Medkit},                                                                                    //3
                new List<ItemType>{},                                                                                                   //4
                new List<ItemType>{ItemType.GrenadeFlash},                                                                              //5
                new List<ItemType>{},                                                                                                   //6
                new List<ItemType>{ItemType.Medkit},                                                                                    //7
                new List<ItemType>{},                                                                                                   //8
                new List<ItemType>{ItemType.Painkillers},                                                                               //9
                new List<ItemType>{ItemType.GrenadeHE},                                                                                 //10
                new List<ItemType>{ItemType.Medkit},                                                                                    //11
                new List<ItemType>{},                                                                                                   //12
                new List<ItemType>{ItemType.Painkillers},                                                                               //13
                new List<ItemType>{},                                                                                                   //14
                new List<ItemType>{ItemType.Adrenaline},                                                                                //15
                new List<ItemType>{},                                                                                                   //16
                new List<ItemType>{ItemType.Painkillers},                                                                               //17
                new List<ItemType>{},                                                                                                   //18
                new List<ItemType>{ItemType.Medkit},                                                                                    //19
                new List<ItemType>{ItemType.GrenadeHE},                                                                                 //20
                new List<ItemType>{ItemType.Painkillers},                                                                               //21
                new List<ItemType>{},                                                                                                   //22
                new List<ItemType>{ItemType.Medkit},                                                                                    //23
                new List<ItemType>{},                                                                                                   //24
                new List<ItemType>{ItemType.GrenadeFlash }                                                                              //25
            };

        static List<List<ItemType>> standard_kill_streak_table = new List<List<ItemType>>
            {
                new List<ItemType>{                         ItemType.Painkillers},                                                                              //1
                new List<ItemType>{                         ItemType.SCP330},                                                                                   //2
                new List<ItemType>{                         ItemType.Medkit},                                                                                   //3
                new List<ItemType>{                         ItemType.SCP330},                                                                                   //4
                new List<ItemType>{ItemType.GrenadeFlash,   ItemType.Adrenaline},                                                                               //5
                new List<ItemType>{                         ItemType.SCP330},                                                                                   //6
                new List<ItemType>{                         ItemType.Medkit},                                                                                   //7
                new List<ItemType>{                         ItemType.Adrenaline},                                                                               //8
                new List<ItemType>{ItemType.GrenadeHE,      ItemType.Painkillers},                                                                              //9
                new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330},                                                          //10
                new List<ItemType>{                         ItemType.Adrenaline},                                                                               //11
                new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330},                                                          //12
                new List<ItemType>{ItemType.SCP1853,        ItemType.Painkillers},                                                                              //13
                new List<ItemType>{                         ItemType.Adrenaline},                                                                               //14
                new List<ItemType>{                         ItemType.SCP500},                                                                                   //15
                new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330},                                                          //16
                new List<ItemType>{ItemType.SCP207,         ItemType.Painkillers,    ItemType.Adrenaline},                                                      //17
                new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330},                                                          //18
                new List<ItemType>{                         ItemType.SCP500},                                                                                   //19
                new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330,       ItemType.SCP330,        ItemType.Adrenaline},       //20
                new List<ItemType>{ItemType.SCP018,         ItemType.Painkillers},                                                                              //21
                new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330,       ItemType.SCP330},                                   //22
                new List<ItemType>{                         ItemType.SCP500,         ItemType.Adrenaline},                                                      //23
                new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330,       ItemType.SCP330},                                   //24
                new List<ItemType>{ItemType.SCP268,         ItemType.Painkillers }                                                                              //25
            };

        static List<List<ItemType>> expert_kill_streak_table = new List<List<ItemType>>
            {
                new List<ItemType>{                                                         ItemType.Painkillers},                                              //1    
                new List<ItemType>{},                                                                                                                           //2    
                new List<ItemType>{                                                         ItemType.Painkillers},                                              //3    
                new List<ItemType>{},                                                                                                                           //4    
                new List<ItemType>{ItemType.SCP207,                                         ItemType.SCP330},                                                   //5
                new List<ItemType>{                                                         ItemType.Adrenaline},                                               //6    
                new List<ItemType>{                                                         ItemType.SCP330},                                                   //7    
                new List<ItemType>{                         ItemType.Jailbird,              ItemType.Painkillers},                                              //8    
                new List<ItemType>{                                                         ItemType.SCP330},                                                   //9    
                new List<ItemType>{ItemType.SCP244a,                                        ItemType.Adrenaline},                                               //10    
                new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.SCP330,        ItemType.SCP330},   //11   
                new List<ItemType>{                         ItemType.GunCom45,              ItemType.Painkillers,   ItemType.Adrenaline},                       //12    
                new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.SCP330,        ItemType.SCP330},   //13    
                new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.Adrenaline},                       //14    
                new List<ItemType>{ItemType.SCP268,                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //15    
                new List<ItemType>{                         ItemType.ParticleDisruptor,     ItemType.Painkillers,   ItemType.Adrenaline},                       //16    
                new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //17    
                new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.Adrenaline},                       //18    
                new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //19     
                new List<ItemType>{ItemType.SCP018,         ItemType.Jailbird,              ItemType.Painkillers,   ItemType.Adrenaline},                       //20
                new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //21    
                new List<ItemType>{                                                         ItemType.SCP500,        ItemType.Adrenaline},                       //22    
                new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //23    
                new List<ItemType>{                         ItemType.ParticleDisruptor,     ItemType.SCP500,        ItemType.Adrenaline},                       //24    
                new List<ItemType>{ItemType.ArmorHeavy,                                     ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330 }   //25    
            };

        static List<List<ItemType>> rage_kill_streak_table = new List<List<ItemType>>
            {
                new List<ItemType>{                                                         ItemType.Painkillers},                                              //1    
                new List<ItemType>{},                                                                                                                           //2    
                new List<ItemType>{                                                         ItemType.Painkillers},                                              //3    
                new List<ItemType>{},                                                                                                                           //4    
                new List<ItemType>{                                                         ItemType.Adrenaline},                                               //5
                new List<ItemType>{},                                                                                                                           //6    
                new List<ItemType>{                                                         ItemType.Painkillers},                                              //7    
                new List<ItemType>{                         ItemType.Jailbird},                                                                                 //8    
                new List<ItemType>{                                                         ItemType.Painkillers},                                              //9    
                new List<ItemType>{ItemType.SCP244a,                                        ItemType.Adrenaline},                                               //10    
                new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.SCP330},                           //11   
                new List<ItemType>{                         ItemType.GunCom45,              ItemType.Painkillers,   ItemType.Adrenaline},                       //12    
                new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.SCP330},                           //13    
                new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.Adrenaline},                       //14    
                new List<ItemType>{ItemType.SCP268,                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //15    
                new List<ItemType>{                         ItemType.ParticleDisruptor,     ItemType.Painkillers,   ItemType.Adrenaline},                       //16    
                new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //17    
                new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.Adrenaline},                       //18    
                new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //19     
                new List<ItemType>{ItemType.SCP018,         ItemType.Jailbird,              ItemType.Painkillers,   ItemType.Adrenaline},                       //20
                new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //21    
                new List<ItemType>{                                                         ItemType.SCP500,        ItemType.Adrenaline},                       //22    
                new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //23    
                new List<ItemType>{                         ItemType.ParticleDisruptor,     ItemType.SCP500,        ItemType.Adrenaline},                       //24    
                new List<ItemType>{ItemType.SCP244b,                                        ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330 }   //25    
            };

        static List<List<ItemType>> rage_kill_streak_loadout = new List<List<ItemType>>
            {
                new List<ItemType>{                         ItemType.GunCOM15},                                                                                 //0
                new List<ItemType>{                         ItemType.GunCOM15},                                                                                 //1
                new List<ItemType>{                         ItemType.GunCOM15},                                                                                 //2
                new List<ItemType>{                         ItemType.GunCOM18},                                                                                 //3
                new List<ItemType>{                         ItemType.GunCOM18},                                                                                 //4
                new List<ItemType>{ItemType.ArmorLight,     ItemType.GunCOM18},                                                                                 //5
                new List<ItemType>{ItemType.ArmorLight,     ItemType.GunRevolver},                                                                              //6
                new List<ItemType>{ItemType.ArmorLight,     ItemType.GunRevolver},                                                                              //7
                new List<ItemType>{ItemType.ArmorLight,     ItemType.GunRevolver},                                                                              //8
                new List<ItemType>{ItemType.ArmorLight,     ItemType.GunFSP9},                                                                                  //9
                new List<ItemType>{ItemType.ArmorCombat,    ItemType.GunFSP9},                                                                                  //10
                new List<ItemType>{ItemType.ArmorCombat,    ItemType.GunFSP9},                                                                                  //11
                new List<ItemType>{ItemType.ArmorCombat,    ItemType.GunCrossvec},                                                                              //12
                new List<ItemType>{ItemType.ArmorCombat,    ItemType.GunCrossvec},                                                                              //13
                new List<ItemType>{ItemType.ArmorCombat,    ItemType.GunCrossvec},                                                                              //14
                new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunE11SR},                                                                                 //15
                new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunE11SR},                                                                                 //16
                new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunE11SR},                                                                                 //17
                new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunAK},                                                                                    //18
                new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunAK},                                                                                    //19
                new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunAK},                                                                                    //20
                new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunShotgun},                                                                               //21
                new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunShotgun},                                                                               //22
                new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunShotgun},                                                                               //23
                new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunLogicer,        ItemType.GunShotgun},                                                   //24
                new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunLogicer,        ItemType.GunShotgun }                                                   //25
            };

        static List<List<Effect>> rage_kill_streak_status_effects = new List<List<Effect>>
            {
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Disabled", 1),          new Effect("Bleeding", 1)},                                                             //0
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Disabled", 1),          new Effect("Bleeding", 1)},                                                             //1
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Disabled", 1),          new Effect("Bleeding", 1)},                                                             //2
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Disabled", 1)},                                                                                                 //3
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Disabled", 1)},                                                                                                 //4
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Disabled", 1),          new Effect("Scp1853", 1)},                                                              //5
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Scp1853", 1)},                                                                                                  //6
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("MovementBoost", 4),     new Effect("Scp1853", 1)},                                                              //7
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("MovementBoost", 8),     new Effect("Scp1853", 1),                           new Effect("BodyshotReduction", 1)},//8
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("MovementBoost", 12),     new Effect("Scp1853", 1),           new Effect("BodyshotReduction", 1)},                                                    //9
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("MovementBoost", 16),    new Effect("Scp1853", 2),           new Effect("BodyshotReduction", 1)},                                                    //10
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("MovementBoost", 20),    new Effect("Scp1853", 2),           new Effect("BodyshotReduction", 1)},                                                    //11
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("MovementBoost", 24),    new Effect("Scp1853", 2),           new Effect("BodyshotReduction", 2)},                                                                                        //12
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("MovementBoost", 28),    new Effect("Scp1853", 2),           new Effect("BodyshotReduction", 2)},                                                                                        //13
                new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("MovementBoost", 32),    new Effect("Scp1853", 2),           new Effect("BodyshotReduction", 2)},                                                                                        //14
                new List<Effect>{new Effect("MovementBoost", 36),   new Effect("Scp1853", 3),           new Effect("BodyshotReduction", 2), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 1.0f)))) },                                            //15
                new List<Effect>{new Effect("MovementBoost", 40),   new Effect("Scp1853", 3),           new Effect("BodyshotReduction", 3), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 2.0f)))) },                                            //16
                new List<Effect>{new Effect("MovementBoost", 44),   new Effect("Scp1853", 3),           new Effect("BodyshotReduction", 3), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 3.0f)))) },                                            //17
                new List<Effect>{new Effect("MovementBoost", 48),   new Effect("Scp1853", 3),           new Effect("BodyshotReduction", 3), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 4.0f)))) },                                            //18
                new List<Effect>{new Effect("MovementBoost", 52),   new Effect("Scp1853", 3),           new Effect("BodyshotReduction", 3), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 5.0f)))) },                                            //19
                new List<Effect>{new Effect("MovementBoost", 56),   new Effect("Scp1853", 4),           new Effect("BodyshotReduction", 4), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 6.0f)))) },                                            //20
                new List<Effect>{new Effect("MovementBoost", 60),   new Effect("Scp1853", 4),           new Effect("BodyshotReduction", 4), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 7.0f)))) },                                            //21
                new List<Effect>{new Effect("MovementBoost", 64),   new Effect("Scp1853", 4),           new Effect("BodyshotReduction", 4), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 8.0f)))) },                                            //22
                new List<Effect>{new Effect("MovementBoost", 68),   new Effect("Scp1853", 4),           new Effect("BodyshotReduction", 4), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 9.0f)))) },                                            //23
                new List<Effect>{new Effect("MovementBoost", 72),   new Effect("Scp1853", 4),           new Effect("BodyshotReduction", 4), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 10.0f)))) },                                           //24
                new List<Effect>{new Effect("MovementBoost", 76),   new Effect("Scp1853", 255),         new Effect("BodyshotReduction", 4), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 11.0f)))) }                                            //25
            };

        public static Dictionary<int, Killstreak> player_killstreak = new Dictionary<int, Killstreak>();

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            if (!player_killstreak.ContainsKey(player.PlayerId))
                player_killstreak.Add(player.PlayerId, new Killstreak());
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            if (player_killstreak.ContainsKey(player.PlayerId))
                player_killstreak.Remove(player.PlayerId);
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDeath(Player target, Player killer, DamageHandlerBase damage)
        {
            if (killer != null && player_killstreak.ContainsKey(killer.PlayerId))
            {
                Killstreak killstreak = player_killstreak[killer.PlayerId];
                if (killstreak.mode == KillstreakMode.Rage)
                {
                    List<ItemType> previous_loadout = rage_kill_streak_loadout[math.min(25, killstreak.count)];
                    List<ItemType> new_loadout = rage_kill_streak_loadout[math.min(25, killstreak.count + 1)];
                    IEnumerable<ItemType> remove_items = previous_loadout.Except(new_loadout);
                    IEnumerable<ItemType> add_items = new_loadout.Except(previous_loadout);

                    foreach (ItemType item in remove_items)
                    {
                        IEnumerable<ItemBase> matches = killer.Items.Where((i) => i.ItemTypeId == item);
                        killer.RemoveItem(new Item(matches.First()));
                    }
                    foreach (ItemType item in add_items)
                    {
                        if (IsGun(item))
                        {
                            if (killer.IsInventoryFull)
                                if (!RemoveItem(killer, ItemType.Painkillers))
                                    if (!RemoveItem(killer, ItemType.Medkit))
                                        if (!RemoveItem(killer, ItemType.Adrenaline))
                                            if (!RemoveItem(killer, ItemType.SCP500))
                                                if (!RemoveItem(killer, ItemType.SCP244a))
                                                    if (!RemoveItem(killer, ItemType.SCP244b))
                                                        if (!RemoveItem(killer, ItemType.SCP018))
                                                            RemoveItem(killer, ItemType.GunShotgun);
                            AddFirearm(killer, item, true);
                        }
                        else
                            killer.AddItem(item);
                    }

                    List<Effect> previous_effects = rage_kill_streak_status_effects[math.min(25, killstreak.count)];
                    List<Effect> new_effects = rage_kill_streak_status_effects[math.min(25, killstreak.count + 1)];
                    IEnumerable<Effect> remove_effects = previous_effects.Except(new_effects, new EffectCompare());
                    IEnumerable<Effect> add_effects = new_effects.Except(previous_effects);

                    foreach (Effect effect in remove_effects)
                    {
                        killer.EffectsManager.ChangeState(effect.name, 0);
                    }

                    foreach (Effect effect in add_effects)
                    {
                        killer.EffectsManager.ChangeState(effect.name, effect.intensity);
                    }
                }

                List<ItemType> reward_items = new List<ItemType>();
                switch (killstreak.mode)
                {
                    case KillstreakMode.Easy:
                        reward_items = easy_kill_streak_table[killstreak.count % 25];
                        break;
                    case KillstreakMode.Standard:
                        reward_items = standard_kill_streak_table[killstreak.count % 25];
                        break;
                    case KillstreakMode.Expert:
                        reward_items = expert_kill_streak_table[killstreak.count % 25];
                        break;
                    case KillstreakMode.Rage:
                        reward_items = rage_kill_streak_table[killstreak.count % 25];
                        break;
                    default:
                        reward_items = standard_kill_streak_table[killstreak.count % 25];
                        break;
                }

                AddItems(killer, reward_items);
            }
        }

        public static Killstreak GetKillstreak(Player player)
        {
            return player_killstreak[player.PlayerId];
        }

        public static string KillstreakColorCode(Player player)
        {
            switch (player_killstreak[player.PlayerId].mode)
            {
                case KillstreakMode.Easy:
                    return "<color=#5900ff>";
                case KillstreakMode.Standard:
                    return "<color=#43BFF0>";
                case KillstreakMode.Expert:
                    return "<color=#36a832>";
                case KillstreakMode.Rage:
                    return "<color=#FF0000>";
            }
            return "<color=#43BFF0>";
        }

        public static bool IsGunGame(Player player)
        {
            return player_killstreak[player.PlayerId].mode == KillstreakMode.Rage;
        }

        public static void AddKillstreakStartItems(Player player)
        {
            Killstreak killstreak = player_killstreak[player.PlayerId];

            if (killstreak.mode != KillstreakMode.Rage)
            {
                if (killstreak.mode == KillstreakMode.Easy)
                    AddItems(player, new List<ItemType>() { ItemType.Painkillers, ItemType.Medkit, ItemType.Medkit, ItemType.Medkit });
                else if (killstreak.mode == KillstreakMode.Standard)
                    AddItems(player, new List<ItemType>() { ItemType.Painkillers });
            }
            else
            {
                foreach (ItemType item in rage_kill_streak_loadout[0])
                {
                    if (IsGun(item))
                        AddFirearm(player, item, true);
                    else
                        player.AddItem(item);
                }
            }
        }

        public static void AddKillstreakEffects(Player player)
        {
            Killstreak killstreak = player_killstreak[player.PlayerId];
            player.EffectsManager.DisableAllEffects();

            if (killstreak.mode == KillstreakMode.Rage)
            {
                foreach (Effect effect in rage_kill_streak_status_effects[0])
                {
                    player.EffectsManager.ChangeState(effect.name, effect.intensity);
                }
            }
        }
    }
}
