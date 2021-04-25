using System;
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
        private bool gameStarted = false;

        private void Start()
        {
            Player.gameObject.SetActive(false);
            controller = GameController.Instance;
            controller.Player = Player;
            controller.WinCon = WinCon;
            controller.MapGenerator = MapGenerator;
        }

        private void Update()
        {
            if (!gameStarted)
            {
                controller.StartGame();
                gameStarted = true;
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                controller.RestartGame();
            }
        }
    }
}
