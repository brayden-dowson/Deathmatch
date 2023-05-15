using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Items;
using PluginAPI.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Mathematics;
using static TheRiptide.Utility;
using static TheRiptide.Translation;

namespace TheRiptide
{
    //colors https://colorswall.com/palette/171311
    //#9b5fe0
    //#16a4d8 
    //#60dbe8 
    //#8bd346 
    //#efdf48 
    //#f9a52c 
    //#d64e12 
    public enum InventoryAction { Add, Remove }
    public class ItemReward
    {
        public InventoryAction Action { get; set; } = InventoryAction.Add;
        public ItemType Item { get; set; }
    }

    public class EffectReward
    {
        public string Effect { get; set; }
        public byte Intensity { get; set; }
        public byte Duration { get; set; }
    }

    public enum AmmoAction { Add, Remove, Set }
    public enum AmmoStat { Inventory, Gun }
    public class AmmoReward
    {
        public AmmoAction Action { get; set; } = AmmoAction.Add;
        public AmmoStat Stat { get; set; }
        public float Proportion { get; set; }
    }

    public enum PlayerAction { Add, Remove, Set }
    public enum PlayerStat { HP, AHP, Stamina }
    public class PlayerReward
    {
        public PlayerAction Action { get; set; } = PlayerAction.Add;
        public PlayerStat Stat { get; set; }
        public float Value { get; set; }
        public float Sustain { get; set; }
        public bool Persistent { get; set; }
    }

    public enum OverflowAction { End, Rollover, Clamp}
    public class KillstreakRewardTable
    {
        public ItemType MenuItem { get; set; }
        public string MenuDescription { get; set; }

        public bool LoadoutLock { get; set; }
        public string ColorHex { get; set; }

        public OverflowAction ItemOverflowAction { get; set; }
        public SortedDictionary<int, List<ItemReward>> ItemTable { get; set; }

        public OverflowAction AmmoOverflowAction { get; set; }
        public SortedDictionary<int, List<AmmoReward>> AmmoTable { get; set; }

        public OverflowAction PlayerOverflowAction { get; set; }
        public SortedDictionary<int, List<PlayerReward>> PlayerTable { get; set; }

        public OverflowAction EffectOverflowAction { get; set; }
        public SortedDictionary<int, List<EffectReward>> EffectTable { get; set; }
    }

    public class KillstreakConfig
    {
        public string DefaultKillstreak { get; set; } = "Novice";

        public Dictionary<string, KillstreakRewardTable> KillstreakTables { get; set; } = new Dictionary<string, KillstreakRewardTable>
        {
            {
                "Noob",
                new KillstreakRewardTable
                {
                    MenuItem = ItemType.KeycardJanitor,
                    MenuDescription = "<color=#bdafe4>[JANITOR]</color> = <b><color=#16a4d8>Noob</color> - spawn with heavy armor and 3x medkits, max killstreak cap 5</b>",
                    LoadoutLock = false,
                    ColorHex = "#16a4d8",

                    ItemOverflowAction = OverflowAction.End,
                    ItemTable = new SortedDictionary<int, List<ItemReward>>
                    {
                        { 0, new List<ItemReward>{ new ItemReward {Item = ItemType.ArmorHeavy }, new ItemReward {Item = ItemType.Medkit }, new ItemReward{ Item = ItemType.Medkit }, new ItemReward { Item = ItemType.Medkit } } },
                        { 2, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers } } },
                        { 5, new List<ItemReward>{ new ItemReward {Item = ItemType.GrenadeFlash } } }
                    },

                    AmmoOverflowAction = OverflowAction.Rollover,
                    AmmoTable = new SortedDictionary<int, List<AmmoReward>>
                    {
                        {0, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Set, Stat = AmmoStat.Inventory, Proportion = 1.0f } } },
                        {1, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.1f } } }
                    },

                    PlayerOverflowAction = OverflowAction.End,
                    PlayerTable = new SortedDictionary<int, List<PlayerReward>>{},

                    EffectOverflowAction = OverflowAction.End,
                    EffectTable = new SortedDictionary<int, List<EffectReward>>{}
                }
            },
            {
                "Novice",
                new KillstreakRewardTable
                {
                    MenuItem = ItemType.KeycardScientist,
                    MenuDescription = "<color=#e7d77b>[SCIENTIST]</color> = <b><color=#60dbe8>Novice</color> - spawn with combat armor, medkit and painkillers, max killstreak cap 10</b>",
                    LoadoutLock = false,
                    ColorHex = "#60dbe8",

                    ItemOverflowAction = OverflowAction.End,
                    ItemTable = new SortedDictionary<int, List<ItemReward>>
                    {
                        { 0, new List<ItemReward>{ new ItemReward {Item = ItemType.ArmorCombat }, new ItemReward {Item = ItemType.Medkit }, new ItemReward{ Item = ItemType.Painkillers } } },
                        { 2, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers } } },
                        { 4, new List<ItemReward>{ new ItemReward {Item = ItemType.Medkit } } },
                        { 6, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers } } },
                        { 8, new List<ItemReward>{ new ItemReward {Item = ItemType.Medkit } } },
                        { 10, new List<ItemReward>{ new ItemReward {Item = ItemType.GrenadeHE } } },
                    },

                    AmmoOverflowAction = OverflowAction.Rollover,
                    AmmoTable = new SortedDictionary<int, List<AmmoReward>>
                    {
                        {0, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Set, Stat = AmmoStat.Inventory, Proportion = 1.0f } } },
                        {1, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.15f } } }
                    },

                    PlayerOverflowAction = OverflowAction.End,
                    PlayerTable = new SortedDictionary<int, List<PlayerReward>>{},

                    EffectOverflowAction = OverflowAction.End,
                    EffectTable = new SortedDictionary<int, List<EffectReward>>{}
                }
            },
            {
                "Intermediate",
                new KillstreakRewardTable
                {
                    MenuItem = ItemType.KeycardNTFOfficer,
                    MenuDescription = "<color=#accfe1>[PRIVATE]</color> = <b><color=#8bd346>Intermediate</color> - spawn with combat armor and painkillers, max killstreak cap 15</b>",
                    LoadoutLock = false,
                    ColorHex = "#8bd346",

                    ItemOverflowAction = OverflowAction.End,
                    ItemTable = new SortedDictionary<int, List<ItemReward>>
                    {
                        { 0, new List<ItemReward>{ new ItemReward {Item = ItemType.ArmorCombat }, new ItemReward { Item = ItemType.Painkillers } } },
                        { 3, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers } } },
                        { 6, new List<ItemReward>{ new ItemReward {Item = ItemType.Medkit } } },
                        { 8, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers } } },
                        { 10, new List<ItemReward>{ new ItemReward {Item = ItemType.Medkit }, new ItemReward { Item = ItemType.GrenadeFlash } } },
                        { 12, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers } } },
                        { 13, new List<ItemReward>{ new ItemReward {Item = ItemType.Medkit }, new ItemReward { Item = ItemType.SCP2176 } } },
                        { 14, new List<ItemReward>{ new ItemReward {Item = ItemType.Adrenaline } } },
                        { 15, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers }, new ItemReward { Item = ItemType.ParticleDisruptor } } },
                    },

                    AmmoOverflowAction = OverflowAction.Rollover,
                    AmmoTable = new SortedDictionary<int, List<AmmoReward>>
                    {
                        {0, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Set, Stat = AmmoStat.Inventory, Proportion = 0.5f } } },
                        {1, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.2f } } }
                    },

                    PlayerOverflowAction = OverflowAction.End,
                    PlayerTable = new SortedDictionary<int, List<PlayerReward>>{},

                    EffectOverflowAction = OverflowAction.End,
                    EffectTable = new SortedDictionary<int, List<EffectReward>>{}
                }
            },
            {
                "Advanced",
                new KillstreakRewardTable
                {
                    MenuItem = ItemType.KeycardNTFLieutenant,
                    MenuDescription = "<color=#177dde>[SERGEANT]</color> = <b><color=#efdf48>Advanced</color> - spawn with light armor, max killstreak 20+</b>",
                    LoadoutLock = false,
                    ColorHex = "#efdf48",

                    ItemOverflowAction = OverflowAction.Clamp,
                    ItemTable = new SortedDictionary<int, List<ItemReward>>
                    {
                        { 0, new List<ItemReward>{ new ItemReward {Item = ItemType.ArmorLight } } },
                        { 1, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers } } },
                        { 2, new List<ItemReward>{ new ItemReward {Item = ItemType.SCP330 } } },
                        { 3, new List<ItemReward>{ new ItemReward {Item = ItemType.Medkit } } },
                        { 4, new List<ItemReward>{ new ItemReward {Item = ItemType.SCP330 } } },
                        { 5, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers },                                   new ItemReward { Item = ItemType.SCP1853 } } },
                        { 6, new List<ItemReward>{ new ItemReward {Item = ItemType.SCP330 },                                        new ItemReward { Item = ItemType.SCP330 } } },
                        { 7, new List<ItemReward>{ new ItemReward {Item = ItemType.Medkit },                                        new ItemReward { Item = ItemType.SCP330 } } },
                        { 8, new List<ItemReward>{ new ItemReward {Item = ItemType.SCP330 },                                        new ItemReward { Item = ItemType.SCP330 } } },
                        { 9, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers } } },
                        { 10, new List<ItemReward>{ new ItemReward {Action = InventoryAction.Remove, Item = ItemType.ArmorLight },  new ItemReward { Item = ItemType.ArmorCombat },   new ItemReward { Item = ItemType.ParticleDisruptor } } },
                        { 11, new List<ItemReward>{ new ItemReward {Item = ItemType.Medkit } } },
                        { 12, new List<ItemReward>{ new ItemReward {Item = ItemType.SCP330 },                                       new ItemReward { Item = ItemType.SCP330 },        new ItemReward { Item = ItemType.Adrenaline } } },
                        { 13, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers },                                  new ItemReward { Item = ItemType.Adrenaline } } },
                        { 14, new List<ItemReward>{ new ItemReward {Item = ItemType.SCP330 },                                       new ItemReward { Item = ItemType.SCP330 },        new ItemReward { Item = ItemType.Adrenaline } } },
                        { 15, new List<ItemReward>{ new ItemReward {Action = InventoryAction.Remove, Item = ItemType.ArmorCombat }, new ItemReward { Item = ItemType.ArmorHeavy },    new ItemReward { Item = ItemType.SCP244a } } },
                        { 16, new List<ItemReward>{ new ItemReward {Item = ItemType.SCP330 },                                       new ItemReward { Item = ItemType.SCP330 },        new ItemReward { Item = ItemType.Adrenaline } } },
                        { 17, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers },                                  new ItemReward { Item = ItemType.Jailbird } } },
                        { 18, new List<ItemReward>{ new ItemReward {Item = ItemType.SCP330 },                                       new ItemReward { Item = ItemType.SCP330 },        new ItemReward { Item = ItemType.Adrenaline } } },
                        { 19, new List<ItemReward>{ new ItemReward {Item = ItemType.Medkit },                                       new ItemReward { Item = ItemType.SCP244b } } },
                        { 20, new List<ItemReward>{ new ItemReward {Item = ItemType.SCP500 },                                       new ItemReward { Item = ItemType.SCP330 },        new ItemReward { Item = ItemType.SCP330 },                new ItemReward { Item = ItemType.Adrenaline } } },
                    },

                    AmmoOverflowAction = OverflowAction.Rollover,
                    AmmoTable = new SortedDictionary<int, List<AmmoReward>>
                    {
                        {0, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Set, Stat = AmmoStat.Inventory, Proportion = 1.0f } } },
                        {1, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.25f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.25f } } }
                    },

                    PlayerOverflowAction = OverflowAction.Clamp,
                    PlayerTable = new SortedDictionary<int, List<PlayerReward>>
                    {
                        {5, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {6, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {7, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {8, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {9, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {10, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {11, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {12, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {13, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {14, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {15, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {16, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {17, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {18, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {19, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {20, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 80 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 30 }, new PlayerReward {Stat = PlayerStat.HP, Value = 30} } },
                    },

                    EffectOverflowAction = OverflowAction.End,
                    EffectTable = new SortedDictionary<int, List<EffectReward>>
                    {
                        {7, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 10 } } },
                        {15, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 15 } } },
                        {20, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 20 } } }
                    }
                }
            },
            {
                "Expert",
                new KillstreakRewardTable
                {
                    MenuItem = ItemType.KeycardNTFCommander,
                    MenuDescription = "<color=#1b43cb>[CAPTAIN]</color> = <b><color=#f9a52c>Expert</color> - spawn with light armor, max killstreak 25+</b>",
                    LoadoutLock = true,
                    ColorHex = "#f9a52c",

                    ItemOverflowAction = OverflowAction.Clamp,
                    ItemTable = new SortedDictionary<int, List<ItemReward>>
                    {
                        { 0, new List<ItemReward>{ new ItemReward {Item = ItemType.ArmorLight },                                        new ItemReward {Item = ItemType.GunFSP9 } } },
                        { 2, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers } } },
                        { 4, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP330 } } },
                        { 5, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.GunFSP9 },         new ItemReward { Item = ItemType.GunCrossvec } } },
                        { 6, new List<ItemReward>{ new ItemReward { Item = ItemType.Painkillers } } },
                        { 8, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP330 },                                           new ItemReward { Item = ItemType.ParticleDisruptor } } },
                        { 10, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.GunCrossvec },    new ItemReward { Item = ItemType.GunE11SR },        new ItemReward { Item = ItemType.Painkillers },         new ItemReward { Item = ItemType.SCP244a } } },
                        { 11, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 12, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.ArmorLight },     new ItemReward { Item = ItemType.ArmorCombat },     new ItemReward { Item = ItemType.Painkillers },         new ItemReward { Item = ItemType.GunCom45 } } },
                        { 13, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 14, new List<ItemReward>{ new ItemReward { Item = ItemType.Painkillers } } },
                        { 15, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.GunE11SR },       new ItemReward { Item = ItemType.GunAK },           new ItemReward { Item = ItemType.Adrenaline },          new ItemReward { Item = ItemType.SCP330 },                  new ItemReward { Item = ItemType.SCP268 } } },
                        { 16, new List<ItemReward>{ new ItemReward { Item = ItemType.Painkillers },                                     new ItemReward { Item = ItemType.SCP330 },          new ItemReward { Item = ItemType.Jailbird } } },
                        { 17, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 18, new List<ItemReward>{ new ItemReward { Item = ItemType.Painkillers },                                     new ItemReward { Item = ItemType.SCP330 } } },
                        { 19, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 20, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.GunAK },          new ItemReward { Item = ItemType.GunLogicer },      new ItemReward{Item = ItemType.SCP018 },                new ItemReward {Item = ItemType.ParticleDisruptor } } },
                        { 21, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 22, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP500 },                                          new ItemReward { Item = ItemType.SCP330 } } },
                        { 23, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 24, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP500 },                                          new ItemReward { Item = ItemType.SCP330 },          new ItemReward { Item = ItemType.Jailbird } } },
                        { 25, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.ArmorCombat },    new ItemReward { Item = ItemType.ArmorHeavy },      new ItemReward { Item = ItemType.GunShotgun },          new ItemReward { Item = ItemType.SCP500 },                  new ItemReward { Item = ItemType.Adrenaline },      new ItemReward { Item = ItemType.SCP330 }, new ItemReward { Item = ItemType.SCP244b } } },
                        { 26, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP500 },                                          new ItemReward { Item = ItemType.Adrenaline },      new ItemReward { Item = ItemType.SCP330 } } },
                    },

                    AmmoOverflowAction = OverflowAction.Rollover,
                    AmmoTable = new SortedDictionary<int, List<AmmoReward>>
                    {
                        {0, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Set, Stat = AmmoStat.Inventory, Proportion = 1.0f } } },
                        {1, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.3f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.3f } } }
                    },

                    PlayerOverflowAction = OverflowAction.Clamp,
                    PlayerTable = new SortedDictionary<int, List<PlayerReward>>
                    {
                        {5, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {6, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {7, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {8, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {9, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {10, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {11, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {12, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {13, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {14, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {15, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {16, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {17, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {18, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {19, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {20, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 80 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 30 }, new PlayerReward {Stat = PlayerStat.HP, Value = 30} } },
                        {21, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 80 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 30 }, new PlayerReward {Stat = PlayerStat.HP, Value = 30} } },
                        {22, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 80 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 30 }, new PlayerReward {Stat = PlayerStat.HP, Value = 30} } },
                        {23, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 80 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 30 }, new PlayerReward {Stat = PlayerStat.HP, Value = 30} } },
                        {24, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 80 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 30 }, new PlayerReward {Stat = PlayerStat.HP, Value = 30} } },
                        {25, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 100 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 40 }, new PlayerReward {Stat = PlayerStat.HP, Value = 45} } },
                    },

                    EffectOverflowAction = OverflowAction.Clamp,
                    EffectTable = new SortedDictionary<int, List<EffectReward>>
                    {
                        {5, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 10 } } },
                        {10, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 20 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 1 } } },
                        {15, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 30 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 2 }, new EffectReward { Effect = "Scp1853", Intensity = 1 } } },
                        {20, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 40 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 3 }, new EffectReward { Effect = "Scp1853", Intensity = 2 } } },
                        {21, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 40 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 3 }, new EffectReward { Effect = "Scp1853", Intensity = 2 } } },
                        {22, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 40 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 3 }, new EffectReward { Effect = "Scp1853", Intensity = 2 } } },
                        {23, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 40 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 3 }, new EffectReward { Effect = "Scp1853", Intensity = 2 } } },
                        {24, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 40 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 3 }, new EffectReward { Effect = "Scp1853", Intensity = 2 } } },
                        {25, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 50 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 4 }, new EffectReward { Effect = "Scp1853", Intensity = 3 } } },
                    }
                }
            },
            {
                "RAGE",
                new KillstreakRewardTable
                {
                    MenuItem = ItemType.KeycardChaosInsurgency,
                    MenuDescription = "<color=#008f1c>[CHAOS]</color> = <b><color=#d64e12>[DATA EXPUNGED]</color></b>",
                    LoadoutLock = true,
                    ColorHex = "#d64e12",

                    ItemOverflowAction = OverflowAction.Clamp,
                    ItemTable = new SortedDictionary<int, List<ItemReward>>
                    {
                        { 0, new List<ItemReward>{ new ItemReward { Item = ItemType.ArmorLight },                                       new ItemReward {Item = ItemType.GunCOM15 } } },
                        { 2, new List<ItemReward>{ new ItemReward { Item = ItemType.Painkillers } } },
                        { 3, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.GunCOM15 },        new ItemReward { Item = ItemType.GunCOM18 } } },
                        { 4, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP330 } } },
                        { 5, new List<ItemReward>{ } },
                        { 6, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.GunCOM18 },        new ItemReward { Item = ItemType.GunRevolver },     new ItemReward { Item = ItemType.Painkillers } } },
                        { 8, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP330 },                                           new ItemReward { Item = ItemType.ParticleDisruptor } } },
                        { 9, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.GunRevolver },     new ItemReward { Item = ItemType.GunFSP9 } } },
                        { 10, new List<ItemReward>{ new ItemReward { Item = ItemType.Painkillers },                                     new ItemReward { Item = ItemType.SCP244a } } },
                        { 11, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 12, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.GunFSP9 },        new ItemReward { Item = ItemType.GunCrossvec },     new ItemReward { Action = InventoryAction.Remove, Item = ItemType.ArmorLight },     new ItemReward { Item = ItemType.ArmorCombat },     new ItemReward { Item = ItemType.Painkillers },         new ItemReward { Item = ItemType.GunCom45 } } },
                        { 13, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 14, new List<ItemReward>{ new ItemReward { Item = ItemType.Painkillers } } },
                        { 15, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.GunCrossvec },    new ItemReward { Item = ItemType.GunE11SR },        new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 },          new ItemReward { Item = ItemType.SCP268 } } },
                        { 16, new List<ItemReward>{ new ItemReward { Item = ItemType.Painkillers },                                     new ItemReward { Item = ItemType.SCP330 },          new ItemReward { Item = ItemType.Jailbird } } },
                        { 17, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 18, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.GunE11SR },       new ItemReward { Item = ItemType.GunAK },           new ItemReward { Item = ItemType.Painkillers },                                     new ItemReward { Item = ItemType.SCP330 } } },
                        { 19, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 20, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP018 },                                          new ItemReward { Item = ItemType.ParticleDisruptor } } },
                        { 21, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.GunAK },          new ItemReward { Item = ItemType.GunLogicer },      new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 22, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP500 },                                          new ItemReward { Item = ItemType.SCP330 } } },
                        { 23, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 24, new List<ItemReward>{ new ItemReward { Item = ItemType.GunShotgun },                                      new ItemReward { Item = ItemType.SCP500 },          new ItemReward { Item = ItemType.SCP330 },                                          new ItemReward { Item = ItemType.Jailbird } } },
                        { 25, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.ArmorCombat },    new ItemReward { Item = ItemType.ArmorHeavy },      new ItemReward { Item = ItemType.GunShotgun },                                      new ItemReward { Item = ItemType.SCP500 },          new ItemReward { Item = ItemType.Adrenaline },          new ItemReward { Item = ItemType.SCP330 },          new ItemReward { Item = ItemType.SCP244b } } },
                        { 26, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP500 },                                          new ItemReward { Item = ItemType.Adrenaline },      new ItemReward { Item = ItemType.SCP330 } } },
                    },

                    AmmoOverflowAction = OverflowAction.Clamp,
                    AmmoTable = new SortedDictionary<int, List<AmmoReward>>
                    {
                        {0, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Set, Stat = AmmoStat.Inventory, Proportion = 1.0f } } },
                        {1, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.2f } } },
                        {2, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.2f } } },
                        {3, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.2f } } },
                        {4, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.25f } } },
                        {5, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.25f } } },
                        {6, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.25f } } },
                        {7, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.3f } } },
                        {8, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.3f } } },
                        {9, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.3f } } },
                        {10, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.35f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.35f } } },
                        {11, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.35f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.35f } } },
                        {12, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.35f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.35f } } },
                        {13, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.1f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.1f } } },
                        {14, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.1f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.1f } } },
                        {15, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.1f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.1f } } },
                        {16, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.15f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.15f } } },
                        {17, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.15f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.15f } } },
                        {18, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.15f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.15f } } },
                        {19, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.25f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.25f } } },
                        {20, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.25f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.25f } } },
                        {21, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.25f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.25f } } },
                        {22, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.4f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.4f } } },
                        {23, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.4f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.4f } } },
                        {24, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.4f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.4f } } },
                        {25, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.5f }, new AmmoReward { Action= AmmoAction.Add, Stat = AmmoStat.Gun, Proportion = 0.5f } } }
                    },

                    PlayerOverflowAction = OverflowAction.Clamp,
                    PlayerTable = new SortedDictionary<int, List<PlayerReward>>
                    {
                        {5, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {6, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {7, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {8, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {9, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 20 } } },
                        {10, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {11, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {12, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {13, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {14, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 40 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 10 } } },
                        {15, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {16, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {17, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {18, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {19, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 60 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 20 }, new PlayerReward {Stat = PlayerStat.HP, Value = 15} } },
                        {20, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 80 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 30 }, new PlayerReward {Stat = PlayerStat.HP, Value = 30} } },
                        {21, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 80 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 30 }, new PlayerReward {Stat = PlayerStat.HP, Value = 30} } },
                        {22, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 80 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 30 }, new PlayerReward {Stat = PlayerStat.HP, Value = 30} } },
                        {23, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 80 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 30 }, new PlayerReward {Stat = PlayerStat.HP, Value = 30} } },
                        {24, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 80 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 30 }, new PlayerReward {Stat = PlayerStat.HP, Value = 30} } },
                        {25, new List<PlayerReward>{ new PlayerReward {Stat = PlayerStat.Stamina, Value = 100 }, new PlayerReward {Stat = PlayerStat.AHP, Value = 40 }, new PlayerReward {Stat = PlayerStat.HP, Value = 45} } },
                    },

                    EffectOverflowAction = OverflowAction.Clamp,
                    EffectTable = new SortedDictionary<int, List<EffectReward>>
                    {
                        {0, new List<EffectReward>{ new EffectReward {Effect ="Hemorrhage", Intensity = 1 }, new EffectReward { Effect = "Burned", Intensity = 1}, new EffectReward { Effect = "Exhasted", Intensity = 1 },new EffectReward { Effect = "Disabled", Intensity = 1}, new EffectReward { Effect = "Bleeding", Intensity = 1} } },
                        {3, new List<EffectReward>{ new EffectReward {Effect = "Bleeding", Intensity = 0 } } },
                        {5, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 10 } } },
                        {6, new List<EffectReward>{ new EffectReward {Effect = "Disabled", Intensity = 0 } } },
                        {9, new List<EffectReward>{ new EffectReward {Effect = "Exhausted", Intensity = 0 } } },
                        {10, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 20 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 1 } } },
                        {12, new List<EffectReward>{ new EffectReward {Effect = "Burned", Intensity = 0 } } },
                        {15, new List<EffectReward>{ new EffectReward {Effect = "Heomorrhage", Intensity = 0 }, new EffectReward {Effect = "MovementBoost", Intensity = 30 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 2 }, new EffectReward { Effect = "Scp1853", Intensity = 1 } } },
                        {20, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 40 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 3 }, new EffectReward { Effect = "Scp1853", Intensity = 2 } } },
                        {21, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 40 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 3 }, new EffectReward { Effect = "Scp1853", Intensity = 2 } } },
                        {22, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 40 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 3 }, new EffectReward { Effect = "Scp1853", Intensity = 2 } } },
                        {23, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 40 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 3 }, new EffectReward { Effect = "Scp1853", Intensity = 2 } } },
                        {24, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 40 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 3 }, new EffectReward { Effect = "Scp1853", Intensity = 2 } } },
                        {25, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 50 }, new EffectReward { Effect = "BodyshotReduction", Intensity = 4 }, new EffectReward { Effect = "Scp1853", Intensity = 3 } } },
                    }
                }
            }
        };
    }

    public class Killstreaks
    {
        public static Killstreaks Singleton { get; private set; }

        public KillstreakConfig config;

        public class Killstreak
        {
            public string name = "";
            public int count = 0;
        }

        public static Dictionary<int, Killstreak> player_killstreak = new Dictionary<int, Killstreak>();

        public Killstreaks()
        {
            Singleton = this;
        }

        public void Init(KillstreakConfig config)
        {
            this.config = config;
            if (!config.KillstreakTables.ContainsKey(config.DefaultKillstreak))
                throw new System.Exception("Killstreak Config Error: KillstreakTables must contain the DefaultKillstreak");
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            if (!player_killstreak.ContainsKey(player.PlayerId))
                player_killstreak.Add(player.PlayerId, new Killstreak { name = config.DefaultKillstreak });
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            if (player_killstreak.ContainsKey(player.PlayerId))
            {
                Database.Singleton.SaveConfigKillstreak(player);
                player_killstreak.Remove(player.PlayerId);
            }
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDeath(Player victim, Player killer, DamageHandlerBase damage)
        {
            if (killer != null && player_killstreak.ContainsKey(killer.PlayerId))
            {
                Killstreak killstreak = player_killstreak[killer.PlayerId];
                killstreak.count++;
                KillstreakRewardTable table = config.KillstreakTables[killstreak.name];

                if (!table.ItemTable.IsEmpty())
                {
                    int item_index = CalculateIndex(killstreak.count, table.ItemTable.Last().Key, table.ItemOverflowAction);
                    if (table.ItemTable.ContainsKey(item_index))
                        foreach (ItemReward reward in table.ItemTable[item_index])
                            GrantItemReward(killer, reward);
                }

                if(!table.AmmoTable.IsEmpty())
                {
                    int ammo_index = CalculateIndex(killstreak.count, table.AmmoTable.Last().Key, table.AmmoOverflowAction);
                    if(table.AmmoTable.ContainsKey(ammo_index))
                    {
                        Firearm firearm = killer.CurrentItem as Firearm;
                        if (firearm != null && damage is FirearmDamageHandler fdh && firearm.ItemTypeId == fdh.WeaponType && !(firearm is ParticleDisruptor))
                            foreach (AmmoReward reward in table.AmmoTable[ammo_index])
                                GrantAmmoReward(killer, firearm, reward);
                    }
                }

                if(!table.PlayerTable.IsEmpty())
                {
                    int player_index = CalculateIndex(killstreak.count, table.PlayerTable.Last().Key, table.PlayerOverflowAction);
                    if(table.PlayerTable.ContainsKey(player_index))
                        foreach (PlayerReward reward in table.PlayerTable[player_index])
                            GrantPlayerReward(killer, reward);
                }

                if (!table.EffectTable.IsEmpty())
                {
                    int effect_index = CalculateIndex(killstreak.count, table.EffectTable.Last().Key, table.EffectOverflowAction);
                    if (table.EffectTable.ContainsKey(effect_index))
                        foreach (EffectReward reward in table.EffectTable[effect_index])
                            GrantEffectReward(killer, reward);
                }

                string killstreak_name = "<color=" + table.ColorHex + ">" + killstreak.name + "</color>";
                if (killstreak.count % 5 == 0)
                    BroadcastOverride.BroadcastLine(1, killstreak.count, BroadcastPriority.Medium, translation.GlobalKillstreak.Replace("{killstreak}", killstreak_name).Replace("{name}", killer.Nickname).Replace("{count}", killstreak.count.ToString()));
                else
                    BroadcastOverride.BroadcastLine(killer, 2, 3, BroadcastPriority.Low, translation.PrivateKillstreak.Replace("{count}", killstreak.count.ToString()));
            }
            if (player_killstreak.ContainsKey(victim.PlayerId))
            {
                Killstreak victim_killstreak = player_killstreak[victim.PlayerId];
                string victim_killstreak_name = "<color=" + config.KillstreakTables[victim_killstreak.name].ColorHex + ">" + victim_killstreak.name + "</color>";

                if (victim_killstreak.count >= 5)
                {
                    string killer_killstreak_name = "";
                    if (killer == null)
                        killer_killstreak_name = victim_killstreak_name;
                    else
                    {
                        Killstreak killer_killstreak = player_killstreak[killer.PlayerId];
                        killer_killstreak_name = "<color=" + config.KillstreakTables[killer_killstreak.name].ColorHex + ">" + killer_killstreak.name + "</color>";
                    }
                    BroadcastOverride.BroadcastLine(2, victim_killstreak.count, BroadcastPriority.Medium, translation.GlobalKillstreakEnded.Replace("{victim_killstreak}", victim_killstreak_name).Replace("{killer_killstreak}", killer_killstreak_name).Replace("{killer}", killer.Nickname).Replace("{count}", victim_killstreak.count.ToString()).Replace("{victim}", victim.Nickname));
                }
                victim_killstreak.count = 0;
            }
        }

        public static Killstreak GetKillstreak(Player player)
        {
            return player_killstreak[player.PlayerId];
        }

        public string KillstreakColorCode(Player player)
        {
            Killstreak killstreak = player_killstreak[player.PlayerId];
            return config.KillstreakTables[killstreak.name].ColorHex;
        }

        public bool IsLoadoutLocked(Player player)
        {
            Killstreak killstreak = player_killstreak[player.PlayerId];
            return config.KillstreakTables[killstreak.name].LoadoutLock;
        }

        public void AddKillstreakStartItems(Player player)
        {
            Killstreak killstreak = player_killstreak[player.PlayerId];
            KillstreakRewardTable table = config.KillstreakTables[killstreak.name];

            if (!table.ItemTable.IsEmpty())
            {
                int item_index = CalculateIndex(killstreak.count, table.ItemTable.Last().Key, table.ItemOverflowAction);
                if (table.ItemTable.ContainsKey(item_index))
                    foreach (ItemReward reward in table.ItemTable[item_index])
                        GrantItemReward(player, reward);
            }

            if (!table.AmmoTable.IsEmpty())
            {
                int ammo_index = CalculateIndex(killstreak.count, table.AmmoTable.Last().Key, table.AmmoOverflowAction);
                if (table.AmmoTable.ContainsKey(ammo_index))
                    foreach (ItemBase item in player.ReferenceHub.inventory.UserInventory.Items.Values)
                        if (item is Firearm firearm && !(firearm is ParticleDisruptor))
                            foreach (AmmoReward reward in table.AmmoTable[ammo_index])
                                GrantAmmoReward(player, firearm, reward);
            }
        }

        public void AddKillstreakEffects(Player player)
        {
            Killstreak killstreak = player_killstreak[player.PlayerId];
            player.EffectsManager.DisableAllEffects();
            KillstreakRewardTable table = config.KillstreakTables[killstreak.name];

            if (!table.PlayerTable.IsEmpty())
            {
                int player_index = CalculateIndex(killstreak.count, table.PlayerTable.Last().Key, table.PlayerOverflowAction);
                if (table.PlayerTable.ContainsKey(player_index))
                    foreach (PlayerReward reward in table.PlayerTable[player_index])
                        GrantPlayerReward(player, reward);
            }

            if (!table.EffectTable.IsEmpty())
            {
                int effect_index = CalculateIndex(killstreak.count, table.EffectTable.Last().Key, table.EffectOverflowAction);
                if (table.EffectTable.ContainsKey(effect_index))
                    foreach (EffectReward reward in table.EffectTable[effect_index])
                        GrantEffectReward(player, reward);
            }
        }

        public ItemType ArmorType(Player player)
        {
            Killstreak killstreak = player_killstreak[player.PlayerId];
            KillstreakRewardTable table = config.KillstreakTables[killstreak.name];
            if(table.ItemTable.ContainsKey(0))
            {
                if (table.ItemTable[0].Any(x => x.Item == ItemType.ArmorLight))
                    return ItemType.ArmorLight;
                else if (table.ItemTable[0].Any(x => x.Item == ItemType.ArmorCombat))
                    return ItemType.ArmorCombat;
                else if (table.ItemTable[0].Any(x => x.Item == ItemType.ArmorHeavy))
                    return ItemType.ArmorHeavy;
            }
            return ItemType.None;
        }

        private void GrantPlayerReward(Player player, PlayerReward reward)
        {
            switch (reward.Action)
            {
                case PlayerAction.Add:
                    switch (reward.Stat)
                    {
                        case PlayerStat.HP:
                            player.Health = UnityEngine.Mathf.Clamp(player.Health + reward.Value, 0.0f, player.MaxHealth);
                            break;
                        case PlayerStat.AHP:
                            AhpStat ahp = null;
                            if (player.ReferenceHub.playerStats.TryGetModule(out ahp))
                                ahp.ServerAddProcess(reward.Value, AhpStat.DefaultMax, AhpStat.DefaultDecay, AhpStat.DefaultEfficacy, reward.Sustain, reward.Persistent);
                            break;
                        case PlayerStat.Stamina:
                            StaminaStat s = null;
                            if (player.ReferenceHub.playerStats.TryGetModule(out s))
                                s.ModifyAmount(UnityEngine.Mathf.Clamp(s.CurValue + reward.Value, s.MinValue, s.MaxValue));
                            break;
                    }
                    break;
                case PlayerAction.Remove:
                    switch (reward.Stat)
                    {
                        case PlayerStat.HP:
                            player.Health = UnityEngine.Mathf.Clamp(player.Health - reward.Value, 0.0f, player.MaxHealth);
                            break;
                        case PlayerStat.AHP:
                            AhpStat ahp = null;
                            if (player.ReferenceHub.playerStats.TryGetModule(out ahp))
                            {
                                float remaining = reward.Value;
                                while (!ahp._activeProcesses.IsEmpty())
                                {
                                    AhpStat.AhpProcess process = ahp._activeProcesses.Last();
                                    process.CurrentAmount -= remaining;
                                    if (process.CurrentAmount <= 0)
                                    {
                                        remaining = -process.CurrentAmount;
                                        ahp.ServerKillProcess(process.KillCode);
                                    }
                                    else
                                    {
                                        //ahp.ServerUpdateProcesses();
                                        break;
                                    }
                                }
                            }
                            break;
                        case PlayerStat.Stamina:
                            StaminaStat s = null;
                            if (player.ReferenceHub.playerStats.TryGetModule(out s))
                                s.ModifyAmount(UnityEngine.Mathf.Clamp(s.CurValue - reward.Value, s.MinValue, s.MaxValue));
                            break;
                    }
                    break;
                case PlayerAction.Set:
                    switch (reward.Stat)
                    {
                        case PlayerStat.HP:
                            player.Health = UnityEngine.Mathf.Clamp(reward.Value, 0.0f, player.MaxHealth);
                            break;
                        case PlayerStat.AHP:
                            AhpStat ahp = null;
                            if (player.ReferenceHub.playerStats.TryGetModule(out ahp))
                            {
                                while (!ahp._activeProcesses.IsEmpty())
                                    ahp.ServerKillProcess(ahp._activeProcesses.Last().KillCode);
                                ahp.ServerAddProcess(reward.Value, AhpStat.DefaultMax, AhpStat.DefaultDecay, AhpStat.DefaultEfficacy, reward.Sustain, reward.Persistent);
                                //ahp.ServerUpdateProcesses();
                            }
                            break;
                        case PlayerStat.Stamina:
                            StaminaStat s = null;
                            if (player.ReferenceHub.playerStats.TryGetModule(out s))
                                s.ModifyAmount(UnityEngine.Mathf.Clamp(reward.Value, s.MinValue, s.MaxValue));
                            break;
                    }
                    break;
            }
        }

        private void GrantAmmoReward(Player player, Firearm firearm, AmmoReward reward)
        {
            switch (reward.Action)
            {
                case AmmoAction.Add:
                    if (reward.Stat == AmmoStat.Inventory)
                        GrantAmmo(player, firearm.AmmoType, reward.Proportion);
                    else if (reward.Stat == AmmoStat.Gun)
                        GrantWeaponAmmo(firearm, reward.Proportion);
                    break;
                case AmmoAction.Remove:
                    if (reward.Stat == AmmoStat.Inventory)
                        GrantAmmo(player, firearm.AmmoType, -reward.Proportion);
                    else if (reward.Stat == AmmoStat.Gun)
                        GrantWeaponAmmo(firearm, -reward.Proportion);
                    break;
                case AmmoAction.Set:
                    if (reward.Stat == AmmoStat.Inventory)
                        player.SetAmmo(firearm.AmmoType, (ushort)UnityEngine.Mathf.Clamp(player.GetAmmoLimit(firearm.AmmoType) * reward.Proportion, 0.0f, player.GetAmmoLimit(firearm.AmmoType)));
                    else if (reward.Stat == AmmoStat.Gun)
                        firearm.Status = new FirearmStatus((byte)UnityEngine.Mathf.Clamp(firearm.AmmoManagerModule.MaxAmmo * reward.Proportion, 0.0f, firearm.AmmoManagerModule.MaxAmmo), firearm.Status.Flags, firearm.Status.Attachments);
                    break;
            }
        }

        private void GrantEffectReward(Player player, EffectReward reward)
        {
            player.EffectsManager.ChangeState(reward.Effect, reward.Intensity, reward.Duration);
        }

        private void GrantItemReward(Player player, ItemReward reward)
        {
            switch (reward.Action)
            {
                case InventoryAction.Add:
                    GrantItem(player, reward.Item);
                    break;
                case InventoryAction.Remove:
                    RemoveItem(player, reward.Item);
                    break;
            }
        }

        private int CalculateIndex(int index, int max, OverflowAction action)
        {
            if (index > max)
            {
                switch (action)
                {
                    case OverflowAction.End: index = -1; break;
                    case OverflowAction.Rollover: index = ((index - 1) % max) + 1; break;
                    case OverflowAction.Clamp: index = max; break;
                }
            }
            return index;
        }

        private void GrantItem(Player player, ItemType item)
        {
            if (IsGun(item))
            {
                if (player.IsInventoryFull)
                    if (!RemoveItem(player, ItemType.Painkillers))
                        if (!RemoveItem(player, ItemType.Medkit))
                            if (!RemoveItem(player, ItemType.Adrenaline))
                                if (!RemoveItem(player, ItemType.SCP500))
                                    if (!RemoveItem(player, ItemType.SCP244a))
                                        if (!RemoveItem(player, ItemType.SCP244b))
                                            if (!RemoveItem(player, ItemType.SCP018))
                                                return;
                GrantAmmo(player, item, 1.0f);
                GrantFirearm(player, item);
            }
            else
            {
                player.AddItem(item);
            }
        }

        private void GrantWeaponAmmo(Firearm firearm, float proportion)
        {
            byte ammo = (byte)UnityEngine.Mathf.Clamp(firearm.Status.Ammo + (firearm.AmmoManagerModule.MaxAmmo * proportion), 0.0f, firearm.AmmoManagerModule.MaxAmmo * proportion);
            firearm.Status = new FirearmStatus(ammo, firearm.Status.Flags, firearm.Status.Attachments);
        }

        private void GrantAmmo(Player player, ItemType firearm_type, float proportion)
        {
            ItemType ammo_type = GunAmmoType(firearm_type);
            if (ammo_type != ItemType.None)
                player.SetAmmo(ammo_type, (ushort)UnityEngine.Mathf.Clamp(player.GetAmmo(ammo_type) + (player.GetAmmoLimit(ammo_type) * proportion), 0.0f, player.GetAmmoLimit(ammo_type)));
        }

        private void GrantFirearm(Player player, ItemType type)
        {
            Firearm firearm = player.AddItem(type) as Firearm;
            uint code = firearm.Status.Attachments;
            if (firearm is ParticleDisruptor)
            {
                firearm.Status = new FirearmStatus(5, firearm.Status.Flags, code);
            }
            else
            {
                if (AttachmentsServerHandler.PlayerPreferences[player.ReferenceHub].ContainsKey(type))
                    code = AttachmentsServerHandler.PlayerPreferences[player.ReferenceHub][type];
                ushort ammo_reserve = player.GetAmmo(firearm.AmmoType);
                byte ammo_loaded = (byte)UnityEngine.Mathf.Min(ammo_reserve, firearm.AmmoManagerModule.MaxAmmo);
                player.SetAmmo(firearm.AmmoType, (ushort)(ammo_reserve - ammo_loaded));
                firearm.Status = new FirearmStatus(ammo_loaded, FirearmStatusFlags.MagazineInserted, code);
            }
        }
    }
}
