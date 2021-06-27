using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

namespace Legacy
{
    class MapGenerator : MonoBehaviour
    {
        public GameObject[] RoomPrefabs;
        public GameObject[] PropPrefabs;
        public GameObject DoorPrefab;
        public struct MapDescription
        {
            public GameObject center;
            public List<GameObject> rooms;
            public RoomDepth startingPoint;
        }

        public struct RoomDepth
        {
            public RoomDepth(GameObject room, int depth)
            {
                this.room = room;
                this.depth = depth;
            }
            public GameObject room;
            public int depth;
        }

        private MapDescription map;
        System.Random random;
        private const int MAX_RECURSION_DEPTH = 20;
        /// <summary>
        /// We maintain a look-up table for the possiblity of room spawn with respect to center.
        /// In general, the further away we are from the center, the less possible we are to spawn a new room
        /// </summary>
        private int[] roomDensity;

        /// <summary>
        /// We generate map in a DFS-like fashion, start with a random room and append rooms recursively to generate our map
        /// </summary>
        public MapDescription GenerateMap()
        {
            map = new MapDescription();
            map.rooms = new List<GameObject>();
            GameObject centerCandidate = RoomPrefabs[random.Next(RoomPrefabs.Length)];
            map.center = GameObject.Instantiate(centerCandidate);
            map.rooms.Add(map.center);
            AppendRooms(map.center, 0);
            return map;
        }

        private void AppendRooms(GameObject room, int depth)
        {
            if (depth >= MAX_RECURSION_DEPTH)
            {
                return;
            }
            Room roomData = room.GetComponent<Room>();
            if (roomData == null)
            {
                Debug.LogError("Try to instantiate a room without its data model");
                return;
            }
            foreach (GameObject door in roomData.Doors)
            {
                GameObject candidate = RoomPrefabs[random.Next(RoomPrefabs.Length)];
                // Check if we want to add a room to this door
                if (PercentageCheck(roomDensity[depth]))
                {
                    // Try to add room at this door
                    GameObject newRoom = 
                        GameObject.Instantiate(candidate, 
                        door.transform.position + door.transform.forward * candidate.GetComponent<Room>().Length, 
                        door.transform.rotation);
                    // Bounding box check
                    if (CanBePlaced(newRoom))
                    {
                        SpawnProps(newRoom);
                        map.rooms.Add(newRoom);
                        if (depth >= map.startingPoint.depth)
                        {
                            map.startingPoint = new RoomDepth(newRoom, depth);
                        }
                        AppendRooms(newRoom, depth + 1);
                    }
                    else
                    {
                        Destroy(newRoom);
                    }
                }
                else
                {
                    // We seal off the empty entrance with a door prefab
                    //GameObject.Instantiate(DoorPrefab, door.transform.position, door.transform.rotation);
                }
            }
        }

        private bool PercentageCheck(int percentage)
        {
            return random.Next(100) < percentage;
        }

        private bool CanBePlaced(GameObject newRoom)
        {
            bool canBePlaced = true;
            BoxCollider newRoomCollider = newRoom.GetComponent<BoxCollider>();
            foreach (GameObject room in map.rooms)
            {
                Collider roomCollider = room.GetComponent<BoxCollider>();
                if (newRoomCollider.bounds.Intersects(roomCollider.bounds))
                {
                    canBePlaced = false;
                }
            }
            return canBePlaced;
        }

        private void SpawnProps(GameObject newRoom)
        {
            Room room = newRoom.GetComponent<Room>();
            foreach (GameObject spawnPoint in room.PropSpawns)
            {
                if (PercentageCheck(50))
                {
                    GameObject prop = PropPrefabs[random.Next(PropPrefabs.Length)];
                    GameObject.Instantiate(prop, spawnPoint.transform);
                }
            }
        }

        private int[] InitializeRoomDensity()
        {
            int[] density = new int[MAX_RECURSION_DEPTH];
            for (int i = 0; i < MAX_RECURSION_DEPTH; i++)
            {
                density[i] = 100 - 5 * i;
            }
            return density;
        }

        private void Start()
        {
            random = new System.Random();
            roomDensity = InitializeRoomDensity();
        }
    }
}
