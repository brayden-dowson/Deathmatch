using InventorySystem.Items;
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
    public class Effect
    {
        public string name { get; set; }
        public byte intensity { get; set; }

        public Effect()
        {
        }

        public Effect(string name, byte intensity)
        {
            this.name = name;
            this.intensity = intensity;
        }
    }

    public enum InventoryAction { Add, Remove }
    public class ItemReward
    {
        public InventoryAction Action { get; set; } = InventoryAction.Add;
        public ItemType Item { get; set; }
    }

    public enum EffectAction { Enable, Disable, Change }
    public class EffectReward
    {
        public EffectAction Action { get; set; } = EffectAction.Change;
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

    public enum PlayerAction { Add, Remove, Set, AddTemp}
    public enum PlayerStat { HP, AHP, Stamina }
    public class PlayerReward
    {
        public PlayerAction Action { get; set; } = PlayerAction.Add;
        public PlayerStat Stat { get; set; }
        public float Value { get; set; }
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

        public OverflowAction EffectOverflowAction { get; set; }
        public SortedDictionary<int, List<EffectReward>> EffectTable { get; set; }

        public OverflowAction AmmoOverflowAction { get; set; }
        public SortedDictionary<int, List<AmmoReward>> AmmoTable { get; set; }

        public OverflowAction PlayerOverflowAction { get; set; }
        public SortedDictionary<int, List<PlayerReward>> PlayerTable { get; set; }
    }

    public class KillstreakConfig
    {
        Dictionary<string, KillstreakRewardTable> KillstreakTables = new Dictionary<string, KillstreakRewardTable>
        {
            {
                "Noob",
                new KillstreakRewardTable
                {
                    MenuItem = ItemType.KeycardJanitor,
                    MenuDescription = "",
                    LoadoutLock = false,
                    ColorHex = "#FFFFFF",

                    ItemOverflowAction = OverflowAction.End,
                    ItemTable = new SortedDictionary<int, List<ItemReward>>
                    {
                        { 0, new List<ItemReward>{ new ItemReward {Item = ItemType.ArmorHeavy }, new ItemReward {Item = ItemType.Medkit }, new ItemReward{ Item = ItemType.Medkit }, new ItemReward { Item = ItemType.Medkit } } },
                        { 2, new List<ItemReward>{ new ItemReward {Item = ItemType.Painkillers } } },
                        { 5, new List<ItemReward>{ new ItemReward {Item = ItemType.GrenadeFlash } } }
                    },

                    EffectOverflowAction = OverflowAction.End,
                    EffectTable = new SortedDictionary<int, List<EffectReward>>{},

                    AmmoOverflowAction = OverflowAction.Rollover,
                    AmmoTable = new SortedDictionary<int, List<AmmoReward>>
                    {
                        {0, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Set, Stat = AmmoStat.Inventory, Proportion = 1.0f } } },
                        {1, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.1f } } }
                    },

                    PlayerOverflowAction = OverflowAction.End,
                    PlayerTable = new SortedDictionary<int, List<PlayerReward>>{}
                }
            },
            {
                "Novice",
                new KillstreakRewardTable
                {
                    MenuItem = ItemType.KeycardScientist,
                    MenuDescription = "",
                    LoadoutLock = false,
                    ColorHex = "#FFFFFF",

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

                    EffectOverflowAction = OverflowAction.End,
                    EffectTable = new SortedDictionary<int, List<EffectReward>>{},

                    AmmoOverflowAction = OverflowAction.Rollover,
                    AmmoTable = new SortedDictionary<int, List<AmmoReward>>
                    {
                        {0, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Set, Stat = AmmoStat.Inventory, Proportion = 1.0f } } },
                        {1, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.15f } } }
                    },

                    PlayerOverflowAction = OverflowAction.End,
                    PlayerTable = new SortedDictionary<int, List<PlayerReward>>{}
                }
            },
            {
                "Intermediate",
                new KillstreakRewardTable
                {
                    MenuItem = ItemType.KeycardNTFOfficer,
                    MenuDescription = "",
                    LoadoutLock = false,
                    ColorHex = "#FFFFFF",

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

                    EffectOverflowAction = OverflowAction.End,
                    EffectTable = new SortedDictionary<int, List<EffectReward>>{},

                    AmmoOverflowAction = OverflowAction.Rollover,
                    AmmoTable = new SortedDictionary<int, List<AmmoReward>>
                    {
                        {0, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Set, Stat = AmmoStat.Inventory, Proportion = 0.5f } } },
                        {1, new List<AmmoReward>{ new AmmoReward {Action = AmmoAction.Add, Stat = AmmoStat.Inventory, Proportion = 0.2f } } }
                    },

                    PlayerOverflowAction = OverflowAction.End,
                    PlayerTable = new SortedDictionary<int, List<PlayerReward>>{}
                }
            },
            {
                "Advanced",
                new KillstreakRewardTable
                {
                    MenuItem = ItemType.KeycardNTFLieutenant,
                    MenuDescription = "",
                    LoadoutLock = false,
                    ColorHex = "#FFFFFF",

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

                    EffectOverflowAction = OverflowAction.End,
                    EffectTable = new SortedDictionary<int, List<EffectReward>>
                    {
                        {7, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 10 } } },
                        {15, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 15 } } },
                        {20, new List<EffectReward>{ new EffectReward {Effect = "MovementBoost", Intensity = 20 } } }
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
                    }
                }
            },
            {
                "Expert",
                new KillstreakRewardTable
                {
                    MenuItem = ItemType.KeycardNTFCommander,
                    MenuDescription = "",
                    LoadoutLock = true,
                    ColorHex = "#FFFFFF",

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
                    }
                }
            },
            {
                "RAGE",
                new KillstreakRewardTable
                {
                    MenuItem = ItemType.KeycardChaosInsurgency,
                    MenuDescription = "",
                    LoadoutLock = true,
                    ColorHex = "#FFFFFF",

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
                        { 18, new List<ItemReward>{ new ItemReward { Item = ItemType.Painkillers },                                     new ItemReward { Item = ItemType.SCP330 } } },
                        { 19, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 20, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP018 },                                          new ItemReward { Item = ItemType.ParticleDisruptor } } },
                        { 21, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 22, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP500 },                                          new ItemReward { Item = ItemType.SCP330 } } },
                        { 23, new List<ItemReward>{ new ItemReward { Item = ItemType.Adrenaline },                                      new ItemReward { Item = ItemType.SCP330 } } },
                        { 24, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP500 },                                          new ItemReward { Item = ItemType.SCP330 },          new ItemReward { Item = ItemType.Jailbird } } },
                        { 25, new List<ItemReward>{ new ItemReward { Action = InventoryAction.Remove, Item = ItemType.ArmorCombat },    new ItemReward { Item = ItemType.ArmorHeavy },      new ItemReward { Item = ItemType.GunShotgun },                                      new ItemReward { Item = ItemType.SCP500 },          new ItemReward { Item = ItemType.Adrenaline },          new ItemReward { Item = ItemType.SCP330 },          new ItemReward { Item = ItemType.SCP244b } } },
                        { 26, new List<ItemReward>{ new ItemReward { Item = ItemType.SCP500 },                                          new ItemReward { Item = ItemType.Adrenaline },      new ItemReward { Item = ItemType.SCP330 } } },
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
                    }
                }
            }
        };
    }

    public class KillstreakConfig
    {
        public List<List<ItemType>> EasyKillstreakTable { get; set; } = new List<List<ItemType>>
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
            new List<ItemType>{ItemType.GrenadeFlash }
        };

        public List<List<ItemType>> StandardKillstreakTable { get; set; } = new List<List<ItemType>>
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

        public List<List<ItemType>> ExpertKillstreakTable { get; set; } = new List<List<ItemType>>
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

        public List<List<ItemType>> RageKillstreakTable { get; set; } = new List<List<ItemType>>
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

        public List<List<ItemType>> RageKillstreakLoadout { get; set; } = new List<List<ItemType>>
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

        public List<List<Effect>> RageKillstreakStatusEffects { get; set; } = new List<List<Effect>>
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
    }

    public class Killstreaks
    {
        public static Killstreaks Singleton { get; private set; }

        public KillstreakConfig config;

        public enum KillstreakMode { Easy, Standard, Expert, Rage }
        public class Killstreak
        {
            public KillstreakMode mode = KillstreakMode.Standard;
            public int count = 0;
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

        public static Dictionary<int, Killstreak> player_killstreak = new Dictionary<int, Killstreak>();

        public Killstreaks()
        {
            Singleton = this;
        }

        public void Init(KillstreakConfig config)
        {
            this.config = config;
        }

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
            {
                Database.Singleton.SaveConfigKillstreak(player);
                player_killstreak.Remove(player.PlayerId);
            }
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDeath(Player target, Player killer, DamageHandlerBase damage)
        {
            if (killer != null && player_killstreak.ContainsKey(killer.PlayerId))
            {
                Killstreak killstreak = player_killstreak[killer.PlayerId];
                if (killstreak.mode == KillstreakMode.Rage)
                {
                    List<ItemType> previous_loadout = config.RageKillstreakLoadout[math.min(config.RageKillstreakLoadout.Count, killstreak.count)];
                    List<ItemType> new_loadout = config.RageKillstreakLoadout[math.min(config.RageKillstreakLoadout.Count, killstreak.count + 1)];
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

                    List<Effect> previous_effects = config.RageKillstreakStatusEffects[math.min(config.RageKillstreakStatusEffects.Count, killstreak.count)];
                    List<Effect> new_effects = config.RageKillstreakStatusEffects[math.min(config.RageKillstreakStatusEffects.Count, killstreak.count + 1)];
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
                        reward_items = config.EasyKillstreakTable[killstreak.count % config.EasyKillstreakTable.Count];
                        break;
                    case KillstreakMode.Standard:
                        reward_items = config.StandardKillstreakTable[killstreak.count % config.StandardKillstreakTable.Count];
                        break;
                    case KillstreakMode.Expert:
                        reward_items = config.ExpertKillstreakTable[killstreak.count % config.ExpertKillstreakTable.Count];
                        break;
                    case KillstreakMode.Rage:
                        reward_items = config.RageKillstreakTable[killstreak.count % config.RageKillstreakTable.Count];
                        break;
                    default:
                        reward_items = config.StandardKillstreakTable[killstreak.count % config.StandardKillstreakTable.Count];
                        break;
                }

                AddItems(killer, reward_items);
                killstreak.count++;
                if (killstreak.count % 5 == 0)
                    BroadcastOverride.BroadcastLine(1, killstreak.count, BroadcastPriority.Medium, translation.GlobalKillstreak.Replace("{name}", killer.Nickname).Replace("{streak}", killstreak.count.ToString()));
                else
                    BroadcastOverride.BroadcastLine(killer, 2, 3, BroadcastPriority.Low, translation.PrivateKillstreak.Replace("{streak}", killstreak.count.ToString()));
            }
            if (player_killstreak.ContainsKey(target.PlayerId))
            {
                Killstreak killstreak = player_killstreak[target.PlayerId];
                if (killstreak.count >= 5)
                    BroadcastOverride.BroadcastLine(2, killstreak.count, BroadcastPriority.Medium, translation.GlobalKillstreakEnded.Replace("{killer}", killer.Nickname).Replace("{streak}", killstreak.count.ToString()).Replace("{victim}",target.Nickname));
                killstreak.count = 0;
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

        public void AddKillstreakStartItems(Player player)
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
                foreach (ItemType item in config.RageKillstreakLoadout[0])
                {
                    if (IsGun(item))
                        AddFirearm(player, item, true);
                    else
                        player.AddItem(item);
                }
            }
        }

        public void AddKillstreakEffects(Player player)
        {
            Killstreak killstreak = player_killstreak[player.PlayerId];
            player.EffectsManager.DisableAllEffects();

            if (killstreak.mode == KillstreakMode.Rage)
            {
                foreach (Effect effect in config.RageKillstreakStatusEffects[0])
                {
                    player.EffectsManager.ChangeState(effect.name, effect.intensity);
                }
            }
        }
    }
}
