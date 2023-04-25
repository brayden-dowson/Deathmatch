using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginAPI.Core.Interfaces;
using Mirror;

namespace RandomDeathMatch
{
    ////IGameComponent

    ////public class BotID : IGameComponent
    ////{
    ////}

    //public class Bot : Player
    //{
    //    public Bot(UnityEngine.GameObject obj):base(obj.GetComponent<IGameComponent>())
    //    {
            
    //    }
    //}

    //public class BotManager
    //{
    //    //public static Bots Singleton { get; private set; }

    //    Dictionary<int, Bot> bots = new Dictionary<int, Bot>();

    //    //public Bots()
    //    //{
    //    //    Singleton = this;
    //    //}

    //    public List<int> Add(int count)
    //    {
    //        List<int> ids = new List<int>();
    //        for (int i = 0; i < count; i++)
    //        {
    //            UnityEngine.GameObject prefab = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
    //            ReferenceHub hub = prefab.GetComponent<ReferenceHub>();
    //            hub.roleManager.voice
    //            hub.queryProcessor._ipAddress = "127.0.0.WAN";
    //            Bot bot = new Bot(prefab);
    //            NetworkServer.Spawn(bot.GameObject);
    //            ids.Add(i);
    //            bots.Add(bots.Count, bot);
    //        }
    //        return ids;
    //    }

    //    public Bot GetBot(int id)
    //    {
    //        return bots[id];
    //    }

    //}
}
