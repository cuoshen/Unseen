using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jail
{
    class Room : MonoBehaviour
    {
        public GameObject DoorsDirectory;
        public List<GameObject> Doors { get; private set; }
        public float Length = 10f;
        public Collider RoomCollider { get; private set; }

        private void Awake()
        {
            Doors = new List<GameObject>();
            foreach (Transform t in DoorsDirectory.transform)
            {
                Doors.Add(t.gameObject);
            }
            RoomCollider = gameObject.GetComponent<Collider>();
        }
    }
}
