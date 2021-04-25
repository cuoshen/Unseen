﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jail
{
    /// <summary>
    /// Singleton game controller logic
    /// </summary>
    class GameController
    {
        private static GameController instance;
        public static GameController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameController();
                }
                return instance;
            }
        }

        private GameController()
        {
        }

        public MapGenerator MapGenerator { get; set; }
        public TopDownPlayerController Player { get; set; }
        public GameObject WinCon { get; set; }
        private MapGenerator.MapDescription map;

        public void StartGame()
        {
            map = MapGenerator.GenerateMap();
            Player.WinCon = GameObject.Instantiate(WinCon, new Vector3(0, 2, 0), Quaternion.identity);
            // In the end we place player character in starting room
            Player.gameObject.SetActive(true);
            Player.Character.enabled = false;
            Player.gameObject.transform.position = map.startingPoint.transform.position + new Vector3(0.0f, 1.0f, 0.0f);
            Player.Character.enabled = true;
            RebakeNavmesh();
        }

        public void RestartGame()
        {
            CleanUp();
            StartGame();
        }

        private void CleanUp()
        {
            GameObject.Destroy(Player.WinCon);
            for (int i = 0; i < map.rooms.Count; i++)
            {
                GameObject.Destroy(map.rooms[i]);
            }
        }

        private void RebakeNavmesh()
        {

        }
    }
}
