using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using MapGeneration;
using Interactables.Interobjects.DoorUtils;
using Interactables.Interobjects;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;

//todo voice and spectate cmd
namespace TheRiptide
{
    public class Deathmatch
    {
        public static Deathmatch Singleton { get; private set; }

        private static bool game_started = false;
        public static SortedSet<int> players = new SortedSet<int>();
        public static float round_time = 30.0f;

        public static bool GameStarted
        {
            get => game_started;
            set
            {
                if(value == true)
                {
                    foreach (var player in Player.GetPlayers())
                        if (player.IsAlive)
                            Killstreaks.AddKillstreakEffects(player);
                }
                else
                {
                    foreach (var player in Player.GetPlayers())
                        if (player.IsAlive)
                            Lobby.ApplyGameNotStartedEffects(player);
                }
                game_started = value;
            }
        }

        public Deathmatch()
        {
            Singleton = this;
            Killfeeds.Init(2, 5, 20);
            DeathmatchMenu.SetupMenus();
            Lobby.Init();
        }

        [PluginEntryPoint("Deathmatch", "1.0", "needs no explanation", "The Riptide")]
        void EntryPoint()
        {
            EventManager.RegisterEvents(this);
            //dependecies
            EventManager.RegisterEvents<InventoryMenu>(this);
            EventManager.RegisterEvents<BroadcastOverride>(this);
            EventManager.RegisterEvents<FacilityManager>(this);

            //features
            EventManager.RegisterEvents<DataBase>(this);
            EventManager.RegisterEvents<Statistics>(this);
            EventManager.RegisterEvents<Killfeeds>(this);
            EventManager.RegisterEvents<Killstreaks>(this);
            EventManager.RegisterEvents<Loadouts>(this);
            EventManager.RegisterEvents<Lobby>(this);
            EventManager.RegisterEvents<Rooms>(this);
        }

        [PluginReload]
        void Reload()
        {
            EventManager.RegisterEvents(this);
            //dependecies
            EventManager.RegisterEvents<InventoryMenu>(this);
            EventManager.RegisterEvents<BroadcastOverride>(this);
            EventManager.RegisterEvents<FacilityManager>(this);

            //features
            EventManager.RegisterEvents<DataBase>(this);
            EventManager.RegisterEvents<Statistics>(this);
            EventManager.RegisterEvents<Killfeeds>(this);
            EventManager.RegisterEvents<Killstreaks>(this);
            EventManager.RegisterEvents<Loadouts>(this);
            EventManager.RegisterEvents<Lobby>(this);
            EventManager.RegisterEvents<Rooms>(this);
        }

        [PluginUnload]
        void Unload()
        {
            DataBase.PluginUnload();

            //features
            EventManager.UnregisterEvents<Rooms>(this);
            EventManager.UnregisterEvents<Lobby>(this);
            EventManager.UnregisterEvents<Loadouts>(this);
            EventManager.UnregisterEvents<Killstreaks>(this);
            EventManager.UnregisterEvents<Killfeeds>(this);
            EventManager.UnregisterEvents<Statistics>(this);
            EventManager.UnregisterEvents<DataBase>(this);

            //dependecies
            EventManager.UnregisterEvents<FacilityManager>(this);
            EventManager.UnregisterEvents<BroadcastOverride>(this);
            EventManager.UnregisterEvents<InventoryMenu>(this);

            EventManager.UnregisterEvents(this);
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        void WaitingForPlayers()
        {
        }

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            //foreach (var room in RoomIdentifier.AllRoomIdentifiers)
            //{
            //    SafeTeleportPosition stp = room.GetComponentInParent<SafeTeleportPosition>();
            //    if (stp != null && stp.SafePositions != null && !stp.SafePositions.IsEmpty() && stp.SafePositions[0] != null)
            //    {
            //        Transform transform = stp.SafePositions[0];
            //        ServerConsole.AddLog("room " + room.Name.ToString() + " | " + room.Shape.ToString() + " has a SafeTeleportPosition component " + transform.ToString(), ConsoleColor.White);
            //    }
            //    else
            //        ServerConsole.AddLog("room " + room.Name.ToString() + " | " + room.Shape.ToString() + " does not have a SafeTeleportPosition component", ConsoleColor.Red);
            //}

            Server.Instance.SetRole(RoleTypeId.Scp939);
            Server.Instance.ReferenceHub.nicknameSync.SetNick("[THE RIPTIDE]");
            Server.Instance.Position = new Vector3(128.8f, 994.0f, 18.0f);
            Server.FriendlyFire = true;

            Timing.CallDelayed(1.0f, () =>
            {
                try
                {
                    Server.Instance.ReferenceHub.serverRoles.Permissions = (ulong)PlayerPermissions.FacilityManagement;
                    CommandSystem.Commands.RemoteAdmin.Cleanup.ItemsCommand cmd = new CommandSystem.Commands.RemoteAdmin.Cleanup.ItemsCommand();
                    string response = "";
                    string[] empty = { "" };
                    cmd.Execute(new ArraySegment<string>(empty, 0, 0), new RemoteAdmin.PlayerCommandSender(Server.Instance.ReferenceHub), out response);
                    ServerConsole.AddLog(response);
                }
                catch (Exception ex)
                {
                    ServerConsole.AddLog(ex.ToString());
                }
            });
            if (round_time > 5.0f)
                Timing.CallDelayed(60.0f * (round_time - 5.0f), () => { BroadcastOverride.BroadcastLine(1, 30, BroadcastPriority.Medium, "<color=#43BFF0>Round Ends in 5 minutes</color>"); });
            if (round_time > 1.0f)
                Timing.CallDelayed(60.0f * (round_time - 1.0f), () => { BroadcastOverride.BroadcastLine(1, 30, BroadcastPriority.Medium, "<color=#43BFF0>Round Ends in 1 minute</color>"); });
            Timing.CallDelayed(60.0f * round_time, () => 
            {
                Statistics.DisplayRoundStats();
                Timing.CallPeriodically(20.0f, 0.2f, () =>
                {
                    foreach (var p in Player.GetPlayers())
                        p.EffectsManager.ChangeState<CustomPlayerEffects.DamageReduction>(200, 20.0f, false);
                });
                Timing.CallDelayed(20.0f, () => Round.Restart(false));
            });
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            players.Add(player.PlayerId);
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            players.Remove(player.PlayerId);
        }

        [PluginEvent(ServerEventType.RoundEndConditionsCheck)]
        RoundEndConditionsCheckCancellationData OnRoundEndConditionsCheck(bool baseGameConditionsSatisfied)
        {
            return RoundEndConditionsCheckCancellationData.Override(false);
        }
        [PluginEvent(ServerEventType.RoundRestart)]
        void OnRoundRestart()
        {
            Timing.KillCoroutines();
        }

        public static bool IsPlayerValid(Player player)
        {
            return players.Contains(player.PlayerId);
        }
    }

    //public class Deathmatch
    //{
    //    public static Deathmatch Singleton { get; private set; }

    //    //public class TestPlayer: Player
    //    //{
    //    //    TestPlayer(IGameComponent component):base(component)
    //    //    {
    //    //        EventManager.RegisterEvents(RandomDeathMatch.Singleton, this);
    //    //        this.
    //    //    }


    //    //    public override void OnDestroy()
    //    //    {
    //    //        EventManager.UnregisterEvents(RandomDeathMatch.Singleton, this);
    //    //    }
    //    //}

    //    //public enum MenuPage { None, Main, GunSlot2, GunSlot3, GunClass, MtfGun, ChaosGun, KillstreakMode, KillstreakModeSecret, Preference, Stats, Debug };
    //    //public enum GunSlot { Primary, Secondary, Tertiary };
    //    //public enum KillstreakMode { Easy, Standard, Expert, Rage }

    //    //public class RDMStats
    //    //{
    //    //    public int killstreak = 0;
    //    //    public int highest_killstreak = 0;
    //    //    public int kills = 0;
    //    //    public int headshot_kills = 0;
    //    //    public int deaths = 0;
    //    //    public int shots = 0;
    //    //    public int shots_hit = 0;
    //    //    public int headshots = 0;
    //    //    public int time_alive = 0;
    //    //    public int damage_delt = 0;
    //    //    public int damage_recieved = 0;
    //    //}

    //    public class RDMPlayer
    //    {
    //        //public MenuPage menu_page = MenuPage.None;
    //        //public ItemType primary = ItemType.None;
    //        //public ItemType secondary = ItemType.None;
    //        //public ItemType tertiary = ItemType.None;
    //        //public GunSlot slot = GunSlot.Primary;
    //        //public KillstreakMode killstreak_mode = KillstreakMode.Standard;

    //        //public bool loadout_locked = false;
    //        //public bool customising_loadout = false;
    //        public bool is_spectating = false;
    //        public bool in_spawn = true;
    //        //public bool rage_mode_enabled = false;
    //        //public bool loadout_radio = true;

    //        //public RDMStats stats = new RDMStats();

    //        public CoroutineHandle teleport_handle;
    //        //public float start_time = 0.0f;

    //        //public void SetGunInSlot(ItemType gun)
    //        //{
    //        //    if (slot == GunSlot.Primary)
    //        //        primary = gun;
    //        //    else if (slot == GunSlot.Secondary)
    //        //        secondary = gun;
    //        //    else if (slot == GunSlot.Tertiary)
    //        //        tertiary = gun;
    //        //}
    //        //public bool IsEmptyLoadout { get { return primary == ItemType.None && secondary == ItemType.None && tertiary == ItemType.None && killstreak_mode != KillstreakMode.Rage; } }
    //    }

    //    //class Effect
    //    //{
    //    //    public string name;
    //    //    public byte intensity;

    //    //    public Effect(string name, byte intensity)
    //    //    {
    //    //        this.name = name;
    //    //        this.intensity = intensity;
    //    //    }
    //    //}

    //    public static Dictionary<int, RDMPlayer> players;
    //    public static bool game_started = false;
    //    //todo fix teleport queue
    //    //Queue<Player> teleport_queue = new Queue<Player>();
    //    //CoroutineHandle teleport_queue_coroutine = new CoroutineHandle();

    //    //string loadout_customisation_hint = "<b>CHECK INVENTORY! <color=#FF0000>Right Click O5 to select gun</color></b>";
    //    //string teleport_msg = "<color=#43BFF0>you will be teleported after selecting a gun</color>";
    //    //
    //    //List<string> waiting_for_players_msg = new List<string>(){
    //    //    "<color=#43BFF0>Waiting for 1 player to join</color>",
    //    //    "<color=#43BFF0>You get to choose the starting area!</color>"};
    //    //
    //    //List<string> main_menu_details = new List<string>(){
    //    //    "<b><color=#43BFF0>[MAIN MENU]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>",
    //    //    "<color=#5900ff>[O5]</color> = <b><color=#5900ff>Save and Exit</color></b>",
    //    //    "<color=#e7d77b>[SCIENTIST]</color> = <b><color=#FF0000>Customise Loadout - </color><color=#43BFF0>[GUN SLOT]</color></b>",
    //    //    "<color=#e1ab21>[RESEARCH SUPERVISOR]</color> = <b><color=#43BFF0>[KILLSTREAK REWARD SYSTEM]</color></b>",
    //    //    "<color=#bd8f86>[CONTAINMENT ENGINEER]</color> = <b><color=#43BFF0>[PREFERENCES]</color></b>",};
    //    //
    //    //List<string> gun_slot_menu_details_2_slot = new List<string>(){
    //    //    "<b><color=#43BFF0>[GUN SLOT]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>",
    //    //    "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>",
    //    //    "<color=#bd1a4a>[FACILITY MANAGER]</color> = <b><color=#FF0000>Primary - </color><color=#43BFF0>[GUN CLASS]</color></b>",
    //    //    "<color=#217b7b>[ZONE MANAGER]</color> = <b>Secondary - <color=#43BFF0>[GUN CLASS]</color></b>" };
    //    //
    //    //List<string> gun_slot_menu_details_3_slot = new List<string>(){
    //    //    "<b><color=#43BFF0>[GUN SLOT]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>",
    //    //    "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>",
    //    //    "<color=#1b43cb>[CAPTAIN]</color> = <b><color=#FF0000>Primary - </color><color=#43BFF0>[GUN CLASS]</color></b>",
    //    //    "<color=#177dde>[SERGEANT]</color> = <b>Secondary - <color=#43BFF0>[GUN CLASS]</color></b>",
    //    //    "<color=#accfe1>[PRIVATE]</color> = <b>Tertiary - <color=#43BFF0>[GUN CLASS]</color></b>" };
    //    //
    //    //List<string> gun_class_menu_details = new List<string>() {
    //    //    "<b><color=#43BFF0>[GUN CLASS]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>",
    //    //    "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>",
    //    //    "<color=#1b43cb>[CAPTAIN]</color> = <b><color=#1b43cb>[MTF GUNS]</color></b>",
    //    //    "<color=#008f1c>[CHAOS]</color> = <b><color=#008f1c>[CHAOS GUNS]</color></b>" };
    //    //
    //    //List<string> mtf_gun_menu_details = new List<string>() {
    //    //    "<b><color=#1b43cb>[MTF GUNS]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>",
    //    //    "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>" };

    //    //List<string> chaos_gun_menu_details = new List<string>() {
    //    //    "<b><color=#008f1c>[CHAOS GUNS]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>",
    //    //    "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>" };
    //    //
    //    //List<string> killstreak_mode_details = new List<string>() {
    //    //    "<b><color=#43BFF0>[KILLSTREAK REWARD SYSTEM]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>",
    //    //    "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>",
    //    //    "<color=#eb0d47>[HEAVY ARMOR]</color> = <b><color=#5900ff>Easy</color>: low risk low reward, good loadout but bad killstreak rewards</b>",
    //    //    "<color=#eb0d47>[COMBAT ARMOR]</color> = <b><color=#43BFF0>Standard</color>: medium risk medium reward, ok loadout and ok killsteak rewards</b>",
    //    //    "<color=#eb0d47>[LIGHT ARMOR]</color> = <b><color=#36a832>Expert</color>: high risk high reward, bad loadout but good killstreak rewards </b>",
    //    //    "<color=#eb0d47>[COM 45]</color> = <b><color=#FF0000>RAGE</color> - [DATA EXPUNGED]</b>" };
    //    //
    //    //List<string> preference_menu_details = new List<string>() {
    //    //    "<b><color=#43BFF0>[PREFERENCES]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>",
    //    //    "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>",
    //    //    "<color=#eb0d47>[GUARD]</color> = <b>Toggle Loadout Radio</b>",
    //    //    "<color=#eb0d47>[SCIENTIST]</color> = <b><color=#43BFF0>[STATS]</color></b>",
    //    //    "<color=#eb0d47>[FLASH LIGHT]</color> = <b>Enable spectator mode</b>",
    //    //    "<color=#eb0d47>[COIN]</color> = <b>Enable [DATA EXPUNGED]</b>" };
    //    //
    //    //List<string> stats_menu_details = new List<string>() {
    //    //    "<b><color=#43BFF0>[STATS]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>",
    //    //    "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[PREFERENCES]</color></b>" };
    //    //
    //    ////List<string> loadout_customisation_denied = new List<string>() {
    //    ////    "<color=#f8d107>Loadout can not be customised after shooting gun/using item</color>",
    //    ////    "<color=#43BFF0>Wait until next respawn</color>" };
    //    //
    //    //List<string> debug_menu_details = new List<string>() {
    //    //    "close all rooms",
    //    //    "open all rooms",
    //    //    "open 2 rooms",
    //    //    "close 2 rooms" };



    //    //List<ItemType> main_menu = new List<ItemType> {
    //    //        ItemType.KeycardO5,                                 //exit
    //    //        ItemType.KeycardScientist,                          //select gun
    //    //        ItemType.KeycardResearchCoordinator,                //set killstreak mode
    //    //        ItemType.KeycardContainmentEngineer,                //set preferences
    //    //        ItemType.Radio};

    //    //List<ItemType> main_menu = new List<ItemType> {
    //    //        ItemType.KeycardO5,                                 //exit
    //    //        ItemType.KeycardScientist,                          //select gun
    //    //        ItemType.KeycardResearchCoordinator,                //set killstreak mode
    //    //        ItemType.KeycardContainmentEngineer,                //set preferences
    //    //        ItemType.Coin,                                      //debug
    //    //        ItemType.Radio };

    //    //List<ItemType> gun_slot_menu_2_slot = new List<ItemType> {
    //    //        ItemType.KeycardO5,                                 //back to main menu
    //    //        ItemType.KeycardFacilityManager,                    //primary gun
    //    //        ItemType.KeycardZoneManager,                        //secondary gun
    //    //        ItemType.Radio };


    //    //List<ItemType> gun_slot_menu_3_slot = new List<ItemType> {
    //    //        ItemType.KeycardO5,                                 //back to main menu
    //    //        ItemType.KeycardNTFCommander,                       //primary gun
    //    //        ItemType.KeycardNTFLieutenant,                      //secondary gun
    //    //        ItemType.KeycardNTFOfficer,                         //tertiary gun
    //    //        ItemType.Radio };

    //    //List<ItemType> gun_class_menu = new List<ItemType> {
    //    //        ItemType.KeycardO5,                                 //back to main menu
    //    //        ItemType.KeycardNTFCommander,                       //MTF
    //    //        ItemType.KeycardChaosInsurgency,                    //CHAOS
    //    //        ItemType.Radio };

    //    //List<ItemType> mtf_gun_menu = new List<ItemType> {
    //    //        ItemType.KeycardO5,                                 //back to main menu
    //    //        ItemType.GunCOM15,
    //    //        ItemType.GunCOM18,
    //    //        ItemType.GunFSP9,
    //    //        ItemType.GunCrossvec,
    //    //        ItemType.GunE11SR,
    //    //        ItemType.Radio };

    //    //List<ItemType> chaos_gun_menu = new List<ItemType> {
    //    //        ItemType.KeycardO5,                                 //back to main menu
    //    //        ItemType.GunAK,
    //    //        ItemType.GunLogicer,
    //    //        ItemType.GunShotgun,
    //    //        ItemType.GunRevolver,
    //    //        ItemType.Radio };

    //    //List<ItemType> killstreak_mode_menu = new List<ItemType> {
    //    //        ItemType.KeycardO5,                                 //back to main menu
    //    //        ItemType.ArmorHeavy,                                //easy - grants heavy armour and start meds with the downside of no real progression 
    //    //        ItemType.ArmorCombat,                               //standard - grants combat armour and painkillers with good item progression
    //    //        ItemType.ArmorLight,                                //expert - grants light armour and no start meds with very good item progression
    //    //        ItemType.GunCom45,                                  //RAGE - grants no armour no start meds and negative status effects and no choice of guns but has extreme item and status effect progression
    //    //        ItemType.Radio };


    //    //List<ItemType> preferences_menu = new List<ItemType> {
    //    //        ItemType.KeycardO5,                                 //back to main menu
    //    //        ItemType.KeycardGuard,                              //toggle loadout radio
    //    //        ItemType.KeycardScientist,                          //display stats
    //    //        ItemType.Flashlight,                                //enable spectate mode 
    //    //        ItemType.Coin };                                    //enable rage access mode 

    //    //List<ItemType> stats_menu = new List<ItemType> {
    //    //        ItemType.KeycardO5,                                 //back to preference menu
    //    //        ItemType.Radio };                                 

    //    //List<ItemType> debug_menu = new List<ItemType>(){
    //    //        ItemType.KeycardO5,
    //    //        ItemType.KeycardGuard,                                  //func 1
    //    //        ItemType.KeycardNTFOfficer,                             //func 2
    //    //        ItemType.KeycardNTFLieutenant,                          //func 3
    //    //        ItemType.KeycardNTFCommander };                         //func 4

    //    //static List<List<ItemType>> easy_kill_streak_table = new List<List<ItemType>>
    //    //    {
    //    //        new List<ItemType>{ItemType.Painkillers},                                                                               //1
    //    //        new List<ItemType>{},                                                                                                   //2
    //    //        new List<ItemType>{ItemType.Medkit},                                                                                    //3
    //    //        new List<ItemType>{},                                                                                                   //4
    //    //        new List<ItemType>{ItemType.GrenadeFlash},                                                                              //5
    //    //        new List<ItemType>{},                                                                                                   //6
    //    //        new List<ItemType>{ItemType.Medkit},                                                                                    //7
    //    //        new List<ItemType>{},                                                                                                   //8
    //    //        new List<ItemType>{ItemType.Painkillers},                                                                               //9
    //    //        new List<ItemType>{ItemType.GrenadeHE},                                                                                 //10
    //    //        new List<ItemType>{ItemType.Medkit},                                                                                    //11
    //    //        new List<ItemType>{},                                                                                                   //12
    //    //        new List<ItemType>{ItemType.Painkillers},                                                                               //13
    //    //        new List<ItemType>{},                                                                                                   //14
    //    //        new List<ItemType>{ItemType.Adrenaline},                                                                                //15
    //    //        new List<ItemType>{},                                                                                                   //16
    //    //        new List<ItemType>{ItemType.Painkillers},                                                                               //17
    //    //        new List<ItemType>{},                                                                                                   //18
    //    //        new List<ItemType>{ItemType.Medkit},                                                                                    //19
    //    //        new List<ItemType>{ItemType.GrenadeHE},                                                                                 //20
    //    //        new List<ItemType>{ItemType.Painkillers},                                                                               //21
    //    //        new List<ItemType>{},                                                                                                   //22
    //    //        new List<ItemType>{ItemType.Medkit},                                                                                    //23
    //    //        new List<ItemType>{},                                                                                                   //24
    //    //        new List<ItemType>{ItemType.GrenadeFlash }                                                                              //25
    //    //    };

    //    //static List<List<ItemType>> standard_kill_streak_table = new List<List<ItemType>>
    //    //    {
    //    //        new List<ItemType>{                         ItemType.Painkillers},                                                                              //1
    //    //        new List<ItemType>{                         ItemType.SCP330},                                                                                   //2
    //    //        new List<ItemType>{                         ItemType.Medkit},                                                                                   //3
    //    //        new List<ItemType>{                         ItemType.SCP330},                                                                                   //4
    //    //        new List<ItemType>{ItemType.GrenadeFlash,   ItemType.Adrenaline},                                                                               //5
    //    //        new List<ItemType>{                         ItemType.SCP330},                                                                                   //6
    //    //        new List<ItemType>{                         ItemType.Medkit},                                                                                   //7
    //    //        new List<ItemType>{                         ItemType.Adrenaline},                                                                               //8
    //    //        new List<ItemType>{ItemType.GrenadeHE,      ItemType.Painkillers},                                                                              //9
    //    //        new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330},                                                          //10
    //    //        new List<ItemType>{                         ItemType.Adrenaline},                                                                               //11
    //    //        new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330},                                                          //12
    //    //        new List<ItemType>{ItemType.SCP1853,        ItemType.Painkillers},                                                                              //13
    //    //        new List<ItemType>{                         ItemType.Adrenaline},                                                                               //14
    //    //        new List<ItemType>{                         ItemType.SCP500},                                                                                   //15
    //    //        new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330},                                                          //16
    //    //        new List<ItemType>{ItemType.SCP207,         ItemType.Painkillers,    ItemType.Adrenaline},                                                      //17
    //    //        new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330},                                                          //18
    //    //        new List<ItemType>{                         ItemType.SCP500},                                                                                   //19
    //    //        new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330,       ItemType.SCP330,        ItemType.Adrenaline},       //20
    //    //        new List<ItemType>{ItemType.SCP018,         ItemType.Painkillers},                                                                              //21
    //    //        new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330,       ItemType.SCP330},                                   //22
    //    //        new List<ItemType>{                         ItemType.SCP500,         ItemType.Adrenaline},                                                      //23
    //    //        new List<ItemType>{                         ItemType.SCP330,         ItemType.SCP330,       ItemType.SCP330},                                   //24
    //    //        new List<ItemType>{ItemType.SCP268,         ItemType.Painkillers }                                                                              //25
    //    //    };

    //    //static List<List<ItemType>> expert_kill_streak_table = new List<List<ItemType>>
    //    //    {
    //    //        new List<ItemType>{                                                         ItemType.Painkillers},                                              //1    
    //    //        new List<ItemType>{},                                                                                                                           //2    
    //    //        new List<ItemType>{                                                         ItemType.Painkillers},                                              //3    
    //    //        new List<ItemType>{},                                                                                                                           //4    
    //    //        new List<ItemType>{ItemType.SCP207,                                         ItemType.SCP330},                                                   //5
    //    //        new List<ItemType>{                                                         ItemType.Adrenaline},                                               //6    
    //    //        new List<ItemType>{                                                         ItemType.SCP330},                                                   //7    
    //    //        new List<ItemType>{                         ItemType.Jailbird,              ItemType.Painkillers},                                              //8    
    //    //        new List<ItemType>{                                                         ItemType.SCP330},                                                   //9    
    //    //        new List<ItemType>{ItemType.SCP244a,                                        ItemType.Adrenaline},                                               //10    
    //    //        new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.SCP330,        ItemType.SCP330},   //11   
    //    //        new List<ItemType>{                         ItemType.GunCom45,              ItemType.Painkillers,   ItemType.Adrenaline},                       //12    
    //    //        new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.SCP330,        ItemType.SCP330},   //13    
    //    //        new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.Adrenaline},                       //14    
    //    //        new List<ItemType>{ItemType.SCP268,                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //15    
    //    //        new List<ItemType>{                         ItemType.ParticleDisruptor,     ItemType.Painkillers,   ItemType.Adrenaline},                       //16    
    //    //        new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //17    
    //    //        new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.Adrenaline},                       //18    
    //    //        new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //19     
    //    //        new List<ItemType>{ItemType.SCP018,         ItemType.Jailbird,              ItemType.Painkillers,   ItemType.Adrenaline},                       //20
    //    //        new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //21    
    //    //        new List<ItemType>{                                                         ItemType.SCP500,        ItemType.Adrenaline},                       //22    
    //    //        new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //23    
    //    //        new List<ItemType>{                         ItemType.ParticleDisruptor,     ItemType.SCP500,        ItemType.Adrenaline},                       //24    
    //    //        new List<ItemType>{ItemType.ArmorHeavy,                                     ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330 }   //25    
    //    //    };

    //    //static List<List<ItemType>> rage_kill_streak_table = new List<List<ItemType>>
    //    //    {
    //    //        new List<ItemType>{                                                         ItemType.Painkillers},                                              //1    
    //    //        new List<ItemType>{},                                                                                                                           //2    
    //    //        new List<ItemType>{                                                         ItemType.Painkillers},                                              //3    
    //    //        new List<ItemType>{},                                                                                                                           //4    
    //    //        new List<ItemType>{                                                         ItemType.Adrenaline},                                               //5
    //    //        new List<ItemType>{},                                                                                                                           //6    
    //    //        new List<ItemType>{                                                         ItemType.Painkillers},                                              //7    
    //    //        new List<ItemType>{                         ItemType.Jailbird},                                                                                 //8    
    //    //        new List<ItemType>{                                                         ItemType.Painkillers},                                              //9    
    //    //        new List<ItemType>{ItemType.SCP244a,                                        ItemType.Adrenaline},                                               //10    
    //    //        new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.SCP330},                           //11   
    //    //        new List<ItemType>{                         ItemType.GunCom45,              ItemType.Painkillers,   ItemType.Adrenaline},                       //12    
    //    //        new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.SCP330},                           //13    
    //    //        new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.Adrenaline},                       //14    
    //    //        new List<ItemType>{ItemType.SCP268,                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //15    
    //    //        new List<ItemType>{                         ItemType.ParticleDisruptor,     ItemType.Painkillers,   ItemType.Adrenaline},                       //16    
    //    //        new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //17    
    //    //        new List<ItemType>{                                                         ItemType.Painkillers,   ItemType.Adrenaline},                       //18    
    //    //        new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //19     
    //    //        new List<ItemType>{ItemType.SCP018,         ItemType.Jailbird,              ItemType.Painkillers,   ItemType.Adrenaline},                       //20
    //    //        new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //21    
    //    //        new List<ItemType>{                                                         ItemType.SCP500,        ItemType.Adrenaline},                       //22    
    //    //        new List<ItemType>{                                                         ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330},   //23    
    //    //        new List<ItemType>{                         ItemType.ParticleDisruptor,     ItemType.SCP500,        ItemType.Adrenaline},                       //24    
    //    //        new List<ItemType>{ItemType.SCP244b,                                        ItemType.SCP500,        ItemType.SCP330,        ItemType.SCP330 }   //25    
    //    //    };

    //    //static List<List<ItemType>> rage_kill_streak_loadout = new List<List<ItemType>>
    //    //    {
    //    //        new List<ItemType>{                         ItemType.GunCOM15},                                                                                 //0
    //    //        new List<ItemType>{                         ItemType.GunCOM15},                                                                                 //1
    //    //        new List<ItemType>{                         ItemType.GunCOM15},                                                                                 //2
    //    //        new List<ItemType>{                         ItemType.GunCOM18},                                                                                 //3
    //    //        new List<ItemType>{                         ItemType.GunCOM18},                                                                                 //4
    //    //        new List<ItemType>{ItemType.ArmorLight,     ItemType.GunCOM18},                                                                                 //5
    //    //        new List<ItemType>{ItemType.ArmorLight,     ItemType.GunRevolver},                                                                              //6
    //    //        new List<ItemType>{ItemType.ArmorLight,     ItemType.GunRevolver},                                                                              //7
    //    //        new List<ItemType>{ItemType.ArmorLight,     ItemType.GunRevolver},                                                                              //8
    //    //        new List<ItemType>{ItemType.ArmorLight,     ItemType.GunFSP9},                                                                                  //9
    //    //        new List<ItemType>{ItemType.ArmorCombat,    ItemType.GunFSP9},                                                                                  //10
    //    //        new List<ItemType>{ItemType.ArmorCombat,    ItemType.GunFSP9},                                                                                  //11
    //    //        new List<ItemType>{ItemType.ArmorCombat,    ItemType.GunCrossvec},                                                                              //12
    //    //        new List<ItemType>{ItemType.ArmorCombat,    ItemType.GunCrossvec},                                                                              //13
    //    //        new List<ItemType>{ItemType.ArmorCombat,    ItemType.GunCrossvec},                                                                              //14
    //    //        new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunE11SR},                                                                                 //15
    //    //        new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunE11SR},                                                                                 //16
    //    //        new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunE11SR},                                                                                 //17
    //    //        new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunAK},                                                                                    //18
    //    //        new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunAK},                                                                                    //19
    //    //        new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunAK},                                                                                    //20
    //    //        new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunShotgun},                                                                               //21
    //    //        new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunShotgun},                                                                               //22
    //    //        new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunShotgun},                                                                               //23
    //    //        new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunLogicer,        ItemType.GunShotgun},                                                   //24
    //    //        new List<ItemType>{ItemType.ArmorHeavy,     ItemType.GunLogicer,        ItemType.GunShotgun }                                                   //25
    //    //    };

    //    //static List<List<Effect>> rage_kill_streak_status_effects = new List<List<Effect>>
    //    //    {
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Disabled", 1),          new Effect("Bleeding", 1)},                                                             //0
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Disabled", 1),          new Effect("Bleeding", 1)},                                                             //1
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Disabled", 1),          new Effect("Bleeding", 1)},                                                             //2
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Disabled", 1)},                                                                                                 //3
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Disabled", 1)},                                                                                                 //4
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Disabled", 1),          new Effect("Scp1853", 1)},                                                              //5
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("Scp1853", 1)},                                                                                                  //6
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("MovementBoost", 4),     new Effect("Scp1853", 1)},                                                              //7
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("Exhausted", 1),         new Effect("MovementBoost", 8),     new Effect("Scp1853", 1),                           new Effect("BodyshotReduction", 1)},//8
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("MovementBoost", 12),     new Effect("Scp1853", 1),           new Effect("BodyshotReduction", 1)},                                                    //9
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("MovementBoost", 16),    new Effect("Scp1853", 2),           new Effect("BodyshotReduction", 1)},                                                    //10
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("Burned", 1),            new Effect("MovementBoost", 20),    new Effect("Scp1853", 2),           new Effect("BodyshotReduction", 1)},                                                    //11
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("MovementBoost", 24),    new Effect("Scp1853", 2),           new Effect("BodyshotReduction", 2)},                                                                                        //12
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("MovementBoost", 28),    new Effect("Scp1853", 2),           new Effect("BodyshotReduction", 2)},                                                                                        //13
    //    //        new List<Effect>{new Effect("Hemorrhage", 1),       new Effect("MovementBoost", 32),    new Effect("Scp1853", 2),           new Effect("BodyshotReduction", 2)},                                                                                        //14
    //    //        new List<Effect>{new Effect("MovementBoost", 36),   new Effect("Scp1853", 3),           new Effect("BodyshotReduction", 2), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 1.0f)))) },                                            //15
    //    //        new List<Effect>{new Effect("MovementBoost", 40),   new Effect("Scp1853", 3),           new Effect("BodyshotReduction", 3), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 2.0f)))) },                                            //16
    //    //        new List<Effect>{new Effect("MovementBoost", 44),   new Effect("Scp1853", 3),           new Effect("BodyshotReduction", 3), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 3.0f)))) },                                            //17
    //    //        new List<Effect>{new Effect("MovementBoost", 48),   new Effect("Scp1853", 3),           new Effect("BodyshotReduction", 3), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 4.0f)))) },                                            //18
    //    //        new List<Effect>{new Effect("MovementBoost", 52),   new Effect("Scp1853", 3),           new Effect("BodyshotReduction", 3), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 5.0f)))) },                                            //19
    //    //        new List<Effect>{new Effect("MovementBoost", 56),   new Effect("Scp1853", 4),           new Effect("BodyshotReduction", 4), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 6.0f)))) },                                            //20
    //    //        new List<Effect>{new Effect("MovementBoost", 60),   new Effect("Scp1853", 4),           new Effect("BodyshotReduction", 4), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 7.0f)))) },                                            //21
    //    //        new List<Effect>{new Effect("MovementBoost", 64),   new Effect("Scp1853", 4),           new Effect("BodyshotReduction", 4), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 8.0f)))) },                                            //22
    //    //        new List<Effect>{new Effect("MovementBoost", 68),   new Effect("Scp1853", 4),           new Effect("BodyshotReduction", 4), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 9.0f)))) },                                            //23
    //    //        new List<Effect>{new Effect("MovementBoost", 72),   new Effect("Scp1853", 4),           new Effect("BodyshotReduction", 4), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 10.0f)))) },                                           //24
    //    //        new List<Effect>{new Effect("MovementBoost", 76),   new Effect("Scp1853", 255),         new Effect("BodyshotReduction", 4), new Effect("DamageReduction", (byte)(200.0f * (1.0f - math.pow(0.9f, 11.0f)))) }                                            //25
    //    //    };


    //    public static RDMPlayer GetRDMPlayer(Player player)
    //    {
    //        return players[player.PlayerId];
    //    }

    //    //private void SetupMenus()
    //    //{
    //    //    InventoryMenu.CreateMenu((int)MenuPage.None, "", new List<MenuItem>
    //    //    {
    //    //        new MenuItem(ItemType.KeycardO5, "", (player)=>
    //    //        {
    //    //            RDMPlayer rdm = players[player.PlayerId];
    //    //            if(!rdm.loadout_locked)
    //    //            {
    //    //                rdm.customising_loadout = true;
    //    //                Timing.KillCoroutines(rdm.teleport_handle);
    //    //                InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
    //    //            }
    //    //            else
    //    //            {
    //    //                BroadcastOverride.ClearLines(player, BroadcastPriority.High);
    //    //                BroadcastOverride.BroadcastLines(player, 1, 3, BroadcastPriority.High, loadout_customisation_denied);
    //    //            }
    //    //            return false;
    //    //        })
    //    //    });

    //    //    InventoryMenu.CreateMenu((int)MenuPage.Main, "<b><color=#43BFF0>[MAIN MENU]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
    //    //    {
    //    //        new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b><color=#5900ff>Save and Exit</color></b>", (player)=>
    //    //        {
    //    //            RDMPlayer rdm = players[player.PlayerId];
    //    //            rdm.customising_loadout = false;
    //    //            InventoryMenu.SetMenu(player, (int)MenuPage.None);
    //    //            ClearInventory(player);
    //    //            SetPlayerRDMInv(player);
    //    //            BroadcastOverride.ClearLines(player, BroadcastPriority.High);
    //    //            Killfeed.SetBroadcastKillfeedLayout(player);
    //    //            if (rdm.IsEmptyLoadout)
    //    //            {
    //    //                BroadcastOverride.BroadcastLine(player, 1, 300, BroadcastPriority.High, loadout_customisation_hint);
    //    //                if (rdm.in_spawn)
    //    //                    BroadcastOverride.BroadcastLine(player, 2, 300, BroadcastPriority.High, teleport_msg);
    //    //            }
    //    //            else
    //    //            {
    //    //                if (rdm.in_spawn)
    //    //                {
    //    //                    BroadcastOverride.BroadcastLine(player, 1, 3, BroadcastPriority.VeryLow, "<color=#43BFF0>loadout set, teleporting in 3 seconds</color>");
    //    //                    rdm.teleport_handle = Timing.CallDelayed(3.0f, () =>
    //    //                    {
    //    //                        if (Player.GetPlayers().Count == 1)
    //    //                        {
    //    //                            BroadcastOverride.BroadcastLines(player, 1, 1500.0f, BroadcastPriority.Low, waiting_for_players_msg);
    //    //                            BroadcastOverride.UpdateIfDirty(player);
    //    //                        }
    //    //                        else if (Player.GetPlayers().Count >= 2 && !game_started)
    //    //                        {
    //    //                            game_started = true;
    //    //                            BroadcastOverride.ClearLines(BroadcastPriority.Low);
    //    //                            BroadcastOverride.UpdateAllDirty();
    //    //                        }
    //    //                        TeleportRandom(player);
    //    //                    });
    //    //                }
    //    //            }
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardScientist, "<color=#e7d77b>[SCIENTIST]</color> = <b><color=#FF0000>Customise Loadout - </color><color=#43BFF0>[GUN SLOT]</color></b>", (player)=>
    //    //        {
    //    //            RDMPlayer rdm = players[player.PlayerId];
    //    //            if (rdm.killstreak_mode != KillstreakMode.Rage)
    //    //            {
    //    //                if (rdm.killstreak_mode == KillstreakMode.Expert || rdm.killstreak_mode == KillstreakMode.Standard)
    //    //                    InventoryMenu.ShowMenu(player, (int)MenuPage.GunSlot2);
    //    //                else
    //    //                    InventoryMenu.ShowMenu(player, (int)MenuPage.GunSlot3);
    //    //            }
    //    //            else
    //    //            {
    //    //                BroadcastOverride.BroadcastLine(player, 3, 1, BroadcastPriority.High, "<b>[DATA EXPUNGED]</b>");
    //    //                RemoveItem(player, ItemType.KeycardScientist);
    //    //            }
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardResearchCoordinator, "<color=#e1ab21>[RESEARCH SUPERVISOR]</color> = <b><color=#43BFF0>[KILLSTREAK REWARD SYSTEM]</color></b>", (player)=>
    //    //        {
    //    //            RDMPlayer rdm = players[player.PlayerId];
    //    //            MenuInfo info;
    //    //            if (rdm.rage_mode_enabled)
    //    //            {
    //    //                InventoryMenu.ShowMenu(player, (int)MenuPage.KillstreakModeSecret);
    //    //                info = InventoryMenu.GetInfo((int)MenuPage.KillstreakModeSecret);
    //    //            }
    //    //            else
    //    //            {
    //    //                InventoryMenu.ShowMenu(player, (int)MenuPage.KillstreakMode);
    //    //                info = InventoryMenu.GetInfo((int)MenuPage.KillstreakMode);
    //    //            }

    //    //            BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, "Current killstreak reward system selected: " + KillstreakColorCode(player) + rdm.killstreak_mode.ToString() + "</color>");
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardContainmentEngineer, "<color=#bd8f86>[CONTAINMENT ENGINEER]</color> = <b><color=#43BFF0>[PREFERENCES]</color></b>", (player)=>
    //    //        {
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.Preference);
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.Coin, "<color=#bd8f86>[COIN]</color> = <b><color=#43BFF0>[DEBUG MENU] dont forget to remove!!!</color></b>", (player)=>
    //    //        {
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.Debug);
    //    //            return false;
    //    //        }),
    //    //    });

    //    //    InventoryMenu.CreateMenu((int)MenuPage.GunSlot2, "<b><color=#43BFF0>[GUN SLOT]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
    //    //    {
    //    //        new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
    //    //        {
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardFacilityManager, "<color=#bd1a4a>[FACILITY MANAGER]</color> = <b><color=#FF0000>Primary - </color><color=#43BFF0>[GUN CLASS]</color></b>", (player)=>
    //    //        {
    //    //            players[player.PlayerId].slot = GunSlot.Primary;
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardZoneManager, "<color=#217b7b>[ZONE MANAGER]</color> = <b>Secondary - <color=#43BFF0>[GUN CLASS]</color></b>", (player)=>
    //    //        {
    //    //            players[player.PlayerId].slot = GunSlot.Secondary;
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
    //    //            return false;
    //    //        }),
    //    //    });

    //    //    InventoryMenu.CreateMenu((int)MenuPage.GunSlot3, "<b><color=#43BFF0>[GUN SLOT]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
    //    //    {
    //    //        new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
    //    //        {
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardNTFCommander, "<color=#1b43cb>[CAPTAIN]</color> = <b><color=#FF0000>Primary - </color><color=#43BFF0>[GUN CLASS]</color></b>", (player)=>
    //    //        {
    //    //            players[player.PlayerId].slot = GunSlot.Primary;
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardNTFLieutenant, "<color=#177dde>[SERGEANT]</color> = <b>Secondary - <color=#43BFF0>[GUN CLASS]</color></b>", (player)=>
    //    //        {
    //    //            players[player.PlayerId].slot = GunSlot.Secondary;
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardNTFOfficer, "<color=#accfe1>[PRIVATE]</color> = <b>Tertiary - <color=#43BFF0>[GUN CLASS]</color></b>", (player)=>
    //    //        {
    //    //            players[player.PlayerId].slot = GunSlot.Tertiary;
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.GunClass);
    //    //            return false;
    //    //        }),
    //    //    });

    //    //    InventoryMenu.CreateMenu((int)MenuPage.GunClass, "<b><color=#43BFF0>[GUN CLASS]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
    //    //    {
    //    //        new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
    //    //        {
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardNTFCommander, "<color=#1b43cb>[CAPTAIN]</color> = <b><color=#1b43cb>[MTF GUNS]</color></b>", (player)=>
    //    //        {
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.MtfGun);
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardChaosInsurgency, "<color=#008f1c>[CHAOS]</color> = <b><color=#008f1c>[CHAOS GUNS]</color></b>", (player)=>
    //    //        {
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.ChaosGun);
    //    //            return false;
    //    //        })
    //    //    });

    //    //    Func<Player, ItemType,bool> GunSelected = (player, gun) =>
    //    //    {
    //    //        RDMPlayer rdm = players[player.PlayerId];
    //    //        rdm.SetGunInSlot(gun);
    //    //        InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
    //    //        MenuInfo info = InventoryMenu.GetInfo((int)MenuPage.Main);
    //    //        string gun_name = Enum.GetName(typeof(ItemType), gun).Substring(3);
    //    //        BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, "<color=#43BFF0>" + gun_name + "</color> added to your loadout as the <color=#FF0000>" + rdm.slot.ToString() + "</color> weapon");
    //    //        return false;
    //    //    };

    //    //    InventoryMenu.CreateMenu((int)MenuPage.MtfGun, "<b><color=#1b43cb>[MTF GUNS]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
    //    //    {
    //    //        new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
    //    //        {
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.GunCOM15, "", (player)=> { return GunSelected(player, ItemType.GunCOM15); }),
    //    //        new MenuItem(ItemType.GunCOM18, "", (player)=> { return GunSelected(player, ItemType.GunCOM18);  }),
    //    //        new MenuItem(ItemType.GunFSP9, "", (player)=> { return GunSelected(player, ItemType.GunFSP9);  }),
    //    //        new MenuItem(ItemType.GunCrossvec, "", (player)=> { return GunSelected(player, ItemType.GunCrossvec); }),
    //    //        new MenuItem(ItemType.GunE11SR, "", (player)=> { return GunSelected(player, ItemType.GunE11SR);  })
    //    //    });

    //    //    InventoryMenu.CreateMenu((int)MenuPage.ChaosGun, "<b><color=#008f1c>[CHAOS GUNS]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
    //    //    {
    //    //        new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
    //    //        {
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.GunAK, "", (player)=> { GunSelected(player, ItemType.GunAK); return false; }),
    //    //        new MenuItem(ItemType.GunLogicer, "", (player)=> { GunSelected(player, ItemType.GunLogicer); return false; }),
    //    //        new MenuItem(ItemType.GunShotgun, "", (player)=> { GunSelected(player, ItemType.GunShotgun); return false; }),
    //    //        new MenuItem(ItemType.GunRevolver, "", (player)=> { GunSelected(player, ItemType.GunRevolver); return false; })
    //    //    });

    //    //    Func<Player, KillstreakMode, bool> KillstreakModeSelected = (player, mode) =>
    //    //    {
    //    //        RDMPlayer rdm = players[player.PlayerId];
    //    //        rdm.killstreak_mode = mode;
    //    //        InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
    //    //        MenuInfo info = InventoryMenu.GetInfo((int)MenuPage.Main);
    //    //        BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, KillstreakColorCode(player) + Enum.GetName(typeof(KillstreakMode), rdm.killstreak_mode) + "</color> selected as your killstreak reward system");
    //    //        return false;
    //    //    };

    //    //    List<MenuItem> killstreak_items = new List<MenuItem>()
    //    //    {
    //    //        new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
    //    //        {
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.ArmorHeavy, "<color=#eb0d47>[HEAVY ARMOR]</color> = <b><color=#5900ff>Easy</color>: low risk low reward, good loadout but bad killstreak rewards</b>", (player)=>
    //    //        {
    //    //            return KillstreakModeSelected(player, KillstreakMode.Easy);
    //    //        }),
    //    //        new MenuItem(ItemType.ArmorCombat, "<color=#eb0d47>[COMBAT ARMOR]</color> = <b><color=#43BFF0>Standard</color>: medium risk medium reward, ok loadout and ok killsteak rewards</b>", (player)=>
    //    //        {
    //    //            return KillstreakModeSelected(player, KillstreakMode.Standard);
    //    //        }),                
    //    //        new MenuItem(ItemType.ArmorLight, "<color=#eb0d47>[LIGHT ARMOR]</color> = <b><color=#36a832>Expert</color>: high risk high reward, bad loadout but good killstreak rewards </b>", (player)=>
    //    //        {
    //    //            return KillstreakModeSelected(player, KillstreakMode.Expert);
    //    //        })
    //    //    };

    //    //    List<MenuItem> killstreak_items_secret = killstreak_items.ToList();
    //    //    killstreak_items_secret.Add(
    //    //    new MenuItem(ItemType.GunCom45, "<color=#eb0d47>[COM 45]</color> = <b><color=#FF0000>RAGE</color> - [DATA EXPUNGED]</b>", (player) =>
    //    //    {
    //    //        return KillstreakModeSelected(player, KillstreakMode.Rage);
    //    //    }));

    //    //    InventoryMenu.CreateMenu((int)MenuPage.KillstreakMode, "<b><color=#43BFF0>[KILLSTREAK REWARD SYSTEM]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", killstreak_items);
    //    //    InventoryMenu.CreateMenu((int)MenuPage.KillstreakModeSecret, "<b><color=#43BFF0>[KILLSTREAK REWARD SYSTEM]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", killstreak_items_secret);

    //    //    InventoryMenu.CreateMenu((int)MenuPage.Preference, "<b><color=#43BFF0>[PREFERENCES]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
    //    //    {
    //    //        new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
    //    //        {
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardGuard, "<color=#eb0d47>[GUARD]</color> = <b>Toggle Loadout Radio</b>", (player)=>
    //    //        {
    //    //            RDMPlayer rdm = players[player.PlayerId];
    //    //            rdm.loadout_radio = !rdm.loadout_radio;
    //    //            MenuInfo info = InventoryMenu.GetInfo((int)MenuPage.Preference);
    //    //            BroadcastOverride.BroadcastLine(player, info.broadcast_lines + 1, 1500.0f, BroadcastPriority.High, "<b><color=#43BFF0>Loadout Radio: </color></b>" + (rdm.loadout_radio ? "Enabled" : "Disabled"));
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardScientist, "<color=#eb0d47>[SCIENTIST]</color> = <b><color=#43BFF0>[STATS]</color></b>", (player)=>
    //    //        {
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.Stats);
    //    //            DisplayStats(InventoryMenu.GetInfo((int)MenuPage.Stats).broadcast_lines + 1, player);
    //    //            return false;
    //    //        }),                
    //    //        new MenuItem(ItemType.Flashlight, "<color=#eb0d47>[FLASH LIGHT]</color> = <b>Enable spectator mode</b>", (player)=>
    //    //        {
    //    //            RDMPlayer rdm = players[player.PlayerId];
    //    //            rdm.is_spectating = true;
    //    //            BroadcastOverride.ClearLines(player, BroadcastPriority.High);
    //    //            Killfeed.SetBroadcastKillfeedLayout(player);
    //    //            BroadcastOverride.BroadcastLine(player, 1, 10, BroadcastPriority.High, "spectator mode is currently bugged, you may need to leave and rejoin to respawn");
    //    //            player.SetRole(RoleTypeId.Spectator);
    //    //            return false;
    //    //        }),                
    //    //        new MenuItem(ItemType.Coin, "<color=#eb0d47>[COIN]</color> = <b>Enable [DATA EXPUNGED]</b>", (player)=>
    //    //        {
    //    //            players[player.PlayerId].rage_mode_enabled = true;
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
    //    //            return false;
    //    //        })
    //    //    });

    //    //    InventoryMenu.CreateMenu((int)MenuPage.Stats, "<b><color=#43BFF0>[STATS]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
    //    //    {
    //    //        new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[PREFERENCES]</color></b>", (player)=>
    //    //        {
    //    //            BroadcastOverride.ClearLines(player, BroadcastPriority.Highest);
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.Preference);
    //    //            return false;
    //    //        })
    //    //    });

    //    //    InventoryMenu.CreateMenu((int)MenuPage.Debug, "<b><color=#43BFF0>[DEBUG]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>", new List<MenuItem>
    //    //    {
    //    //        new MenuItem(ItemType.KeycardO5, "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>", (player)=>
    //    //        {
    //    //            InventoryMenu.ShowMenu(player, (int)MenuPage.Main);
    //    //            return false;
    //    //        }),                
    //    //        new MenuItem(ItemType.KeycardGuard, "close all rooms", (player)=>
    //    //        {
    //    //            BroadcastOverride.BroadcastLine(player, 6, 3.0f, BroadcastPriority.High, "closed all rooms");
    //    //            RoomScale.CloseAllRooms();
    //    //            return false;
    //    //        }),               
    //    //        new MenuItem(ItemType.KeycardNTFOfficer, "open all rooms", (player)=>
    //    //        {
    //    //            BroadcastOverride.BroadcastLine(player, 6, 3.0f, BroadcastPriority.High, "opened all rooms");
    //    //            RoomScale.OpenAllRooms();
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardNTFLieutenant, "open 2 rooms", (player)=>
    //    //        {
    //    //            BroadcastOverride.BroadcastLine(player, 6, 3.0f, BroadcastPriority.High, "opened 2 rooms");
    //    //            RoomScale.OpenRooms(2);
    //    //            return false;
    //    //        }),
    //    //        new MenuItem(ItemType.KeycardNTFCommander, "close 2 rooms", (player)=>
    //    //        {
    //    //            BroadcastOverride.BroadcastLine(player, 6, 3.0f, BroadcastPriority.High, "closing 2 rooms");
    //    //            RoomScale.CloseRooms(2);
    //    //            return false;
    //    //        })
    //    //    });
    //    //}

    //    public Deathmatch()
    //    {
    //        Singleton = this;

    //        players = new Dictionary<int, RDMPlayer>();
    //        Killfeed.Init(2, 5, 20);

    //        DeathmatchMenu.SetupMenus();
    //        //SetupMenus();
    //    }

    //    [PluginEntryPoint("Deathmatch", "1.0", "needs no explanation", "The Riptide")]
    //    void EntryPoint()
    //    {
    //        EventManager.RegisterEvents(this);
    //        EventManager.RegisterEvents<InventoryMenu>(this);
    //        EventManager.RegisterEvents<Killfeed>(this);
    //        EventManager.RegisterEvents<BroadcastOverride>(this);
    //        EventManager.RegisterEvents<DeathmatchStats>(this);
    //        EventManager.RegisterEvents<KillstreakSystem>(this);
    //        EventManager.RegisterEvents<DeathmatchLoadout>(this);
    //    }

    //    [PluginEvent(ServerEventType.RoundStart)]
    //    void OnRoundStart()
    //    {

    //        RoomScale.BuildRoomGraph();
    //        //RoomScale.PrintRoomAndDoorInfo();
    //        RoomScale.Enable();

    //        Server.Instance.SetRole(RoleTypeId.Scp939);
    //        Server.Instance.ReferenceHub.nicknameSync.SetNick("[THE RIPTIDE]");
    //        Server.Instance.Position = new Vector3(128.8f, 994.0f, 18.0f);

    //        Timing.CallDelayed(3.0f, () =>
    //        {
    //            try
    //            {
    //                Server.Instance.ReferenceHub.serverRoles.RaEverywhere = true;
    //                Server.Instance.ReferenceHub.serverRoles.Permissions = (ulong)PlayerPermissions.FacilityManagement;
    //                CommandSystem.Commands.RemoteAdmin.Cleanup.ItemsCommand cmd = new CommandSystem.Commands.RemoteAdmin.Cleanup.ItemsCommand();
    //                string response = "";
    //                string[] empty = { "" };
    //                cmd.Execute(new ArraySegment<string>(empty, 0, 0), new RemoteAdmin.PlayerCommandSender(Server.Instance.ReferenceHub), out response);
    //                ServerConsole.AddLog(response);
    //            }
    //            catch(Exception ex)
    //            {
    //                ServerConsole.AddLog(ex.ToString());
    //            }
    //        });

    //        Server.FriendlyFire = true;

    //        List<ElevatorDoor> gate_elevators = ElevatorDoor.AllElevatorDoors[ElevatorManager.ElevatorGroup.GateA];
    //        gate_elevators.AddRange(ElevatorDoor.AllElevatorDoors[ElevatorManager.ElevatorGroup.GateB]);
    //        foreach (ElevatorDoor door in gate_elevators)
    //            door.ServerChangeLock(DoorLockReason.AdminCommand, true);

    //        HashSet<DoorVariant> dspawn_doors = DoorVariant.DoorsByRoom[Facility.Rooms.Find((room) => room.Identifier.Name == RoomName.LczClassDSpawn).Identifier];
    //        foreach (DoorVariant door in dspawn_doors)
    //        {
    //            if (door.Rooms.Count() == 1)
    //                door.ServerChangeLock(DoorLockReason.AdminCommand, true);
    //        }

    //        Timing.CallDelayed(60.0f * 25.0f, () => { BroadcastOverride.BroadcastLine(1, 30, BroadcastPriority.Medium, "<color=#43BFF0>Round Restart in 5 minutes</color>"); });
    //        Timing.CallDelayed(60.0f * 29.0f, () => { BroadcastOverride.BroadcastLine(1, 30, BroadcastPriority.Medium, "<color=#43BFF0>Round Restart in 1 minute</color>"); });
    //        Timing.CallDelayed(60.0f * 30.0f, () => { DeathmatchStats.DisplayRoundStats(); Server.Instance.Kill(); Server.FriendlyFire = false; if (Player.Count <= 1) { Timing.CallDelayed(15.0f, () => Round.Restart(false)); } });
    //    }

    //    [PluginEvent(ServerEventType.MapGenerated)]
    //    void OnMapGenerated()
    //    {
            
    //    }

    //    [PluginEvent(ServerEventType.PlayerJoined)]
    //    void OnPlayerJoined(Player player)
    //    {

    //        //MEC.Timing.CallDelayed(1.0f, () =>
    //        //{
    //        //    //player.ReferenceHub.serverRoles._bgt = "test";
    //        //    player.ReferenceHub.serverRoles.Group.BadgeText = "test";
    //        //    player.ReferenceHub.characterClassManager.UserCode_CmdRequestShowTag(false);
    //        //});
    //        //player.PlayerInfo.IsBadgeHidden = false;

    //        //PermissionsHandler permissions = ServerStatic.GetPermissionsHandler();

    //        //permissions.



    //        if (!players.ContainsKey(player.PlayerId))
    //            players.Add(player.PlayerId, new RDMPlayer());

    //        //BroadcastOverride.RegisterPlayer(player);
    //        //Killfeed.RegisterPlayer(player);
    //        //Killfeed.SetBroadcastKillfeedLayout(player);
    //        //BroadcastOverride.SetEvenLineSizes(player, 4);


    //        if (!Round.IsRoundStarted)
    //        {
    //            MEC.Timing.CallDelayed(1.0f, () => { if (!Round.IsRoundStarted) { Round.Start(); } });
    //        }
    //        else
    //        {
    //            MEC.Timing.CallDelayed(1.0f, () => RespawnPlayer(player));
    //        }

    //        if(Player.GetPlayers().Count == 1)
    //        {
    //            RoomScale.OpenAllRooms();
    //            game_started = false;
    //        }
    //        else if(Player.GetPlayers().Count == 2)
    //        {
    //            RoomScale.CloseAllRooms();
    //            foreach (Player p in Player.GetPlayers())
    //            {
    //                if (p != player && p.IsAlive && p.Room != null && players.ContainsKey(p.PlayerId) && !players[p.PlayerId].in_spawn)
    //                {
    //                    BroadcastOverride.BroadcastLine(p, 1, 300.0f, BroadcastPriority.Low, "Player <color=#43BFF0>" + player.Nickname + "</color> joined, waiting for them to select a loadout");
    //                    BroadcastOverride.UpdateIfDirty(p);
    //                    Timing.CallDelayed(10.0f, () =>
    //                    {
    //                        if(p != null && player != null && IsPlayerValid(p) && !players[p.PlayerId].in_spawn && DeathmatchLoadout.GetLoadout(p).radio /*players[p.PlayerId].loadout_radio*/ && IsPlayerValid(player) && !game_started)
    //                            BroadcastOverride.BroadcastLine(p, 2, 290.0f, BroadcastPriority.Low, "<color=#43BFF0>" + player.Nickname + "</color> is struggling to set his loadout please help him with your radio");
    //                        BroadcastOverride.UpdateIfDirty(p);
    //                    });
    //                    RoomScale.AddRoom(RoomIdUtils.RoomAtPosition(p.Position));
    //                }
    //            }
    //            RoomScale.OpenRooms(6);
    //        }
    //        else if (Player.GetPlayers().Count > 2)
    //        {
    //            RoomScale.SetRooms(Player.GetPlayers().Count * 3);
    //        }
    //    }

    //    [PluginEvent(ServerEventType.PlayerLeft)]
    //    void OnPlayerLeft(Player player)
    //    {
    //        if (players.ContainsKey(player.PlayerId))
    //            players.Remove(player.PlayerId);

    //        //BroadcastOverride.UnregisterPlayer(player);
    //        //Killfeed.UnregisterPlayer(player);

    //        if (Player.GetPlayers().Count == 2)
    //        {
    //            RoomScale.OpenAllRooms();
    //            game_started = false;
    //        }
    //        else if (Player.GetPlayers().Count > 2)
    //        {
    //            RoomScale.SetRooms(Player.GetPlayers().Count * 3);
    //        }
    //        ServerConsole.AddLog("player: " + player.Nickname + " left. player count: " + Player.GetPlayers().Count);
    //    }

    //    [PluginEvent(ServerEventType.PlayerDying)]
    //    void OnPlayerDying(Player target, Player killer, DamageHandlerBase damage)
    //    {
    //        ClearInventory(target);
    //    }

    //    //struct EffectCompare : IEqualityComparer<Effect>
    //    //{
    //    //    public bool Equals(Effect x, Effect y)
    //    //    {
    //    //        return x.name == y.name;
    //    //    }

    //    //    public int GetHashCode(Effect obj)
    //    //    {
    //    //        return obj.name.GetHashCode();
    //    //    }
    //    //}

    //    [PluginEvent(ServerEventType.PlayerDeath)]
    //    void OnPlayerDeath(Player target, Player killer, DamageHandlerBase damage)
    //    {
    //        RoomScale.CloseRooms(1);
    //        RoomScale.OpenRooms(1);

    //        if (killer != null && killer.IsAlive && players.ContainsKey(killer.PlayerId))
    //        {
    //            RDMPlayer rdm_killer = players[killer.PlayerId];
    //            //RDMStats stats = rdm_killer.stats;
    //            //stats.kills++;
    //            //if (damage is StandardDamageHandler standard)
    //            //    if (standard.Hitbox == HitboxType.Headshot)
    //            //        stats.headshot_kills++;

    //            if (killer.CurrentItem.Category == ItemCategory.Firearm)
    //            {
    //                ItemType ammo = (killer.CurrentItem as Firearm).AmmoType;
    //                killer.SetAmmo(ammo, (ushort)math.min(killer.GetAmmo(ammo) + (ushort)(killer.GetAmmoLimit(ammo) / 5), killer.GetAmmoLimit(ammo)));
    //            }

    //            //if (rdm_killer.killstreak_mode == KillstreakMode.Rage)
    //            //{
    //            //    List<ItemType> previous_loadout = rage_kill_streak_loadout[math.min(25, stats.killstreak)];
    //            //    List<ItemType> new_loadout = rage_kill_streak_loadout[math.min(25, stats.killstreak + 1)];
    //            //    IEnumerable<ItemType> remove_items = previous_loadout.Except(new_loadout);
    //            //    IEnumerable<ItemType> add_items = new_loadout.Except(previous_loadout);

    //            //    foreach (ItemType item in remove_items)
    //            //    {
    //            //        IEnumerable<ItemBase> matches = killer.Items.Where((i) => i.ItemTypeId == item);
    //            //        killer.RemoveItem(new Item(matches.First()));
    //            //    }
    //            //    foreach (ItemType item in add_items)
    //            //    {
    //            //        if (IsGun(item))
    //            //        {
    //            //            if (killer.IsInventoryFull)
    //            //                if (!RemoveItem(killer, ItemType.Painkillers))
    //            //                    if (!RemoveItem(killer, ItemType.Medkit))
    //            //                        if (!RemoveItem(killer, ItemType.Adrenaline))
    //            //                            if (!RemoveItem(killer, ItemType.SCP500))
    //            //                                if (!RemoveItem(killer, ItemType.SCP244a))
    //            //                                    if (!RemoveItem(killer, ItemType.SCP244b))
    //            //                                        if (!RemoveItem(killer, ItemType.SCP018))
    //            //                                            RemoveItem(killer, ItemType.GunShotgun);
    //            //            AddFirearm(killer, item, true);
    //            //        }
    //            //        else
    //            //            killer.AddItem(item);
    //            //    }

    //            //    List<Effect> previous_effects = rage_kill_streak_status_effects[math.min(25, stats.killstreak)];
    //            //    List<Effect> new_effects = rage_kill_streak_status_effects[math.min(25, stats.killstreak + 1)];
    //            //    IEnumerable<Effect> remove_effects = previous_effects.Except(new_effects, new EffectCompare());
    //            //    IEnumerable<Effect> add_effects = new_effects.Except(previous_effects);

    //            //    foreach (Effect effect in remove_effects)
    //            //    {
    //            //        killer.EffectsManager.ChangeState(effect.name, 0);
    //            //    }

    //            //    foreach (Effect effect in add_effects)
    //            //    {
    //            //        killer.EffectsManager.ChangeState(effect.name, effect.intensity);
    //            //    }
    //            //}

    //            //List<ItemType> reward_items = new List<ItemType>();
    //            //switch (rdm_killer.killstreak_mode)
    //            //{
    //            //    case KillstreakMode.Easy:
    //            //        reward_items = easy_kill_streak_table[stats.killstreak % 25];
    //            //        break;
    //            //    case KillstreakMode.Standard:
    //            //        reward_items = standard_kill_streak_table[stats.killstreak % 25];
    //            //        break;
    //            //    case KillstreakMode.Expert:
    //            //        reward_items = expert_kill_streak_table[stats.killstreak % 25];
    //            //        break;
    //            //    case KillstreakMode.Rage:
    //            //        reward_items = rage_kill_streak_table[stats.killstreak % 25];
    //            //        break;
    //            //    default:
    //            //        reward_items = standard_kill_streak_table[stats.killstreak % 25];
    //            //        break;
    //            //}
    //            //AddItemsInorder(killer, reward_items);

    //            //stats.killstreak++;
    //            //if (stats.killstreak > stats.highest_killstreak)
    //            //    stats.highest_killstreak = stats.killstreak;
    //            //if (stats.killstreak % 5 == 0)
    //            //    BroadcastOverride.BroadcastLine(1, stats.killstreak, BroadcastPriority.Medium, "<b><color=#43BFF0>" + killer.Nickname + "</color></b> is on a <b><color=#FF0000>" + stats.killstreak.ToString() + "</color></b> kill streak");
    //            //else
    //            //    BroadcastOverride.BroadcastLine(killer, 2, 3, BroadcastPriority.Low, "Kill streak <b><color=#FF0000>" + stats.killstreak.ToString() + "</color></b>");

    //        }

    //        if (players.ContainsKey(target.PlayerId))
    //        {
    //            Killfeed.PushKill(target, killer, damage);
    //            //RDMPlayer rdm_player = players[target.PlayerId];
    //            //RDMStats stats = rdm_player.stats;

    //            //if (stats.killstreak >= 5)
    //            //    BroadcastOverride.BroadcastLine(2, stats.killstreak, BroadcastPriority.Medium, "<b><color=#43BFF0>" + killer.Nickname + "</color></b> ended <b><color=#43BFF0>" + target.Nickname + "'s </color></b>" + "<b><color=#FF0000>" + stats.killstreak.ToString() + "</color></b> kill streak");

    //            //stats.time_alive += (int)Math.Round(Time.time - rdm_player.start_time);
    //            //stats.killstreak = 0;
    //            //stats.deaths++;
    //            //BroadcastOverride.BroadcastLine(target, 1, 300, BroadcastPriority.Low, "<b><color=#FFFF00>Left/Right click to respawn</color></b>");
    //            //BroadcastOverride.BroadcastLine(target, 2, 300, BroadcastPriority.Low, "<b><color=#FF0000>Tab to edit attachments/presets</color></b>");
    //        }
    //        Killfeed.UpdateAllDirty();
    //        BroadcastOverride.UpdateAllDirty();
    //    }

    //    [PluginEvent(ServerEventType.PlayerChangeSpectator)]
    //    void OnPlayerChangeSpectator(Player player, Player old_target, Player new_target)
    //    {
    //        if (!players.ContainsKey(player.PlayerId))
    //            return;

    //        if (!players[player.PlayerId].is_spectating)
    //        {
    //            BroadcastOverride.ClearLine(player, 1, BroadcastPriority.VeryLow);
    //            BroadcastOverride.ClearLine(player, 2, BroadcastPriority.VeryLow);
    //            BroadcastOverride.UpdateIfDirty(player);
    //            RespawnPlayer(player);
    //        }
    //        else
    //        {
    //            if (new_target.PlayerId == Server.Instance.PlayerId || old_target.PlayerId == Server.Instance.PlayerId)
    //            {
    //                players[player.PlayerId].is_spectating = false;
    //                RespawnPlayer(player);
    //            }
    //        }
    //    }

    //    //[PluginEvent(ServerEventType.PlayerDropItem)]
    //    //bool OnPlayerDropitem(Player player, ItemBase item)
    //    //{
    //    //    if (!players.ContainsKey(player.PlayerId))
    //    //        return true;
    //    //
    //    //    bool drop_allowed = false;
    //    //    RDMPlayer rdm = players[player.PlayerId];
    //    //
    //    //    switch (rdm.menu_page)
    //    //    {
    //    //        case MenuPage.None:
    //    //            drop_allowed = HandleMenuPageNone(player, rdm, item);
    //    //            break;
    //    //        case MenuPage.Main:
    //    //            HandleMenuPageMain(player, rdm, item);
    //    //            break;
    //    //        case MenuPage.GunSlot:
    //    //            HandleMenuPageGunSlot(player, rdm, item);
    //    //            break;
    //    //        case MenuPage.GunClass:
    //    //            HandleMenuPageGunClass(player, rdm, item);
    //    //            break;
    //    //        case MenuPage.MtfGun:
    //    //            HandleMenuPageGun(player, rdm, item);
    //    //            break;
    //    //        case MenuPage.ChaosGun:
    //    //            HandleMenuPageGun(player, rdm, item);
    //    //            break;
    //    //        case MenuPage.KillstreakMode:
    //    //            HandleMenuPageKillstreakMode(player, rdm, item);
    //    //            break;
    //    //        case MenuPage.Preference:
    //    //            HandleMenuPagePreferences(player, rdm, item);
    //    //            break;
    //    //        case MenuPage.Stats:
    //    //            HandleMenuPageStats(player, rdm, item);
    //    //            break;
    //    //        case MenuPage.Debug:
    //    //            HandleMenuPageDebug(player, rdm, item);
    //    //            break;
    //    //    }
    //    //    BroadcastOverride.UpdateIfDirty(player);
    //    //    return drop_allowed;
    //    //}

    //    //[PluginEvent(ServerEventType.PlayerDropItem)]
    //    //bool OnPlayerDropitem(Player player, ItemBase item)
    //    //{
    //    //    if (!players.ContainsKey(player.PlayerId))
    //    //        return true;
    //    //
    //    //    bool drop_allowed = false;
    //    //    RDMPlayer rdm = players[player.PlayerId];
    //    //    if (InventoryMenu.GetPlayerMenuID(player) == (int)DeathmatchMenu.MenuPage.None)
    //    //    {
    //    //        if (IsGun(item.ItemTypeId))
    //    //        {
    //    //            if (!rdm.loadout_locked)
    //    //            {
    //    //                if (item.ItemTypeId == rdm.primary)
    //    //                {
    //    //                    rdm.primary = ItemType.None;
    //    //                    RemoveItem(player, item.ItemTypeId);
    //    //                }
    //    //                else if (item.ItemTypeId == rdm.secondary)
    //    //                {
    //    //                    rdm.secondary = ItemType.None;
    //    //                    RemoveItem(player, item.ItemTypeId);
    //    //                }
    //    //                else if (item.ItemTypeId == rdm.tertiary)
    //    //                {
    //    //                    rdm.tertiary = ItemType.None;
    //    //                    RemoveItem(player, item.ItemTypeId);
    //    //                }
    //    //
    //    //                if (rdm.IsEmptyLoadout)
    //    //                {
    //    //                    BroadcastOverride.ClearLines(player, BroadcastPriority.High);
    //    //                    BroadcastOverride.BroadcastLine(player, 1, 300, BroadcastPriority.High, DeathmatchMenu.loadout_customisation_hint);
    //    //                    if (rdm.in_spawn)
    //    //                        BroadcastOverride.BroadcastLine(player, 2, 300, BroadcastPriority.High, DeathmatchMenu.teleport_msg);
    //    //                }
    //    //            }
    //    //            else
    //    //            {
    //    //                BroadcastOverride.ClearLines(player, BroadcastPriority.High);
    //    //                BroadcastOverride.BroadcastLines(player, 1, 3, BroadcastPriority.High, DeathmatchMenu.loadout_customisation_denied);
    //    //            }
    //    //        }
    //    //        else if (item.Category != ItemCategory.Armor)
    //    //        {
    //    //            if (item.ItemTypeId == ItemType.Radio)
    //    //            {
    //    //                RemoveItem(player, ItemType.Radio);
    //    //                BroadcastOverride.BroadcastLine(player, 1, 5, BroadcastPriority.High, "<color=#FF0000>Radio can be disabled in</color> <b><color=#43BFF0>[MAIN MENU]</color> -> <color=#43BFF0>[PREFERENCES]</color> -> <color=#eb0d47>[GUARD]</color></b>");
    //    //            }
    //    //            else if (rdm.loadout_locked)
    //    //                drop_allowed = true;
    //    //        }
    //    //    }
    //    //    BroadcastOverride.UpdateIfDirty(player);
    //    //    return drop_allowed;
    //    //}

    //    [PluginEvent(ServerEventType.PlayerDropAmmo)]
    //    bool OnPlayerDropAmmo(Player player, ItemType ammo, int amount)
    //    {
    //        return false;
    //    }

    //    //[PluginEvent(ServerEventType.PlayerDamage)]
    //    //void OnPlayerDamage(Player victim, Player attacker, DamageHandlerBase damage)
    //    //{
    //    //    if(attacker != null && players.ContainsKey(attacker.PlayerId))
    //    //    {
    //    //        RDMPlayer rdm_attacker = players[attacker.PlayerId];
    //    //        RDMStats stats = rdm_attacker.stats;
    //    //        stats.shots_hit++;
    //    //        if(damage is StandardDamageHandler standard)
    //    //        {
    //    //            if (standard.Hitbox == HitboxType.Headshot)
    //    //                stats.headshots++;
    //    //            stats.damage_delt += (int)math.round(standard.Damage);
    //    //        }
    //    //    }
    //    //
    //    //    if(victim != null && players.ContainsKey(victim.PlayerId))
    //    //    {
    //    //        RDMPlayer rdm_victim = players[victim.PlayerId];
    //    //        RDMStats stats = rdm_victim.stats;
    //    //        if (damage is StandardDamageHandler standard)
    //    //        {
    //    //            stats.damage_recieved += (int)math.round(standard.Damage);
    //    //        }
    //    //    }
    //    //}

    //    //[PluginEvent(ServerEventType.PlayerShotWeapon)]
    //    //void OnPlayerShotWeapon(Player player, Firearm firearm)
    //    //{
    //    //    //players[player.PlayerId].stats.shots++;
    //    //    players[player.PlayerId].loadout_locked = true;
    //    //}

    //    //[PluginEvent(ServerEventType.PlayerUsedItem)]
    //    //void OnPlayerUsedItem(Player player, ItemBase item)
    //    //{
    //    //    players[player.PlayerId].loadout_locked = true;
    //    //}

    //    //[PluginEvent(ServerEventType.PlayerUsingRadio)]
    //    //void OnPlayerUsingRadio(Player player, RadioItem radio, float drain)
    //    //{
    //    //    radio.BatteryPercent = 100;
    //    //}

    //    [PluginEvent(ServerEventType.PlayerSpawn)]
    //    void OnPlayerSpawn(Player player, RoleTypeId role)
    //    {
    //        if (!players.ContainsKey(player.PlayerId))
    //            return;

    //        if (role != RoleTypeId.ClassD && role != RoleTypeId.Spectator)
    //            player.SetRole(RoleTypeId.ClassD);
    //        else if (role == RoleTypeId.ClassD)
    //        {
    //            Timing.CallDelayed(0.5f, () =>
    //            {
    //                player.EffectsManager.ChangeState<CustomPlayerEffects.SpawnProtected>(1, 7);
    //            });
    //        }
    //    }

    //    //[PluginEvent(ServerEventType.PlayerChangeRole)]
    //    //void OnPlayerChangeRole(Player player, PlayerRoleBase old_role, RoleTypeId new_role, RoleChangeReason reason)
    //    //{
    //    //    if (player == null || !players.ContainsKey(player.PlayerId))
    //    //        return;

    //    //    if (new_role == RoleTypeId.ClassD)
    //    //    {
    //    //        //player.ReferenceHub.roleManager.CurrentRole.RoleHelpInfo = new GameObject();
    //    //        //player.VoiceModule.CurrentChannel = VoiceChat.VoiceChatChannel.Spectator;
    //    //        //player.RoleBase.RoleHelpInfo = new GameObject();

    //    //        RDMPlayer rdm = players[player.PlayerId];

    //    //        //rdm.loadout_locked = false;
    //    //        rdm.in_spawn = true;

    //    //        Timing.CallDelayed(0.5f, () =>
    //    //        {
    //    //            if (player == null || !players.ContainsKey(player.PlayerId))
    //    //                return;
    //    //            SetPlayerRDMInv(player);

    //    //            if (rdm.IsEmptyLoadout)
    //    //            {
    //    //                BroadcastOverride.BroadcastLine(player, 1, 300, BroadcastPriority.High, DeathmatchMenu.loadout_customisation_hint);
    //    //                BroadcastOverride.BroadcastLine(player, 2, 300, BroadcastPriority.High, DeathmatchMenu.teleport_msg);
    //    //            }
    //    //            else
    //    //            {
    //    //                BroadcastOverride.BroadcastLine(player, 1, 6, BroadcastPriority.Low, "<color=#43BFF0>Teleporting in 6 seconds</color>");
    //    //                BroadcastOverride.BroadcastLine(player, 2, 6, BroadcastPriority.Low, "<color=#43BFF0>Open [MAIN MENU] to cancel</color>");
    //    //                rdm.teleport_handle = Timing.CallDelayed(6.0f, () =>
    //    //                {
    //    //                    if (Player.GetPlayers().Count == 1)
    //    //                    {
    //    //                        BroadcastOverride.BroadcastLines(player, 1, 1500.0f, BroadcastPriority.Low, DeathmatchMenu.waiting_for_players_msg);
    //    //                        BroadcastOverride.UpdateIfDirty(player);
    //    //                    }
    //    //                    else if (Player.GetPlayers().Count >= 2 && !game_started)
    //    //                    {
    //    //                        game_started = true;
    //    //                        BroadcastOverride.ClearLines(BroadcastPriority.Low);
    //    //                        BroadcastOverride.UpdateAllDirty();
    //    //                    }
    //    //                    TeleportRandom(player);
    //    //                });
    //    //            }
    //    //            BroadcastOverride.UpdateIfDirty(player);
    //    //        });
    //    //    }
    //    //}

    //    [PluginEvent(ServerEventType.LczDecontaminationStart)]
    //    void OnLczDecontaminationStart()
    //    {
    //        Timing.CallDelayed(3.0f, () => { Round.Restart(false); });
    //    }

    //    [PluginEvent(ServerEventType.RoundEndConditionsCheck)]
    //    void OnRoundEndConditionsCheck(bool condition)
    //    {

    //    }

    //    [PluginEvent(ServerEventType.RoundRestart)]
    //    void OnRoundRestart()
    //    {
    //        //Timing.KillCoroutines(teleport_queue_coroutine);
    //        RoomScale.Reset();
    //        RoomScale.Disable();
    //        Timing.KillCoroutines();
    //    }

    //    //public static string KillstreakColorCode(Player player)
    //    //{
    //    //    KillstreakMode mode = players[player.PlayerId].killstreak_mode;

    //    //    switch (mode)
    //    //    {
    //    //        case KillstreakMode.Easy:
    //    //            return "<color=#5900ff>";
    //    //        case KillstreakMode.Standard:
    //    //            return "<color=#43BFF0>";
    //    //        case KillstreakMode.Expert:
    //    //            return "<color=#36a832>";
    //    //        case KillstreakMode.Rage:
    //    //            return "<color=#FF0000>";
    //    //    }
    //    //    return "<color=#43BFF0>";
    //    //}

    //    public static bool IsPlayerValid(Player player)
    //    {
    //        return players.ContainsKey(player.PlayerId);
    //    }

    //    //int menu_lines = 7;

    //    //private bool HandleMenuPageNone(Player player, RDMPlayer rdm, ItemBase item)
    //    //{
    //    //    bool drop_allowed = false;
    //    //    if (IsGun(item.ItemTypeId) || item.ItemTypeId == ItemType.KeycardO5)
    //    //    {
    //    //        if (!rdm.loadout_locked)
    //    //        {
    //    //            if (item.ItemTypeId == ItemType.KeycardO5)
    //    //            {
    //    //                rdm.customising_loadout = true;
    //    //                Timing.KillCoroutines(rdm.teleport_handle);
    //    //                GoToMenu(player, MenuPage.Main, menu_lines, main_menu, main_menu_details);
    //    //            }
    //    //            else if (item.ItemTypeId == rdm.primary)
    //    //            {
    //    //                rdm.primary = ItemType.None;
    //    //                RemoveItem(player, item.ItemTypeId);
    //    //            }
    //    //            else if (item.ItemTypeId == rdm.secondary)
    //    //            {
    //    //                rdm.secondary = ItemType.None;
    //    //                RemoveItem(player, item.ItemTypeId);
    //    //            }
    //    //            else if (item.ItemTypeId == rdm.tertiary)
    //    //            {
    //    //                rdm.tertiary = ItemType.None;
    //    //                RemoveItem(player, item.ItemTypeId);
    //    //            }

    //    //            if (rdm.menu_page == MenuPage.None && rdm.IsEmptyLoadout)
    //    //            {
    //    //                BroadcastOverride.ClearLines(player, BroadcastPriority.High);
    //    //                BroadcastOverride.BroadcastLine(player, 1, 300, BroadcastPriority.High, loadout_customisation_hint);
    //    //                if (rdm.in_spawn)
    //    //                    BroadcastOverride.BroadcastLine(player, 2, 300, BroadcastPriority.High, teleport_msg);
    //    //            }
    //    //        }
    //    //        else
    //    //        {
    //    //            BroadcastOverride.ClearLines(player, BroadcastPriority.High);
    //    //            BroadcastOverride.BroadcastLines(player, 1, 3, BroadcastPriority.High, loadout_customisation_denied);
    //    //        }
    //    //    }
    //    //    else if (item.Category != ItemCategory.Armor)
    //    //    {
    //    //        if(item.ItemTypeId == ItemType.Radio)
    //    //        {
    //    //            RemoveItem(player, ItemType.Radio);
    //    //            BroadcastOverride.BroadcastLine(player, 1, 5, BroadcastPriority.High, "<color=#FF0000>Radio can be disabled in</color> <b><color=#43BFF0>[MAIN MENU]</color> -> <color=#43BFF0>[PREFERENCES]</color> -> <color=#eb0d47>[GUARD]</color></b>");
    //    //        }
    //    //        else if (rdm.loadout_locked)
    //    //            drop_allowed = true;
    //    //    }
    //    //    return drop_allowed;
    //    //}


    //    //private void HandleMenuPageMain(Player player, RDMPlayer rdm, ItemBase item)
    //    //{
    //    //    if (item.ItemTypeId == ItemType.KeycardO5)
    //    //    {
    //    //        rdm.customising_loadout = false;
    //    //        rdm.menu_page = MenuPage.None;
    //    //        ClearInventory(player);
    //    //        SetPlayerRDMInv(player);
    //    //        BroadcastOverride.ClearLines(player, BroadcastPriority.High);
    //    //        Killfeed.SetBroadcastKillfeedLayout(player);
    //    //        if (rdm.IsEmptyLoadout)
    //    //        {
    //    //            BroadcastOverride.BroadcastLine(player, 1, 300, BroadcastPriority.High, loadout_customisation_hint);
    //    //            if (rdm.in_spawn)
    //    //                BroadcastOverride.BroadcastLine(player, 2, 300, BroadcastPriority.High, teleport_msg);
    //    //        }
    //    //        else
    //    //        {
    //    //            if (rdm.in_spawn)
    //    //            {
    //    //                BroadcastOverride.BroadcastLine(player, 1, 3, BroadcastPriority.VeryLow, "<color=#43BFF0>loadout set, teleporting in 3 seconds</color>");
    //    //                rdm.teleport_handle = Timing.CallDelayed(3.0f, () =>
    //    //                {
    //    //                    if (Player.GetPlayers().Count == 1)
    //    //                    {
    //    //                        BroadcastOverride.BroadcastLines(player, 1, 1500.0f, BroadcastPriority.Low, waiting_for_players_msg);
    //    //                        BroadcastOverride.UpdateIfDirty(player);
    //    //                    }
    //    //                    else if (Player.GetPlayers().Count >= 2 && !game_started)
    //    //                    {
    //    //                        game_started = true;
    //    //                        BroadcastOverride.ClearLines(BroadcastPriority.Low);
    //    //                        BroadcastOverride.UpdateAllDirty();
    //    //                    }
    //    //                    TeleportRandom(player);
    //    //                });
    //    //            }
    //    //        }
    //    //    }
    //    //    else if (item.ItemTypeId == ItemType.KeycardScientist)
    //    //    {
    //    //        if (rdm.killstreak_mode != KillstreakMode.Rage)
    //    //        {
    //    //            if (rdm.killstreak_mode == KillstreakMode.Expert || rdm.killstreak_mode == KillstreakMode.Standard)
    //    //                GoToMenu(player, MenuPage.GunSlot, menu_lines, gun_slot_menu_2_slot, gun_slot_menu_details_2_slot);
    //    //            else
    //    //                GoToMenu(player, MenuPage.GunSlot, menu_lines, gun_slot_menu_3_slot, gun_slot_menu_details_3_slot);
    //    //        }
    //    //        else
    //    //        {
    //    //            BroadcastOverride.BroadcastLine(player, 3, 1, BroadcastPriority.High, "<b>[DATA EXPUNGED]</b>");
    //    //            RemoveItem(player, ItemType.KeycardScientist);
    //    //        }
    //    //    }
    //    //    else if (item.ItemTypeId == ItemType.KeycardResearchCoordinator)
    //    //    {
    //    //        List<string> killstreak_mode_selection_details = killstreak_mode_details.ToList();
    //    //        List<ItemType> killstreak_mode_menu_special = killstreak_mode_menu.ToList();

    //    //        if (!rdm.rage_mode_enabled)
    //    //        {
    //    //            killstreak_mode_selection_details.RemoveAt(killstreak_mode_selection_details.Count - 1);
    //    //            killstreak_mode_menu_special.RemoveAt(killstreak_mode_menu_special.Count - 1);
    //    //        }
    //    //        killstreak_mode_selection_details.Add("Current killstreak reward system selected: " + KillstreakColorCode(player) + rdm.killstreak_mode.ToString() + "</color>");

    //    //        GoToMenu(player, MenuPage.KillstreakMode, menu_lines, killstreak_mode_menu_special, killstreak_mode_selection_details);
    //    //    }
    //    //    else if (item.ItemTypeId == ItemType.KeycardContainmentEngineer)
    //    //    {
    //    //        GoToMenu(player, MenuPage.Preference, menu_lines, preferences_menu, preference_menu_details);
    //    //    }
    //    //    else if (main_menu.Contains(ItemType.Coin) && item.ItemTypeId == ItemType.Coin)
    //    //    {
    //    //        GoToMenu(player, MenuPage.Debug, 0, debug_menu, debug_menu_details);
    //    //    }
    //    //}

    //    //private void HandleMenuPageGunSlot(Player player, RDMPlayer rdm, ItemBase item)
    //    //{
    //    //    if (item.ItemTypeId == ItemType.KeycardO5)
    //    //    {
    //    //        GoToMenu(player, MenuPage.Main, menu_lines, main_menu, main_menu_details);
    //    //    }
    //    //    else
    //    //    {
    //    //        if (item.ItemTypeId == ItemType.KeycardNTFCommander || item.ItemTypeId == ItemType.KeycardFacilityManager)
    //    //            rdm.slot = GunSlot.Primary;
    //    //        else if (item.ItemTypeId == ItemType.KeycardNTFLieutenant || item.ItemTypeId == ItemType.KeycardZoneManager)
    //    //            rdm.slot = GunSlot.Secondary;
    //    //        else if (item.ItemTypeId == ItemType.KeycardNTFOfficer)
    //    //            rdm.slot = GunSlot.Tertiary;

    //    //        GoToMenu(player, MenuPage.GunClass, menu_lines, gun_class_menu, gun_class_menu_details);
    //    //    }
    //    //}

    //    //private void HandleMenuPageGunClass(Player player, RDMPlayer rdm, ItemBase item)
    //    //{
    //    //    if (item.ItemTypeId == ItemType.KeycardO5)
    //    //        GoToMenu(player, MenuPage.Main, menu_lines, main_menu, main_menu_details);
    //    //    else if (item.ItemTypeId == ItemType.KeycardNTFCommander)
    //    //        GoToMenu(player, MenuPage.MtfGun, 0, mtf_gun_menu, mtf_gun_menu_details);
    //    //    else if (item.ItemTypeId == ItemType.KeycardChaosInsurgency)
    //    //        GoToMenu(player, MenuPage.ChaosGun, 0, chaos_gun_menu, chaos_gun_menu_details);
    //    //}

    //    //private void HandleMenuPageGun(Player player, RDMPlayer rdm, ItemBase item)
    //    //{
    //    //    List<string> main_menu_selection_details = main_menu_details.ToList();
    //    //    if (item.ItemTypeId != ItemType.KeycardO5)
    //    //    {
    //    //        if (rdm.slot == GunSlot.Primary)
    //    //            rdm.primary = item.ItemTypeId;
    //    //        else if (rdm.slot == GunSlot.Secondary)
    //    //            rdm.secondary = item.ItemTypeId;
    //    //        else if (rdm.slot == GunSlot.Tertiary)
    //    //            rdm.tertiary = item.ItemTypeId;

    //    //        string gun_name = Enum.GetName(typeof(ItemType), item.ItemTypeId).Substring(3);
    //    //        main_menu_selection_details.Add("<color=#43BFF0>" + gun_name + "</color> added to your loadout as the <color=#FF0000>" + rdm.slot.ToString() + "</color> weapon");
    //    //    }
    //    //    GoToMenu(player, MenuPage.Main, menu_lines, main_menu, main_menu_selection_details);
    //    //}

    //    //private void HandleMenuPageKillstreakMode(Player player, RDMPlayer rdm, ItemBase item)
    //    //{
    //    //    List<string> main_menu_selection_details = main_menu_details.ToList();

    //    //    if (item.ItemTypeId != ItemType.KeycardO5)
    //    //    {
    //    //        if (item.ItemTypeId == ItemType.ArmorHeavy)
    //    //            rdm.killstreak_mode = KillstreakMode.Easy;
    //    //        else
    //    //        {
    //    //            rdm.tertiary = ItemType.None;
    //    //            if (item.ItemTypeId == ItemType.ArmorCombat)
    //    //                rdm.killstreak_mode = KillstreakMode.Standard;
    //    //            else if (item.ItemTypeId == ItemType.ArmorLight)
    //    //                rdm.killstreak_mode = KillstreakMode.Expert;
    //    //            else if (item.ItemTypeId == ItemType.GunCom45)
    //    //                rdm.killstreak_mode = KillstreakMode.Rage;
    //    //        }
    //    //        main_menu_selection_details.Add(KillstreakColorCode(player) + Enum.GetName(typeof(KillstreakMode), rdm.killstreak_mode) + "</color> selected as your killstreak reward system");
    //    //    }
    //    //    GoToMenu(player, MenuPage.Main, menu_lines, main_menu, main_menu_selection_details);
    //    //}

    //    //private void HandleMenuPagePreferences(Player player, RDMPlayer rdm, ItemBase item)
    //    //{
    //    //    if (item.ItemTypeId == ItemType.KeycardO5)
    //    //    {
    //    //        GoToMenu(player, MenuPage.Main, menu_lines, main_menu, main_menu_details);
    //    //    }
    //    //    else if (item.ItemTypeId == ItemType.KeycardGuard)
    //    //    {
    //    //        rdm.loadout_radio = !rdm.loadout_radio;
    //    //        BroadcastOverride.BroadcastLine(player, 7, 300, BroadcastPriority.High, "<b><color=#43BFF0>Loadout Radio: </color></b>" + (rdm.loadout_radio ? "Enabled" : "Disabled"));
    //    //    }
    //    //    else if (item.ItemTypeId == ItemType.KeycardScientist)
    //    //    {
    //    //        GoToMenu(player, MenuPage.Stats, 0, stats_menu, stats_menu_details);
    //    //        DisplayStats(3, player);
    //    //    }
    //    //    else if (item.ItemTypeId == ItemType.Flashlight)
    //    //    {
    //    //        rdm.is_spectating = true;
    //    //        BroadcastOverride.SetEvenLineSizes(player, 4);
    //    //        BroadcastOverride.ClearLines(player, BroadcastPriority.High);
    //    //        BroadcastOverride.BroadcastLine(player, 1, 10, BroadcastPriority.High, "spectator mode is currently bugged, you need to leave and rejoin to respawn");
    //    //        player.SetRole(RoleTypeId.Spectator);
    //    //    }
    //    //    else if (item.ItemTypeId == ItemType.Coin)
    //    //    {
    //    //        rdm.rage_mode_enabled = true;
    //    //        GoToMenu(player, MenuPage.Main, 6, main_menu, main_menu_details);
    //    //    }
    //    //}

    //    //private void HandleMenuPageStats(Player player, RDMPlayer rdm, ItemBase item)
    //    //{
    //    //    if (item.ItemTypeId == ItemType.KeycardO5)
    //    //    {
    //    //        BroadcastOverride.ClearLines(player, BroadcastPriority.Highest);
    //    //        GoToMenu(player, MenuPage.Preference, menu_lines, preferences_menu, preference_menu_details);
    //    //    }
    //    //}

    //    //private void HandleMenuPageDebug(Player player, RDMPlayer rdm, ItemBase item)
    //    //{
    //    //    if (item.ItemTypeId == ItemType.KeycardO5)
    //    //    {
    //    //        BroadcastOverride.ClearLines(player, BroadcastPriority.Highest);
    //    //        GoToMenu(player, MenuPage.Main, 6, main_menu, main_menu_details);
    //    //    }
    //    //    else if (item.ItemTypeId == ItemType.KeycardGuard)
    //    //    {
    //    //        BroadcastOverride.BroadcastLine(player, 5, 3.0f, BroadcastPriority.High, "closed all rooms");
    //    //        RoomScale.CloseAllRooms();
    //    //    }
    //    //    else if (item.ItemTypeId == ItemType.KeycardNTFOfficer)
    //    //    {
    //    //        BroadcastOverride.BroadcastLine(player, 5, 3.0f, BroadcastPriority.High, "opened all rooms");
    //    //        RoomScale.OpenAllRooms();
    //    //    }
    //    //    else if (item.ItemTypeId == ItemType.KeycardNTFLieutenant)
    //    //    {
    //    //        BroadcastOverride.BroadcastLine(player, 5, 3.0f, BroadcastPriority.High, "opened 2 rooms");
    //    //        RoomScale.OpenRooms(2);
    //    //    }
    //    //    else if (item.ItemTypeId == ItemType.KeycardNTFCommander)
    //    //    {
    //    //        BroadcastOverride.BroadcastLine(player, 5, 3.0f, BroadcastPriority.High, "closing 2 rooms");
    //    //        RoomScale.CloseRooms(2);
    //    //    }
    //    //}

    //    //private void GoToMenu(Player player, MenuPage page, int lines, List<ItemType> items, List<string> descriptions)
    //    //{
    //    //    if (lines != 0)
    //    //        BroadcastOverride.SetEvenLineSizes(player, lines);
    //    //    BroadcastOverride.ClearLines(player, BroadcastPriority.High);
    //    //    BroadcastOverride.BroadcastLines(player, 1, 300, BroadcastPriority.High, descriptions);
    //    //    ClearInventory(player);
    //    //    AddItemsInorder(player, items);
    //    //    players[player.PlayerId].menu_page = page;
    //    //}

    //    //public static void DisplayStats(int line, Player player)
    //    //{
    //    //    RDMPlayer rdm = players[player.PlayerId];
    //    //    RDMStats stats = rdm.stats;
    //    //    float mins_alive = 60.0f / math.max(stats.time_alive, 300);
    //    //    float kd = (float)stats.kills / stats.deaths;
    //    //    float HsK = (float)stats.headshot_kills / stats.kills;
    //    //    float accuracy = (float)stats.shots_hit / stats.shots;
    //    //    float score = kd * HsK * accuracy / mins_alive;
    //    //
    //    //    string stats_msg_1 = "<color=#76b8b5>Kills:</color> <color=#FF0000>" + stats.kills + "</color>    <color=#76b8b5>Deaths:</color> <color=#FF0000>" + stats.deaths + "</color>    <color=#76b8b5>K/D:</color> <color=#FF0000>" + kd.ToString("0.0") + "</color>    <color=#76b8b5>Highest Killstreak:</color> <color=#FF0000>" + stats.highest_killstreak + "</color>" + "</color>    <color=#76b8b5>Score:</color> <color=#FF0000>" + score.ToString("0.0") + "</color>";
    //    //    string stats_msg_2 = "<color=#76b8b5>Hs Kills:</color> <color=#FF0000>" + (HsK * 100.0f).ToString("0") + "%</color>    <color=#76b8b5>Hs:</color> <color=#FF0000>" + (((float)stats.headshots / stats.shots_hit) * 100.0f).ToString("0.0") + "%</color>    <color=#76b8b5>Accuracy:</color> <color=#FF0000>" + (accuracy * 100.0f).ToString("0") + "%</color>    <color=#76b8b5>Dmg Delt:</color> <color=#FF0000>" + stats.damage_delt + "</color>    <color=#76b8b5>Dmg Taken:</color> <color=#FF0000>" + stats.damage_recieved + "</color>";
    //    //    BroadcastOverride.BroadcastLine(player, line, 300, BroadcastPriority.Highest, stats_msg_1);
    //    //    BroadcastOverride.BroadcastLine(player, line + 1, 300, BroadcastPriority.Highest, stats_msg_2);
    //    //}

    //    //private void DisplayRoundStats()
    //    //{
    //    //    int highest_killstreak = 0;
    //    //    string highest_killstreak_name = "N/A";
    //    //
    //    //    int most_kills = 0;
    //    //    string most_kills_name = "N/A";
    //    //
    //    //    float most_score = 0.0f;
    //    //    string best_player_name = "N/A";
    //    //
    //    //    foreach (Player player in Player.GetPlayers())
    //    //    {
    //    //        RDMPlayer rdm = players[player.PlayerId];
    //    //        RDMStats stats = rdm.stats;
    //    //        if (stats.highest_killstreak > highest_killstreak)
    //    //        {
    //    //            highest_killstreak = stats.highest_killstreak;
    //    //            highest_killstreak_name = player.Nickname;
    //    //        }
    //    //
    //    //        if (stats.kills > most_kills)
    //    //        {
    //    //            most_kills = stats.kills;
    //    //            most_kills_name = player.Nickname;
    //    //        }
    //    //
    //    //        float mins_alive = 60.0f / math.max(stats.time_alive, 300);
    //    //        float kd = (float)stats.kills / stats.deaths;
    //    //        float HsK = (float)stats.headshot_kills / stats.kills;
    //    //        float accuracy = (float)stats.shots_hit / stats.shots;
    //    //        float score = kd * HsK * accuracy / mins_alive;
    //    //
    //    //        if (score > most_score)
    //    //        {
    //    //            most_score = score;
    //    //            best_player_name = player.Nickname;
    //    //        }
    //    //    }
    //    //
    //    //
    //    //    string highest_killstreak_msg = "<b><color=#43BFF0>" + highest_killstreak_name + "</color></b> <color=#d4af37>had the highest killstreak of</color> <b><color=#FF0000>" + highest_killstreak.ToString() + "</color></b>";
    //    //    string most_kills_msg = "<b><color=#43BFF0>" + most_kills_name + "</color></b> <color=#c0c0c0>had the most kills</color> <b><color=#FF0000>" + most_kills.ToString() + "</color></b>";
    //    //    string highest_score_msg = "<b><color=#43BFF0>" + best_player_name + "</color></b> <color=#a97142> was the best player with a score of </color> <b><color=#FF0000>" + most_score.ToString("0.0") + "</color></b>";
    //    //
    //    //    foreach (Player player in Player.GetPlayers())
    //    //        BroadcastOverride.SetEvenLineSizes(player, 5);
    //    //
    //    //    BroadcastOverride.BroadcastLine(1, 300, BroadcastPriority.Highest, highest_killstreak_msg);
    //    //    BroadcastOverride.BroadcastLine(2, 300, BroadcastPriority.Highest, most_kills_msg);
    //    //    BroadcastOverride.BroadcastLine(3, 300, BroadcastPriority.Highest, highest_score_msg);
    //    //
    //    //    foreach (Player player in Player.GetPlayers())
    //    //        DisplayStats(4, player);
    //    //    BroadcastOverride.UpdateAllDirty();
    //    //}

    //    //public static bool RemoveItem(Player player, ItemType type)
    //    //{
    //    //    IEnumerable<ItemBase> matches = player.Items.Where((i) => i.ItemTypeId == type);
    //    //    if(matches.Count() >= 1)
    //    //    {
    //    //        player.RemoveItem(new Item(matches.First()));
    //    //        return true;
    //    //    }
    //    //    return false;
    //    //}

    //    private void ClearInventory(Player player)
    //    {
    //        player.ClearInventory();
    //    }

    //    private void RespawnPlayer(Player player)
    //    {
    //        if (player.Role == RoleTypeId.Spectator)
    //        {
    //            player.SetRole(RoleTypeId.ClassD, RoleChangeReason.Died);
    //        }
    //    }

    //    //static bool once = false;
    //    public static void TeleportRandom(Player player)
    //    {
    //        //teleport_queue.Enqueue(player);
    //        //if (!teleport_queue_coroutine.IsValid)
    //        //    teleport_queue_coroutine = Timing.RunCoroutine(UpdateTeleportQueue());

    //        try
    //        {

    //            if (player == null)
    //            {
    //                ServerConsole.AddLog("could not teleport player because player was null");
    //                return;
    //            }

    //            if (!players.ContainsKey(player.PlayerId))
    //            {
    //                ServerConsole.AddLog("could not teleport player: " + player.Nickname + " because they where never added to players");
    //                return;
    //            }

    //            RDMPlayer rdm = players[player.PlayerId];

    //            if (rdm.in_spawn)
    //            {
    //                ServerConsole.AddLog("teleporting player: " + player.Nickname);

    //                SortedSet<RoomName> room_black_list = new SortedSet<RoomName>
    //                {
    //                    RoomName.Lcz173,
    //                    RoomName.HczTesla,
    //                    RoomName.EzCollapsedTunnel,
    //                    RoomName.EzEvacShelter,
    //                    RoomName.HczTestroom
    //                };

    //                HashSet<RoomIdentifier> occupied_rooms = new HashSet<RoomIdentifier>();
    //                foreach (Player p in Player.GetPlayers())
    //                {
    //                    if (p != null && p.Role == RoleTypeId.ClassD && p.Room != null && players.ContainsKey(p.PlayerId) && !players[p.PlayerId].in_spawn)
    //                        occupied_rooms.Add(p.Room);
    //                }

    //                IEnumerable<RoomIdentifier> valid_rooms = RoomScale.ValidSpawnRooms.Where((r) => { return !room_black_list.Contains(r.Name); });
    //                IEnumerable<RoomIdentifier> available_rooms = valid_rooms.Where((r) => { return !occupied_rooms.Contains(r); });
    //                ServerConsole.AddLog("valid rooms: " + valid_rooms.Count() + " avaliable rooms: " + available_rooms.Count());

    //                System.Random random = new System.Random();
    //                RoomIdentifier room;
    //                if (available_rooms.IsEmpty())
    //                    room = valid_rooms.ElementAt(random.Next(valid_rooms.Count()));
    //                else
    //                    room = available_rooms.ElementAt(random.Next(available_rooms.Count()));

    //                DeathmatchStats.SetPlayerStartTime(player, Time.time);
    //                //rdm.start_time = Time.time;
    //                rdm.in_spawn = false;
    //                player.Position = room.ApiRoom.Position + UnityEngine.Vector3.up * 0.5f;


    //                //if (!once)
    //                //{
    //                //    once = true;
    //                //    RoomScale.CloseAllRooms();
    //                //    if (player.Room != null && players.ContainsKey(player.PlayerId) && !players[player.PlayerId].in_spawn)
    //                //        RoomScale.OpenRooms(4);

    //                //}
    //                //HashSet<Vector3Int> occupide_coords = new HashSet<Vector3Int>();
    //                //foreach (Player p in Player.GetPlayers())
    //                //{
    //                //    if (p != null && p.Role == RoleTypeId.ClassD && players.ContainsKey(p.PlayerId) && !players[p.PlayerId].in_spawn)
    //                //        occupide_coords.Add(RoomIdUtils.PositionToCoords(p.Position));
    //                //}

    //                //IEnumerable<Vector3Int> valid_coords = RoomScale.ValidSpawnRooms.Where((c) => { return !room_black_list.Contains(RoomIdentifier.RoomsByCoordinates[c].Name); });
    //                //IEnumerable<Vector3Int> available_coords = valid_coords.Except(occupide_coords);

    //                //System.Random random = new System.Random();
    //                //RoomIdentifier room;
    //                //if (available_coords.IsEmpty())
    //                //    room = RoomIdentifier.RoomsByCoordinates[valid_coords.ElementAt(random.Next(valid_coords.Count()))];
    //                //else
    //                //    room = RoomIdentifier.RoomsByCoordinates[available_coords.ElementAt(random.Next(available_coords.Count()))];

    //                //rdm.start_time = Time.time;
    //                //rdm.in_spawn = false;
    //                //player.Position = room.ApiRoom.Position + UnityEngine.Vector3.up * 0.5f;




    //                //IEnumerable<KeyValuePair<Vector3Int, RoomIdentifier>> zone_filter = RoomIdentifier.RoomsByCoordinates.Where((p) => { return zones.Contains(p.Value.Zone) && !room_black_list.Contains(p.Value.Name); });
    //                //IEnumerable<KeyValuePair<Vector3Int, RoomIdentifier>> available_filter = zone_filter.Where((p) => { return !occupide_coords.Contains(p.Key); });
    //                //ServerConsole.AddLog("zone rooms: " + zone_filter.Count() + " avaliable_rooms: " + available_filter.Count());

    //                //System.Random random = new System.Random();
    //                //RoomIdentifier room;
    //                //if (available_filter.IsEmpty())
    //                //    room = zone_filter.ElementAt(random.Next(zone_filter.Count())).Value;
    //                //else
    //                //    room = available_filter.ElementAt(random.Next(available_filter.Count())).Value;

    //                //rdm.start_time = Time.time;
    //                //rdm.in_spawn = false;
    //                //player.Position = room.ApiRoom.Position + UnityEngine.Vector3.up * 0.5f;



    //                //HashSet<RoomIdentifier> occupied_rooms = new HashSet<RoomIdentifier>();
    //                //foreach (Player p in Player.GetPlayers())
    //                //{
    //                //    if (p != null && p.Role == RoleTypeId.ClassD && p.Room != null && players.ContainsKey(p.PlayerId) && !players[p.PlayerId].in_spawn)
    //                //        occupied_rooms.Add(p.Room);
    //                //}

    //                //IEnumerable<RoomIdentifier> zone_rooms = Map.Rooms.Where((r) => { return zones.Contains(r.Zone) && !room_black_list.Contains(r.Name); });
    //                //IEnumerable<RoomIdentifier> available_rooms = zone_rooms.Except(occupied_rooms);
    //                //ServerConsole.AddLog("zone rooms: " + zone_rooms.Count() + " avaliable_rooms: " + available_rooms.Count());


    //                //System.Random random = new System.Random();
    //                //RoomIdentifier room;
    //                //if (available_rooms.IsEmpty())
    //                //    room = zone_rooms.ElementAt(random.Next(zone_rooms.Count()));
    //                //else
    //                //    room = available_rooms.ElementAt(random.Next(available_rooms.Count()));

    //                //rdm.start_time = Time.time;
    //                //rdm.in_spawn = false;
    //                //player.Position = room.ApiRoom.Position + UnityEngine.Vector3.up * 0.5f;
    //            }
    //            else
    //            {
    //                ServerConsole.AddLog("could not teleport player: " + player.Nickname + " because they are not in spawn");
    //            }
    //        }
    //        catch(Exception ex)
    //        {
    //            ServerConsole.AddLog("teleport error: " + ex.ToString());
    //        }
    //    }

    //    //private IEnumerator<float> UpdateTeleportQueue()
    //    //{
    //    //    int attempt = 0;
    //    //
    //    //    while (true)
    //    //    {
    //    //        if (!teleport_queue.IsEmpty())
    //    //        {
    //    //            while (teleport_queue.Peek() == null || !players.ContainsKey(teleport_queue.Peek().PlayerId))
    //    //            {
    //    //                teleport_queue.Dequeue();
    //    //                attempt = 0;
    //    //            }
    //    //        }
    //    //
    //    //        if (!teleport_queue.IsEmpty())
    //    //        {
    //    //            SortedSet<RoomName> room_black_list = new SortedSet<RoomName>
    //    //            {
    //    //                RoomName.Lcz173,
    //    //                RoomName.HczTesla,
    //    //                RoomName.EzCollapsedTunnel,
    //    //                RoomName.EzEvacShelter,
    //    //                RoomName.HczTestroom
    //    //            };
    //    //
    //    //            SortedSet<RoomIdentifier> occupied_rooms = new SortedSet<RoomIdentifier>();
    //    //            foreach (Player p in Player.GetPlayers())
    //    //                occupied_rooms.Add(p.Room);
    //    //
    //    //            IEnumerable<RoomIdentifier> available_rooms = Map.Rooms.Except(occupied_rooms).Where((r) => { return zones.Contains(r.Zone) && !room_black_list.Contains(r.Name); });
    //    //
    //    //            attempt++;
    //    //
    //    //            if (available_rooms.IsEmpty())
    //    //            {
    //    //                BroadcastOverride.BroadcastLine(teleport_queue.Peek(), 1, 1, BroadcastPriority.VeryLow, "<color=#43BFF0>you are first in queue!</color>");
    //    //                BroadcastOverride.BroadcastLine(teleport_queue.Peek(), 2, 1, BroadcastPriority.VeryLow, "<color=#43BFF0>searching for available rooms... attempt: " + attempt + "</color>");
    //    //            }
    //    //            else
    //    //            {
    //    //                attempt = 0;
    //    //                System.Random random = new System.Random();
    //    //                RoomIdentifier room = available_rooms.ElementAt(random.Next(available_rooms.Count()));
    //    //                Player player = teleport_queue.Dequeue();
    //    //                BroadcastOverride.ClearLine(player, 1, BroadcastPriority.VeryLow);
    //    //                BroadcastOverride.ClearLine(player, 2, BroadcastPriority.VeryLow);
    //    //                players[player.PlayerId].start_time = Time.time;
    //    //                player.Position = room.ApiRoom.Position + UnityEngine.Vector3.up * 0.5f;
    //    //            }
    //    //
    //    //            int index = 1;
    //    //            foreach(Player player in teleport_queue)
    //    //            {
    //    //                if(index != 1 && player != null)
    //    //                {
    //    //                    BroadcastOverride.BroadcastLine(player, 1, 1, BroadcastPriority.VeryLow, "<color=#43BFF0>you are in the teleport queue! " + index + "/" + teleport_queue.Count + "</color>");
    //    //                    BroadcastOverride.BroadcastLine(player, 2, 1, BroadcastPriority.VeryLow, "<color=#43BFF0>no rooms available. waiting for people to die or move out of a room</color>");
    //    //                }
    //    //                index++;
    //    //            }
    //    //        }
    //    //        yield return Timing.WaitForSeconds(0.1f);
    //    //    }
    //    //}

    //    //private static void AddFirearm(Player player, ItemType type, bool grant_ammo)
    //    //{
    //    //    int ammo_reserve = 0;
    //    //    int load_ammo = 0;
    //    //    Firearm firearm = player.AddItem(type) as Firearm;
    //    //    if(firearm != null)
    //    //    {
    //    //        if (grant_ammo)
    //    //            ammo_reserve = player.GetAmmoLimit(firearm.AmmoType);
    //    //        else
    //    //            ammo_reserve = player.GetAmmo(firearm.AmmoType);

    //    //        uint attachment_code = AttachmentsServerHandler.PlayerPreferences[player.ReferenceHub][type];
    //    //        AttachmentsUtils.ApplyAttachmentsCode(firearm, attachment_code, true);
    //    //        load_ammo = math.min(ammo_reserve, firearm.AmmoManagerModule.MaxAmmo);
    //    //        firearm.Status = new FirearmStatus((byte)load_ammo, FirearmStatusFlags.MagazineInserted, attachment_code);
    //    //        ammo_reserve -= load_ammo;
    //    //        player.SetAmmo(firearm.AmmoType, (ushort)ammo_reserve);
    //    //    }
    //    //}

    //    //public static void SetPlayerRDMInv(Player player)
    //    //{
    //    //    player.EffectsManager.DisableAllEffects();
    //    //    RDMPlayer rdm = players[player.PlayerId];
    //    //    //rdm.menu_page = 0;
    //    //
    //    //    player.AddItem(ItemType.KeycardO5);
    //    //    if (rdm.loadout_radio)
    //    //    {
    //    //        RadioItem radio = player.AddItem(ItemType.Radio) as RadioItem;
    //    //        radio._rangeId = (byte)(radio.Ranges.Length - 1);
    //    //        //todo range
    //    //    }
    //    //    if (!rdm.IsEmptyLoadout)
    //    //    {
    //    //
    //    //        if (rdm.killstreak_mode == KillstreakMode.Easy)
    //    //            player.AddItem(ItemType.ArmorHeavy);
    //    //        else if (rdm.killstreak_mode == KillstreakMode.Standard)
    //    //            player.AddItem(ItemType.ArmorCombat);
    //    //        else if (rdm.killstreak_mode == KillstreakMode.Expert)
    //    //            player.AddItem(ItemType.ArmorLight);
    //    //
    //    //        if (rdm.killstreak_mode != KillstreakMode.Rage)
    //    //        {
    //    //            AddFirearm(player, rdm.primary, true);
    //    //            AddFirearm(player, rdm.secondary, GunAmmoType(rdm.primary) != GunAmmoType(rdm.secondary));
    //    //            if (rdm.killstreak_mode == KillstreakMode.Easy)
    //    //                AddFirearm(player, rdm.tertiary, GunAmmoType(rdm.primary) != GunAmmoType(rdm.tertiary) && GunAmmoType(rdm.secondary) != GunAmmoType(rdm.tertiary));
    //    //
    //    //            if (rdm.killstreak_mode == KillstreakMode.Easy)
    //    //                AddItemsInorder(player, new List<ItemType>() { ItemType.Painkillers, ItemType.Medkit, ItemType.Medkit, ItemType.Medkit });
    //    //            else if (rdm.killstreak_mode == KillstreakMode.Standard)
    //    //                AddItemsInorder(player, new List<ItemType>() { ItemType.Painkillers });
    //    //        }
    //    //        else
    //    //        {
    //    //            foreach (ItemType item in rage_kill_streak_loadout[0])
    //    //            {
    //    //                if (IsGun(item))
    //    //                    AddFirearm(player, item, true);
    //    //                else
    //    //                    player.AddItem(item);
    //    //            }
    //    //
    //    //            foreach (Effect effect in rage_kill_streak_status_effects[0])
    //    //            {
    //    //                player.EffectsManager.ChangeState(effect.name, effect.intensity);
    //    //            }
    //    //        }
    //    //    }
    //    //}


    //    //private static void AddItemsInorder(Player player, List<ItemType> items)
    //    //{
    //    //    foreach (ItemType i in items)
    //    //    {
    //    //        if (i == ItemType.Radio && !players[player.PlayerId].loadout_radio)
    //    //            continue;
    //    //        if (i == ItemType.Radio)
    //    //        {
    //    //            RadioItem radio = player.AddItem(ItemType.Radio) as RadioItem;
    //    //            radio._rangeId = (byte)(radio.Ranges.Length - 1);
    //    //        }
    //    //        else
    //    //            player.AddItem(i);
    //    //    }    
    //    //}

    //    //private static bool IsGun(ItemType type)
    //    //{
    //    //    bool result = false;
    //    //    switch(type)
    //    //    {
    //    //        case ItemType.GunCOM15:
    //    //            result = true;
    //    //            break;
    //    //        case ItemType.GunCOM18:
    //    //            result = true;
    //    //            break;
    //    //        case ItemType.GunCom45:
    //    //            result = true;
    //    //            break;
    //    //        case ItemType.GunFSP9:
    //    //            result = true;
    //    //            break;
    //    //        case ItemType.GunCrossvec:
    //    //            result = true;
    //    //            break;
    //    //        case ItemType.GunE11SR:
    //    //            result = true;
    //    //            break;
    //    //        case ItemType.GunAK:
    //    //            result = true;
    //    //            break;
    //    //        case ItemType.GunRevolver:
    //    //            result = true;
    //    //            break;
    //    //        case ItemType.GunShotgun:
    //    //            result = true;
    //    //            break;
    //    //        case ItemType.GunLogicer:
    //    //            result = true;
    //    //            break;
    //    //    }
    //    //    return result;
    //    //}

    //    //private static ItemType GunAmmoType(ItemType type)
    //    //{
    //    //    ItemType ammo = ItemType.None;
    //    //    switch (type)
    //    //    {
    //    //        case ItemType.GunCOM15:
    //    //        case ItemType.GunCOM18:
    //    //        case ItemType.GunCom45:
    //    //        case ItemType.GunFSP9:
    //    //        case ItemType.GunCrossvec:
    //    //            ammo = ItemType.Ammo9x19;
    //    //            break;
    //    //        case ItemType.GunE11SR:
    //    //            ammo = ItemType.Ammo556x45;
    //    //            break;
    //    //        case ItemType.GunAK:
    //    //        case ItemType.GunLogicer:
    //    //            ammo = ItemType.Ammo762x39;
    //    //            break;
    //    //        case ItemType.GunRevolver:
    //    //            ammo = ItemType.Ammo44cal;
    //    //            break;
    //    //        case ItemType.GunShotgun:
    //    //            ammo = ItemType.Ammo12gauge;
    //    //            break;
    //    //    }
    //    //    return ammo;
    //    //}

    //}

    ////[PluginEvent(ServerEventType.PlayerGameConsoleCommand)]
    ////void OnPlayerGameConsoleCommand(Player player, string command, string[] arguments)
    ////{
    ////    if (command == "spectate" || command == "st")
    ////    {
    ////        RDMPlayer player_rdm = players[player.PlayerId];
    ////        player_rdm.is_spectating = true;
    ////        player_rdm.killstreak = 0;
    ////        player.SetRole(RoleTypeId.Spectator);
    ////        player.SendBroadcast("You have entered spectating mode to exit spectating mode type respawn in your game console", 300, shouldClearPrevious: true);
    ////        player.SendConsoleMessage("You have entered spectating mode to exit spectating mode type respawn in your game console");
    ////    }
    ////
    ////    if (command == "respawn" || command == "rs")
    ////    {
    ////        if (!players.ContainsKey(player.PlayerId))
    ////            return;
    ////
    ////        players[player.PlayerId].is_spectating = false;
    ////        player.SetRole(RoleTypeId.ClassD);
    ////    }
    ////
    ////
    ////    if (!players.ContainsKey(player.PlayerId))
    ////        return;
    ////}

    ////[CommandHandler(typeof(ClientCommandHandler))]
    ////public class SpectateCommand : ICommand
    ////{
    ////    public string Command { get; } = "spectate";
    ////
    ////    public string[] Aliases { get; } = new string[] { "spec" };
    ////
    ////    public string Description { get; } = "Enter spectating mode. To exit spectating mode type respawn";
    ////
    ////    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    ////    {
    ////        response = "Success response";
    ////        return true;
    ////    }
    ////}

    ////[CommandHandler(typeof(ClientCommandHandler))]
    ////public class RespawnCommand : ICommand
    ////{
    ////    public string Command { get; } = "respawn";
    ////
    ////    public string[] Aliases { get; } = new string[] { "rs" };
    ////
    ////    public string Description { get; } = "Exit spectating mode. To edit attachments/presets type spectate";
    ////
    ////    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    ////    {
    ////        response = "Success response";
    ////        return true;
    ////    }
    ////}
}
