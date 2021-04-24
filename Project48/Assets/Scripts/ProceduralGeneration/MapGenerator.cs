using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jail
{
    class MapGenerator : MonoBehaviour
    {
        public GameObject spawnable;

        public void GenerateMap(int seed = 0)
        {
            GameObject.Instantiate(spawnable, Vector3.zero, Quaternion.identity);
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
