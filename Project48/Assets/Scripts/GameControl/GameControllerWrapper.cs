using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Jail
{

    public enum GameStates
    {
        WIN,
        FAIL,
        PROGRESS,
        TOSTART
    };
    class GameControllerWrapper : MonoBehaviour
    {
        public TopDownPlayerController Player;
        public GameObject Enemy;
        public GameObject WinCon;
        public MapGenerator MapGenerator;
        public AudioClip[] Musics;
        public Image BlackScreen;

        private GameController controller;
        private bool gameStarted = false;
        private GameStates gamestate = GameStates.TOSTART;
        

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
        }

        private void Update()
        {
            if (!gameStarted)
            {
                controller.StartGame();
                gameStarted = true;
                gamestate = GameStates.PROGRESS;
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                Vector3 pos = GameObject.FindWithTag("Player").transform.position;
                controller.SpawnEnemy(Enemy, pos);
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
