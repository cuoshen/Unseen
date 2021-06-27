using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Legacy
{
    class GameControllerWrapper : MonoBehaviour
    {
        public TopDownPlayerController Player;
        public GameObject Enemy;
        public GameObject WinCon;
        public MapGenerator MapGenerator;
        public AudioClip[] Musics;
        public Image BlackScreen;
        public GameObject CreditsScreen;
        public DelayedActivationController delayedActivationController;
        public GameObject InvisibleEnemy;

        private GameController controller;
        private bool gameStarted = false;

        private void Start()
        {
            Player.gameObject.SetActive(false);
            controller = GameController.Instance;
            controller.Player = Player;
            controller.WinConPrefab = WinCon;
            controller.MapGenerator = MapGenerator;
            controller.BlackScreen = BlackScreen;
            controller.FallSound = Musics[0];
            controller.EatenSound = Musics[1];
            controller.CreditsScreen = CreditsScreen;
            controller.delayedActivationController = delayedActivationController;
            controller.InvisibleEnemy = InvisibleEnemy;
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

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}
