using PlayerStatsSystem;
using System;
using GameCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheRiptide
{
    public static class EventSubscriber
    {
        public static void SubscribeOnDamaged(Action<ReferenceHub, DamageHandlerBase> action)
        {
            PlayerStats.OnAnyPlayerDamaged += action;
        }

        public static void UnsubscribeOnDamaged(Action<ReferenceHub, DamageHandlerBase> action)
        {
            PlayerStats.OnAnyPlayerDamaged -= action;
        }

        public static void SubscribeOnConfigReloaded(Action action)
        {
            ConfigFile.OnConfigReloaded += action;
        }

        public static void UnsubscribeOnConfigReloaded(Action action)
        {
            ConfigFile.OnConfigReloaded -= action;
        }
    }
}
