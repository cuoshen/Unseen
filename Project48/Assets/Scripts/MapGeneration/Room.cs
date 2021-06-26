using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jail
{
    class Room : MonoBehaviour
    {
        public GameObject DoorsDirectory;
        public GameObject PropSpawnDirectory;
        public List<GameObject> Doors { get; private set; }
        public List<GameObject> PropSpawns { get; private set; }
        public float Length = 10f;
        public Collider RoomCollider { get; private set; }

        private void Awake()
        {
            Doors = new List<GameObject>();
            foreach (Transform t in DoorsDirectory.transform)
            {
                Doors.Add(t.gameObject);
            }
            PropSpawns = new List<GameObject>();
            if (PropSpawnDirectory != null)
            {
                foreach (Transform t in PropSpawnDirectory.transform)
                {
                    PropSpawns.Add(t.gameObject);
                }
            }
            RoomCollider = gameObject.GetComponent<Collider>();
        }
    }
}
