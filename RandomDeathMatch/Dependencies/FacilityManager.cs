using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheRiptide
{
    public class FacilityManager
    {
        //caridinals in reference to 079s map not including up and down
        public enum Direction { North, East, South, West, Up, Down };

        //facility graph
        private static Dictionary<RoomIdentifier, Dictionary<RoomIdentifier, Direction>> room_adjacent_rooms = new Dictionary<RoomIdentifier, Dictionary<RoomIdentifier, Direction>>();
        private static HashSet<DoorVariant> edge_doors = new HashSet<DoorVariant>();
        private static HashSet<ElevatorDoor> edge_elevators = new HashSet<ElevatorDoor>();
        private static Dictionary<RoomIdentifier, HashSet<DoorVariant>> room_edges = new Dictionary<RoomIdentifier, HashSet<DoorVariant>>();

        //facility lights
        private static Dictionary<RoomIdentifier, RoomLightController> room_lights = new Dictionary<RoomIdentifier, RoomLightController>();

        public static void WaitingForPlayers()
        {
            Timing.CallDelayed(0.0f, () =>
            {
                try
                {
                    room_adjacent_rooms.Clear();
                    room_lights.Clear();
                    room_edges.Clear();

                    edge_doors.Clear();
                    edge_elevators.Clear();

                    foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
                    {
                        room_adjacent_rooms.Add(room, new Dictionary<RoomIdentifier, Direction>());
                        room_edges.Add(room, new HashSet<DoorVariant>());
                        room_lights.Add(room, room.GetComponentInChildren<RoomLightController>());
                    }

                    foreach (DoorVariant door in DoorVariant.AllDoors)
                    {
                        if (door.Rooms.Length == 2)
                        {
                            edge_doors.Add(door);
                            AddAdjacentRoom(door.Rooms);
                            room_edges[door.Rooms[0]].Add(door);
                            room_edges[door.Rooms[1]].Add(door);
                        }
                    }

                    foreach (List<ElevatorDoor> elevators in ElevatorDoor.AllElevatorDoors.Values)
                    {
                        if (elevators.Count() == 2 && elevators[0].Rooms.First() != elevators[1].Rooms.First())
                        {
                            edge_elevators.UnionWith(elevators);
                            AddAdjacentRoom(new RoomIdentifier[] { elevators[0].Rooms.First(), elevators[1].Rooms.First() });
                            room_edges[elevators[0].Rooms.First()].UnionWith(elevators);
                            room_edges[elevators[1].Rooms.First()].UnionWith(elevators);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            });
        }

        public static void RoundRestart()
        {
            room_adjacent_rooms.Clear();
            room_lights.Clear();
            room_edges.Clear();

            edge_doors.Clear();
            edge_elevators.Clear();
        }

        public static void LockAllRooms(DoorLockReason reason)
        {
            foreach (var door in edge_doors)
                LockDoor(door, reason);
            foreach (var elevator in edge_elevators)
                LockDoor(elevator, reason);
        }

        public static void UnlockAllRooms(DoorLockReason reason)
        {
            foreach (var door in edge_doors)
                UnlockDoor(door, reason);
            foreach (var elevator in edge_elevators)
                UnlockDoor(elevator, reason);
        }

        //lock all egde room doors/elevators
        public static void LockRoom(RoomIdentifier room, DoorLockReason reason)
        {
            foreach (var door in room_edges[room])
                LockDoor(door, reason);
        }

        //unlock all egde room doors/elevators
        public static void UnlockRoom(RoomIdentifier room, DoorLockReason reason)
        {
            foreach (var door in room_edges[room])
                UnlockDoor(door, reason);
        }

        //lock all egde room doors/elevators
        public static void LockRooms(HashSet<RoomIdentifier> rooms, DoorLockReason reason)
        {
            HashSet<DoorVariant> doors = new HashSet<DoorVariant>();
            foreach (var room in rooms)
                doors.UnionWith(room_edges[room]);
            foreach (var door in doors)
                LockDoor(door, reason);
        }

        //unlock all egde room doors/elevators
        public static void UnlockRooms(HashSet<RoomIdentifier> rooms, DoorLockReason reason)
        {
            HashSet<DoorVariant> doors = new HashSet<DoorVariant>();
            foreach (var room in rooms)
                doors.UnionWith(room_edges[room]);
            foreach (var door in doors)
                UnlockDoor(door, reason);
        }

        //lock all edge room doors shared between rooms/elevators
        public static void LockJoinedRooms(HashSet<RoomIdentifier> rooms, DoorLockReason reason)
        {
            HashSet<DoorVariant> joint = JointDoors(rooms, room_edges);
            foreach (var door in joint)
                LockDoor(door, reason);
        }

        //unlock all edge room doors shared between rooms/elevators
        public static void UnlockJoinedRooms(HashSet<RoomIdentifier> rooms, DoorLockReason reason)
        {
            HashSet<DoorVariant> joint = JointDoors(rooms, room_edges);
            foreach (var door in joint)
                UnlockDoor(door, reason);
        }

        public static void CloseAllRooms()
        {
            foreach (var door in edge_doors)
                CloseDoor(door);
        }

        public static void OpenAllRooms()
        {
            foreach (var door in edge_doors)
                OpenDoor(door);
        }

        //close all egde room doors/elevators
        public static void CloseRoom(RoomIdentifier room)
        {
            foreach (var door in DoorVariant.DoorsByRoom[room])
                if (door.Rooms.Count() == 2)
                    CloseDoor(door);
        }

        //open all egde room doors/elevators
        public static void OpenRoom(RoomIdentifier room)
        {
            foreach (var door in DoorVariant.DoorsByRoom[room])
                if (door.Rooms.Count() == 2)
                    OpenDoor(door);
        }

        //close all egde room doors/elevators
        public static void CloseRooms(HashSet<RoomIdentifier> rooms)
        {
            HashSet<DoorVariant> doors = new HashSet<DoorVariant>();
            foreach (var room in rooms)
                doors.UnionWith(DoorVariant.DoorsByRoom[room]);
            foreach (var door in doors)
                if (door.Rooms.Count() == 2)
                    CloseDoor(door);
        }

        //open all egde room doors/elevators
        public static void OpenRooms(HashSet<RoomIdentifier> rooms)
        {
            HashSet<DoorVariant> doors = new HashSet<DoorVariant>();
            foreach (var room in rooms)
                doors.UnionWith(DoorVariant.DoorsByRoom[room]);
            foreach (var door in doors)
                if (door.Rooms.Count() == 2)
                    OpenDoor(door);
        }

        //close all edge room doors shared between rooms/elevators
        public static void CloseJoinedRooms(HashSet<RoomIdentifier> rooms)
        {
            HashSet<DoorVariant> joint = JointDoors(rooms, DoorVariant.DoorsByRoom);
            foreach (var door in joint)
                CloseDoor(door);
        }

        //open all edge room doors shared between rooms/elevators
        public static void OpenJoinedRooms(HashSet<RoomIdentifier> rooms)
        {
            HashSet<DoorVariant> joint = JointDoors(rooms, DoorVariant.DoorsByRoom);
            foreach (var door in joint)
                OpenDoor(door);
        }

        public static Dictionary<RoomIdentifier, Direction> GetAdjacent(RoomIdentifier room)
        {
            return room_adjacent_rooms[room];
        }

        //lights
        public static void ResetAllRoomLights()
        {
            foreach (var controller in room_lights.Values)
            {
                controller.NetworkOverrideColor = Color.white;
                controller.NetworkLightsEnabled = true;
            }
        }

        public static void SetAllRoomLightColors(Color color)
        {
            foreach (var controller in room_lights.Values)
                controller.NetworkOverrideColor = color;
        }

        public static void SetAllRoomLightStates(bool is_enabled)
        {
            foreach (var controller in room_lights.Values)
                controller.NetworkLightsEnabled = is_enabled;
        }

        public static void ResetRoomLight(RoomIdentifier room)
        {
            room_lights[room].NetworkOverrideColor = Color.white;
            room_lights[room].NetworkLightsEnabled = true;
        }

        public static void SetRoomLightColor(RoomIdentifier room, Color color)
        {
            room_lights[room].NetworkOverrideColor = color;
        }

        //turn lights on or off
        public static void SetRoomLightState(RoomIdentifier room, bool is_enabled)
        {
            room_lights[room].NetworkLightsEnabled = is_enabled;
        }

        private static HashSet<DoorVariant> JointDoors(HashSet<RoomIdentifier> rooms, Dictionary<RoomIdentifier, HashSet<DoorVariant>> dict)
        {
            Dictionary<DoorVariant, int> door_counts = new Dictionary<DoorVariant, int>();
            foreach (var room in rooms)
            {
                foreach (var door in dict[room])
                {
                    if (door_counts.ContainsKey(door))
                        door_counts[door]++;
                    else
                        door_counts.Add(door, 1);
                }
            }
            HashSet<DoorVariant> result = new HashSet<DoorVariant>();
            foreach (var door_count in door_counts)
                if (door_count.Value != 1)
                    result.Add(door_count.Key);
            return result;
        }

        private static void LockDoor(DoorVariant door, DoorLockReason reason)
        {
            door.ServerChangeLock(reason, true);
        }

        private static void UnlockDoor(DoorVariant door, DoorLockReason reason)
        {
            door.UnlockLater(0.0f, reason);
        }

        private static void OpenDoor(DoorVariant door)
        {
            door.NetworkTargetState = true;
        }

        private static void CloseDoor(DoorVariant door)
        {
            door.NetworkTargetState = false;
        }

        private static void AddAdjacentRoom(RoomIdentifier[] rooms)
        {
            if (rooms[0] != rooms[1])
            {
                Direction dir = RoomDirection(rooms[0], rooms[1]);
                if (!room_adjacent_rooms[rooms[0]].ContainsKey(rooms[1]))
                    room_adjacent_rooms[rooms[0]].Add(rooms[1], dir);

                dir = RoomDirection(rooms[1], rooms[0]);
                if (!room_adjacent_rooms[rooms[1]].ContainsKey(rooms[0]))
                    room_adjacent_rooms[rooms[1]].Add(rooms[0], dir);
            }
        }

        private static Direction RoomDirection(RoomIdentifier from, RoomIdentifier to)
        {
            Vector3 difference = from.ApiRoom.Position - to.ApiRoom.Position;
            Vector3 abs = new Vector3(Math.Abs(difference.x), Math.Abs(difference.y), Math.Abs(difference.z));
            if (abs.x > abs.y && abs.x > abs.z)
            {
                if (difference.x > 0)
                    return Direction.North;
                else
                    return Direction.South;
            }
            else if (abs.z > abs.x && abs.z > abs.y)
            {
                if (difference.z > 0)
                    return Direction.East;
                else
                    return Direction.West;
            }
            else
            {
                if (difference.y > 0)
                    return Direction.Up;
                else
                    return Direction.Down;
            }
        }
    }
}
