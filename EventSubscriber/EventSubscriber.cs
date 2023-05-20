using PlayerStatsSystem;
using System;
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
    }
}
