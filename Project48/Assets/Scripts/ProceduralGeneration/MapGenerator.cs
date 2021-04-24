using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Jail
{
    class MapGenerator : MonoBehaviour
    {
        public GameObject[] Rooms;
        public struct MapDescription
        {
            public GameObject center;
            public List<GameObject> rooms;
            public GameObject startingPoint;
        }
        private MapDescription map;
        System.Random random;
        private const int maxRecursionDepth = 10;

        /// <summary>
        /// We generate map in a DFS-like fashion, start with a random room and append rooms recursively to generate our map
        /// </summary>
        /// <param name="seed"></param>
        public MapDescription GenerateMap()
        {
            map = new MapDescription();
            map.rooms = new List<GameObject>();
            GameObject centerCandidate = Rooms[random.Next(Rooms.Length)];
            map.center = GameObject.Instantiate(centerCandidate);
            AppendRooms(map.center, 0);
            return map;
        }

        private void AppendRooms(GameObject room, int depth)
        {
            if (depth >= maxRecursionDepth)
            {
                return;
            }
            // Parse out entrance
            Room roomData = room.GetComponent<Room>();
            if (roomData == null)
            {
                Debug.LogError("Try to instantiate a room without its data model");
                return;
            }
            foreach (GameObject door in roomData.Doors)
            {
                GameObject candidate = Rooms[random.Next(Rooms.Length)];
                // Check if we want to add a room to this door
                if (PercentageCheck(50))
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
                    }
                    else
                    {
                        Destroy(newRoom);
                    }
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

        private void Start()
        {
            random = new System.Random();
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
