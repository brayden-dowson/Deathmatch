using CustomPlayerEffects;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Zones;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheRiptide
{
    //132.46 988.79 24.52
    //132.44 995.69 -27.34
    //29.19  991.88 -27.15
    //-40.68 988.01 -39.73
    //7.00   999.07 -15.02

    //class RoomScale
    //{
    //    private static Dictionary<RoomIdentifier, HashSet<RoomIdentifier>> Adjacent = new Dictionary<RoomIdentifier, HashSet<RoomIdentifier>>();
    //    private static HashSet<DoorVariant> edge_doors = new HashSet<DoorVariant>();
    //    private static List<ElevatorDoor> lcz_elevators = new List<ElevatorDoor>();
    //    private static HashSet<RoomIdentifier> active_rooms = new HashSet<RoomIdentifier>();
    //    private static Dictionary<RoomIdentifier, float> closing_rooms = new Dictionary<RoomIdentifier, float>();
    //    private static CoroutineHandle update_handle = new CoroutineHandle();
    //    private static float decontamination_warning = 30.0f;
    //    private static int surface_weight = 5;

    //    public static IEnumerable<RoomIdentifier> ValidSpawnRooms { get { return active_rooms.Except(closing_rooms.Keys); } }

    //    public static void Reset()
    //    {
    //        Adjacent.Clear();
    //        edge_doors.Clear();
    //        lcz_elevators.Clear();
    //        active_rooms.Clear();
    //        closing_rooms.Clear();
    //    }

    //    public static void BuildRoomGraph()
    //    {
    //        Adjacent.Clear();
    //        edge_doors.Clear();
    //        lcz_elevators.Clear();
    //        //foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
    //        //    Adjacent.Add(room, new HashSet<RoomIdentifier>());

    //        foreach (DoorVariant door in DoorVariant.AllDoors)
    //        {
    //            if(door.Rooms.Length == 2)
    //            {
    //                edge_doors.Add(door);
    //                if(!Adjacent.ContainsKey(door.Rooms[0]))
    //                    Adjacent.Add(door.Rooms[0], new HashSet<RoomIdentifier>());
    //                if (!Adjacent.ContainsKey(door.Rooms[1]))
    //                    Adjacent.Add(door.Rooms[1], new HashSet<RoomIdentifier>());
    //                Adjacent[door.Rooms[0]].Add(door.Rooms[1]);
    //                Adjacent[door.Rooms[1]].Add(door.Rooms[0]);
    //            }
    //        }

    //        try
    //        {
    //            if (!ConnectElevatorRooms(ElevatorManager.ElevatorGroup.LczA01))
    //                ConnectElevatorRooms(ElevatorManager.ElevatorGroup.LczA02);
    //            if (!ConnectElevatorRooms(ElevatorManager.ElevatorGroup.LczB01))
    //                ConnectElevatorRooms(ElevatorManager.ElevatorGroup.LczB02);
    //            //ConnectElevatorRooms(ElevatorManager.ElevatorGroup.GateA);
    //            //ConnectElevatorRooms(ElevatorManager.ElevatorGroup.GateB);
    //        }
    //        catch(Exception ex)
    //        {
    //            ServerConsole.AddLog("ConnectElevatorRooms error" + ex.ToString());
    //        }
    //        //List<ElevatorDoor> elevators_a = ElevatorDoor.AllElevatorDoors[ElevatorManager.ElevatorGroup.LczA01];
    //        //List<ElevatorDoor> elevators_b = ElevatorDoor.AllElevatorDoors[ElevatorManager.ElevatorGroup.LczB01];
    //        //try
    //        //{
    //        //    ServerConsole.AddLog("elevators count a: " + elevators_a.Count().ToString());
    //        //    ServerConsole.AddLog("elevators count b: " + elevators_b.Count().ToString());
    //        //    Adjacent[elevators_a[0].Rooms.First()].Add(elevators_a[1].Rooms.First());
    //        //    Adjacent[elevators_a[1].Rooms.First()].Add(elevators_a[0].Rooms.First());
    //        //    Adjacent[elevators_b[0].Rooms.First()].Add(elevators_b[1].Rooms.First());
    //        //    Adjacent[elevators_b[1].Rooms.First()].Add(elevators_b[0].Rooms.First());
    //        //}
    //        //catch(Exception ex)
    //        //{
    //        //    ServerConsole.AddLog("elevator error" + ex.ToString());
    //        //}

    //        lcz_elevators = ElevatorDoor.AllElevatorDoors[ElevatorManager.ElevatorGroup.LczA01].ToList();
    //        lcz_elevators.AddRange(ElevatorDoor.AllElevatorDoors[ElevatorManager.ElevatorGroup.LczB01]);
    //        lcz_elevators.AddRange(ElevatorDoor.AllElevatorDoors[ElevatorManager.ElevatorGroup.LczA02]);
    //        lcz_elevators.AddRange(ElevatorDoor.AllElevatorDoors[ElevatorManager.ElevatorGroup.LczB02]);
    //    }

    //    private static bool ConnectElevatorRooms(ElevatorManager.ElevatorGroup group)
    //    {
    //        List<ElevatorDoor> elevators = ElevatorDoor.AllElevatorDoors[group];
    //        if(elevators.Count() != 2)
    //        {
    //            ServerConsole.AddLog("elevator group " + group.ToString() + " has " + elevators.Count + " elevators");
    //            return false;
    //        }
    //        else
    //        {
    //            if (!Adjacent.ContainsKey(elevators[0].Rooms.First()))
    //                Adjacent.Add(elevators[0].Rooms.First(), new HashSet<RoomIdentifier>());
    //            Adjacent[elevators[0].Rooms.First()].Add(elevators[1].Rooms.First());
    //            if (!Adjacent.ContainsKey(elevators[1].Rooms.First()))
    //                Adjacent.Add(elevators[1].Rooms.First(), new HashSet<RoomIdentifier>());
    //            Adjacent[elevators[1].Rooms.First()].Add(elevators[0].Rooms.First());
    //            return true;
    //        }
    //    }

    //    public static void PrintRoomAndDoorInfo()
    //    {
    //        ServerConsole.AddLog("Room Graph");
    //        foreach(var node in Adjacent)
    //        {
    //            ServerConsole.AddLog(node.Key.ApiRoom.Position.ToString() + node.Key.Zone + ", " + node.Key.Name + ", " + node.Key.Shape);
    //            foreach (var adj in node.Value)
    //            {
    //                ServerConsole.AddLog("adj   | " + adj.ApiRoom.Position.ToString() + adj.Zone + ", " + adj.Name + ", " + adj.Shape);
    //            }
    //        }

    //        ServerConsole.AddLog("Rooms apis");
    //        foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
    //        {
    //            FacilityRoom api_room = room.ApiRoom;
    //            ServerConsole.AddLog(api_room.Position.ToString() + room.Zone + ", " + room.Name + ", " + room.Shape);
    //            if (api_room.Lights == null)
    //            {
    //                ServerConsole.AddLog("lights are null");
    //            }
    //            else
    //            {
    //                ServerConsole.AddLog(api_room.Lights.LightColor.ToString());
    //            }
    //        }
    //    }

    //    public static void Enable()
    //    {
    //        update_handle = Timing.RunCoroutine(_Update());
    //    }

    //    public static void Disable()
    //    {
    //        Timing.KillCoroutines(update_handle);
    //    }

    //    public static void OpenAllRooms()
    //    {
    //        active_rooms.Clear();
    //        closing_rooms.Clear();

    //        IEnumerable<RoomIdentifier> all_rooms = AllRooms();
    //        foreach (RoomIdentifier room in all_rooms)
    //        {
    //            active_rooms.Add(room);
    //            FlickerableLightController lights = room.GetComponentInChildren<FlickerableLightController>();
    //            lights.WarheadLightOverride = false;
    //        }

    //        foreach(DoorVariant door in edge_doors)
    //            door.UnlockLater(0.0f, DoorLockReason.AdminCommand);

    //        foreach (ElevatorDoor door in lcz_elevators)
    //            door.UnlockLater(0.0f, DoorLockReason.AdminCommand);
    //    }

    //    public static void CloseAllRooms()
    //    {
    //        active_rooms.Clear();
    //        closing_rooms.Clear();

    //        IEnumerable<RoomIdentifier> all_rooms = AllRooms();
    //        foreach (RoomIdentifier room in all_rooms)
    //        {
    //            FlickerableLightController lights = room.GetComponentInChildren<FlickerableLightController>();
    //            lights.WarheadLightOverride = true;
    //        }

    //        foreach (DoorVariant door in edge_doors)
    //        {
    //            door.NetworkTargetState = false;
    //            door.ServerChangeLock(DoorLockReason.AdminCommand, true);
    //        }

    //        foreach (ElevatorDoor door in lcz_elevators)
    //            door.ServerChangeLock(DoorLockReason.AdminCommand, true);
    //    }

    //    public static void SetRooms(int count)
    //    {
    //        if (count < ValidSpawnRooms.Count())
    //            CloseRooms(ValidSpawnRooms.Count() - count);
    //        else if (count > ValidSpawnRooms.Count())
    //            OpenRooms(count - ValidSpawnRooms.Count());
    //    }

    //    public static void OpenRooms(int count)
    //    {
    //        if (ValidSpawnRooms.IsEmpty())
    //        {
    //            foreach(Player player in Player.GetPlayers())
    //            {
    //                if(ValidPlayerInRoom(player) && Adjacent.ContainsKey(player.Room))
    //                {
    //                    AddRoom(player.Room);
    //                    count--;
    //                    break;
    //                }
    //            }
    //            if (ValidSpawnRooms.IsEmpty())
    //            {
    //                AddRoom(RandomRoom());
    //                count--;
    //            }
    //        }

    //        for (int i = 0; i < count; i++)
    //        {
    //            HashSet<RoomIdentifier> candidate_rooms = new HashSet<RoomIdentifier>();
    //            foreach (RoomIdentifier room in ValidSpawnRooms)
    //                foreach (RoomIdentifier adj in Adjacent[room])
    //                    if (!ValidSpawnRooms.Contains(adj))
    //                        candidate_rooms.Add(adj);

    //            if (!candidate_rooms.IsEmpty())
    //            {
    //                System.Random random = new System.Random();
    //                AddRoom(candidate_rooms.ElementAt(random.Next(candidate_rooms.Count())));
    //            }
    //            else
    //                break;
    //        }
    //    }

    //    public static void CloseRooms(int count)
    //    {
    //        Dictionary<RoomIdentifier, bool> visited = new Dictionary<RoomIdentifier, bool>();
    //        foreach (RoomIdentifier room in active_rooms)
    //            visited.Add(room, false);

    //        List<RoomIdentifier> dfs_order = new List<RoomIdentifier>();

    //        Action<RoomIdentifier> DFS = null;
    //        DFS = (node) =>
    //        {
    //            visited[node] = true;
    //            dfs_order.Add(node);
    //            foreach (RoomIdentifier adj in Adjacent[node])
    //                if (active_rooms.Contains(adj) && !closing_rooms.ContainsKey(adj) && !visited[adj])
    //                    DFS(adj);
    //        };

    //        if (!ValidSpawnRooms.IsEmpty())
    //            DFS(ValidSpawnRooms.Last());

    //        count = Math.Min(count, dfs_order.Count);
    //        for (int i = 0; i < count; i++)
    //            MarkRoomForClosing(dfs_order[dfs_order.Count - (i + 1)]);
    //    }

    //    private static bool ValidPlayerInRoom(Player player)
    //    {
    //        return player.IsAlive && player.Room != null && Deathmatch.IsPlayerValid(player) && !Lobby.InSpawn(player);
    //    }


    //    private static IEnumerator<float> _Update()
    //    {
    //        const float delta = 1.0f;
    //        while (true)
    //        {
    //            try
    //            {
    //                //update closing rooms lights
    //                foreach (var pair in closing_rooms)
    //                {
    //                    FlickerableLightController lights = pair.Key.GetComponentInChildren<FlickerableLightController>();
    //                    float x = pair.Value / decontamination_warning;
    //                    Timing.CallPeriodically(delta, 1.0f / 4.0f, () =>
    //                    {
    //                        lights.WarheadLightColor = new Color(1.0f, x, 0.0f);
    //                        x -= (delta / decontamination_warning) * (1.0f / 4.0f);
    //                    });
    //                }

    //                //warn players inside rooms marked for closing
    //                foreach (Player player in Player.GetPlayers())
    //                {
    //                    if (ValidPlayerInRoom(player) && closing_rooms.ContainsKey(player.Room))
    //                    {
    //                        if (closing_rooms[player.Room] > 15.0f)
    //                            BroadcastOverride.BroadcastLine(player, 1, delta, BroadcastPriority.Low, "<color=#FFFF00>Caution! room decontamination in " + closing_rooms[player.Room].ToString("0") + "</color>");
    //                        else if (closing_rooms[player.Room] > 7.0f)
    //                            BroadcastOverride.BroadcastLine(player, 1, delta, BroadcastPriority.Medium, "<color=#FF8000>Warning! room decontamination in " + closing_rooms[player.Room].ToString("0") + "</color>");
    //                        else
    //                            BroadcastOverride.BroadcastLine(player, 1, delta, BroadcastPriority.High, "<color=#FF0000>DECONTAMINATION IMMINENT! " + closing_rooms[player.Room].ToString("0") + "</color>");
    //                        BroadcastOverride.UpdateIfDirty(player);
    //                    }
    //                }

    //                //decontaminate closed rooms
    //                IEnumerable<RoomIdentifier> closed = AllRooms().Except(active_rooms);
    //                foreach(Player player in Player.GetPlayers())
    //                {
    //                    if (ValidPlayerInRoom(player) && closed.Contains(player.Room))
    //                    {
    //                        BroadcastOverride.BroadcastLine(player, 1, delta, BroadcastPriority.High, "<color=#FF0000>DECONTAMINATNG!</color>");
    //                        BroadcastOverride.UpdateIfDirty(player);
    //                        player.EffectsManager.EnableEffect<Decontaminating>(1);
    //                    }
    //                }

    //                List<RoomIdentifier> removed = new List<RoomIdentifier>();
    //                foreach (RoomIdentifier key in closing_rooms.Keys.ToList())
    //                {
    //                    closing_rooms[key] -= delta;
    //                    if (closing_rooms[key] < 0.0f)
    //                    {
    //                        RemoveRoom(key);
    //                        removed.Add(key);
    //                    }
    //                }

    //                foreach(RoomIdentifier room in removed)
    //                {
    //                    FlickerableLightController lights = room.GetComponentInChildren<FlickerableLightController>();
    //                    lights.WarheadLightColor = new Color(1.0f, 0.0f, 0.0f);
    //                    UnmarkRoomForClosing(room);
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                ServerConsole.AddLog("Room Scale Error: " + ex.Message + " in " + ex.StackTrace, ConsoleColor.White);
    //            }

    //            yield return Timing.WaitForSeconds(delta);
    //        }
    //    }

    //    private static void MarkRoomForClosing(RoomIdentifier room)
    //    {
    //        closing_rooms.Add(room, (room.Zone == MapGeneration.FacilityZone.Surface ? decontamination_warning * 2.0f : decontamination_warning));
    //        FlickerableLightController lights = room.GetComponentInChildren<FlickerableLightController>();
    //        lights.WarheadLightColor = new Color(1.0f, 1.0f, 0.0f);
    //        lights.WarheadLightOverride = true;
    //    }

    //    private static void UnmarkRoomForClosing(RoomIdentifier room)
    //    {
    //        closing_rooms.Remove(room);
    //    }

    //    private static HashSet<ElevatorDoor> GetElevatorDoors(HashSet<DoorVariant> doors)
    //    {
    //        HashSet<ElevatorDoor> elevators = new HashSet<ElevatorDoor>();
    //        foreach (DoorVariant door in doors)
    //            if (door is ElevatorDoor elevator_door)
    //                elevators.Add(elevator_door);
    //        return elevators;
    //    }

    //    private static IEnumerable<ElevatorDoor[]> SharedElevators(HashSet<ElevatorDoor> room, HashSet<ElevatorDoor> adj)
    //    {
    //        return room.Join(adj,
    //                        (room_elevator) => room_elevator.Group,
    //                        (adj_elevator) => adj_elevator.Group,
    //                        (room_elevator, adj_elevator) => new ElevatorDoor[2] { room_elevator, adj_elevator });
    //    }

    //    private static void RemoveRoom(RoomIdentifier room)
    //    {
    //        HashSet<DoorVariant> room_doors = DoorVariant.DoorsByRoom[room];
    //        HashSet<ElevatorDoor> room_elevators = GetElevatorDoors(room_doors);

    //        //lock doors that are between the new room and previous active rooms
    //        foreach (RoomIdentifier adj in Adjacent[room])
    //        {
    //            if (active_rooms.Contains(adj))
    //            {
    //                HashSet<DoorVariant> adj_doors = DoorVariant.DoorsByRoom[adj];
    //                IEnumerable<DoorVariant> shared_doors = room_doors.Intersect(adj_doors);
    //                foreach (DoorVariant shared_door in shared_doors)
    //                {
    //                    shared_door.NetworkTargetState = false;
    //                    shared_door.ServerChangeLock(DoorLockReason.AdminCommand, true);
    //                }

    //                HashSet<ElevatorDoor> adj_elevators = GetElevatorDoors(adj_doors);
    //                IEnumerable<ElevatorDoor[]> shared_elevator_groups = SharedElevators(room_elevators, adj_elevators);
    //                foreach(ElevatorDoor[] group in shared_elevator_groups)
    //                {
    //                    group[0].ServerChangeLock(DoorLockReason.AdminCommand, true);
    //                    group[1].ServerChangeLock(DoorLockReason.AdminCommand, true);
    //                }
    //            }
    //        }

    //        active_rooms.Remove(room);
    //    }

    //    public static void AddRoom(RoomIdentifier room)
    //    {
    //        UnmarkRoomForClosing(room);
    //        FlickerableLightController lights = room.GetComponentInChildren<FlickerableLightController>();
    //        lights.WarheadLightOverride = false;

    //        //unlock unshared doors
    //        HashSet<DoorVariant> room_doors = DoorVariant.DoorsByRoom[room];
    //        HashSet<ElevatorDoor> room_elevators = GetElevatorDoors(room_doors);

    //        //unlock doors that are between the new room and previous active rooms
    //        foreach (RoomIdentifier adj in Adjacent[room])
    //        {
    //            if (active_rooms.Contains(adj))
    //            {
    //                HashSet<DoorVariant> adj_doors = DoorVariant.DoorsByRoom[adj];
    //                IEnumerable<DoorVariant> shared_doors = room_doors.Intersect(adj_doors);
    //                foreach (DoorVariant shared_door in shared_doors)
    //                    shared_door.UnlockLater(0.0f, DoorLockReason.AdminCommand);

    //                HashSet<ElevatorDoor> adj_elevators = GetElevatorDoors(adj_doors);
    //                IEnumerable<ElevatorDoor[]> shared_elevator_groups = SharedElevators(room_elevators, adj_elevators);
    //                foreach (ElevatorDoor[] group in shared_elevator_groups)
    //                {
    //                    group[0].UnlockLater(0.0f, DoorLockReason.AdminCommand);
    //                    group[1].UnlockLater(0.0f, DoorLockReason.AdminCommand);
    //                }
    //            }
    //        }
    //        active_rooms.Add(room);
    //    }

    //    private static IEnumerable<RoomIdentifier> AllRooms()
    //    {
    //        return RoomIdentifier.AllRoomIdentifiers.Where((r) => { return r.Zone != MapGeneration.FacilityZone.None && r.Zone != MapGeneration.FacilityZone.Surface && r.Zone != MapGeneration.FacilityZone.Other; });
    //    }

    //    private static RoomIdentifier RandomRoom()
    //    {
    //        System.Random random = new System.Random();
    //        IEnumerable<RoomIdentifier> rooms = AllRooms();
    //        return rooms.ElementAt(random.Next(rooms.Count()));
    //    }
    //}
}
