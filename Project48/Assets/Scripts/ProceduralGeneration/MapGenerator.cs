using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Jail
{
    class MapGenerator : MonoBehaviour
    {
        public GameObject[] RoomPrefabs;
        public GameObject DoorPrefab;
        public TopDownPlayerController Player;
        public struct MapDescription
        {
            public GameObject center;
            public List<GameObject> rooms;
            public GameObject startingPoint;
        }

        private MapDescription map;
        System.Random random;
        private const int maxRecursionDepth = 20;
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
            AppendRooms(map.center, 0);
            // In the end we place player character in starting room
            Player.gameObject.transform.position = map.startingPoint.transform.position + new Vector3(0.0f, 1.0f, 0.0f);
            return map;
        }

        private void AppendRooms(GameObject room, int depth)
        {
            if (depth >= maxRecursionDepth)
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
                        map.rooms.Add(newRoom);
                        AppendRooms(newRoom, depth + 1);
                        map.startingPoint = newRoom;
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
            Collider newRoomCollider = newRoom.GetComponent<Collider>();
            foreach (GameObject room in map.rooms)
            {
                Collider roomCollider = room.GetComponent<Collider>();
                if (newRoomCollider.bounds.Intersects(roomCollider.bounds))
                {
                    canBePlaced = false;
                }
            }
            return canBePlaced;
        }

        private int[] InitializeRoomDensity()
        {
            int[] density = new int[maxRecursionDepth];
            for (int i = 0; i < maxRecursionDepth; i++)
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GenerateMap();
            }
        }
    }
}
