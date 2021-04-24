using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jail
{
    class Room : MonoBehaviour
    {
        public List<GameObject> Doors;
        public float Length = 10f;

        private void Awake()
        {
            Doors = new List<GameObject>();
            foreach (Transform t in transform)
            {
                Doors.Add(t.gameObject);
            }
        }
    }
}
