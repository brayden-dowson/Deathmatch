using CedMod.Addons.Events;
using CedMod.Addons.Events.Interfaces;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RoundSummary;

namespace TheRiptide
{
    public sealed class Config : IEventConfig
    {
        [Description("Indicates whether the event is enabled or not")]
        public bool IsEnabled { get; set; } = true;
    }

    public class DeathmatchEvent:IEvent
    {
        public static DeathmatchEvent Singleton { get; private set; }

        public static bool IsRunning = false;
        public PluginHandler Handler;

        public string EventName { get; } = "Random Deathmatch";
        public string EvenAuthor { get; } = "The Riptide";
        public string EventDescription { get; set; } = "runs the Deathmatch gamemode for a round\n\n";
        public string EventPrefix { get; } = "RD";
        public bool OverrideWinConditions { get; } = true;
        public bool BulletHolesAllowed { get; set; } = false;
        public PluginHandler PluginHandler { get; }
        public IEventConfig Config => EventConfig;

        [PluginConfig]
        public Config EventConfig;

        public void PrepareEvent()
        {
            if(Deathmatch.Singleton.config.IsEnabled)
            {
                Log.Error("You can only start a Deathmatch event when the plugin isnt enabled! Set MainConfig.yml IsEnabled = false");
                return;
            }

            Log.Info(EventName + " event is preparing");
            IsRunning = true;
            Deathmatch.Singleton.Start();
            //Deathmatch.Singleton.OnMapGenerated();
            Deathmatch.Singleton.OnWaitingForPlayers();
            Log.Info(EventName + " event is prepared");
            PluginAPI.Events.EventManager.RegisterEvents<DmRound>(this);
        }

        public void StopEvent()
        {
            if (Deathmatch.Singleton.config.IsEnabled)
                return;

            IsRunning = false;
            Deathmatch.Singleton.OnRoundEnd(LeadingTeam.Draw);
            Deathmatch.Singleton.OnRoundRestart();
            Deathmatch.Singleton.Stop();
            PluginAPI.Events.EventManager.UnregisterEvents<DmRound>(this);
        }

        [PluginEntryPoint("Random Deathmatch Event", "1.0.0", "", "The Riptide")]
        public void OnEnabled()
        {
            Singleton = this;
            //PluginAPI.Events.EventManager.RegisterEvents<EventHandler>(this);
            Handler = PluginHandler.Get(this);
        }

        [PluginUnload]
        public void OnDisabled()
        {
            StopEvent();
        }
    }
}
