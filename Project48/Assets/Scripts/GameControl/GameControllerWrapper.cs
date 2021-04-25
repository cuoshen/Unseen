﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jail
{
    class GameControllerWrapper : MonoBehaviour
    {
        public TopDownPlayerController Player;
        public GameObject WinCon;
        public MapGenerator MapGenerator;

        private GameController controller;

        private void Start()
        {
            controller = GameController.Instance;
            controller.Player = Player;
            controller.WinCon = WinCon;
            controller.MapGenerator = MapGenerator;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                controller.StartGame();
            }
        }
    }
}
