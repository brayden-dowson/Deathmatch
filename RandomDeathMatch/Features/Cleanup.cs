using InventorySystem.Items.Armor;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Ammo;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using MEC;
using Mirror;
using PlayerRoles.Ragdolls;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheRiptide
{
    public class CleanupConfig
    {
        public bool IsEnabled { get; set; } = true;
        [Description("items to not cleanup when the round start e.g. scp207, medkits ect... [see global reference config for types]")]
        public List<ItemType> InitialCleanupWhitelist { get; set; } = new List<ItemType>
        {
        };

        [Description("how often to cleanup items in seconds. -1 = never")]
        public int ItemCleanupPeriod = 1;
        [Description("items to cleanup throughout the round if dropped by player [see global reference config for types]\n# armor, gun, keycards are automaticaly deleted")]
        public List<ItemType> ItemCleanupBlacklist { get; set; } = new List<ItemType>
        {
            ItemType.Jailbird
        };

        [Description("how often to cleanup ragdolls in seconds. -1 = never")]
        public int RagdollCleanupPeriod { get; set; } = -1;
    }

    class Cleanup
    {
        public static Cleanup Singleton { get; private set; }

        private CleanupConfig config;
        private CoroutineHandle item_cleanup;
        private CoroutineHandle ragdoll_cleanup;

        public Cleanup()
        {
            Singleton = this;
        }

        public void Init(CleanupConfig config)
        {
            this.config = config;
        }

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            Timing.CallDelayed(1.0f, () =>
            {
                ItemPickupBase[] items = UnityEngine.Object.FindObjectsOfType<ItemPickupBase>();
                int num = items.Length;
                for (int i = 0; i < num; i++)
                    if (!config.InitialCleanupWhitelist.Contains(items[i].NetworkInfo.ItemId))
                        NetworkServer.Destroy(items[i].gameObject);

                Timing.KillCoroutines(item_cleanup);
                if (config.ItemCleanupPeriod >= 0)
                    item_cleanup = Timing.RunCoroutine(_ItemCleanup());

                Timing.KillCoroutines(ragdoll_cleanup);
                if (config.RagdollCleanupPeriod >= 0)
                    ragdoll_cleanup = Timing.RunCoroutine(_RagdollCleanup());
            });
        }

        public void RoundRestart()
        {
            Timing.KillCoroutines(item_cleanup, ragdoll_cleanup);
        }

        private IEnumerator<float> _ItemCleanup()
        {
            while(true)
            {
                try
                {
                    ItemPickupBase[] items = UnityEngine.Object.FindObjectsOfType<ItemPickupBase>();
                    int num = items.Length;
                    for (int i = 0; i < num; i++)
                    {
                        ItemPickupBase item = items[i];
                        if (item is AmmoPickup)
                            NetworkServer.Destroy(items[i].gameObject);
                        else if (item is BodyArmorPickup)
                            NetworkServer.Destroy(items[i].gameObject);
                        else if (item is FirearmPickup)
                            NetworkServer.Destroy(items[i].gameObject);
                        else if (item is KeycardPickup)
                            NetworkServer.Destroy(items[i].gameObject);
                        else if (config.ItemCleanupBlacklist.Contains(item.NetworkInfo.ItemId))
                            NetworkServer.Destroy(items[i].gameObject);
                    }
                }
                catch(Exception ex)
                {
                    Log.Error("_ItemCleaup Error: " + ex.ToString());
                }

                yield return Timing.WaitForSeconds(config.ItemCleanupPeriod);
            }
        }

        private IEnumerator<float> _RagdollCleanup()
        {
            while (true)
            {
                try
                {
                    BasicRagdoll[] ragdolls = UnityEngine.Object.FindObjectsOfType<BasicRagdoll>();
                    foreach (var ragdoll in ragdolls)
                        NetworkServer.Destroy(ragdoll.gameObject);
                }
                catch (Exception ex)
                {
                    Log.Error("_RagdollCleaup Error: " + ex.ToString());
                }

                yield return Timing.WaitForSeconds(config.RagdollCleanupPeriod);
            }
        }
    }
}
