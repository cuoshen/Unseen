using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public AudioClip[] musics;
        private GameController controller;
        private bool gameStarted = false;
        private GameStates gamestate = GameStates.TOSTART;
        

        private void Start()
        {
            Player.gameObject.SetActive(false);
            controller = GameController.Instance;
            controller.Player = Player;
            controller.WinCon = WinCon;
            controller.MapGenerator = MapGenerator;
        }

        private void MusicControl()
        {

            switch (gamestate)
            {
                case GameStates.PROGRESS:
                    controller.changeBGM(musics[0]);
                    break;
                case GameStates.FAIL:
                    controller.ChangeBGM(musics[1]);
                    break;
                case GameStates.WIN:
                    controller.ChangeBGM(musics[2]);
                    break;
            }
        }
        


    private void Update()
        {
            if (!gameStarted)
            {
                controller.StartGame();
                gameStarted = true;
                gamestate = GameStates.PROGRESS;
            }
            MusicControl();
            if (Input.GetKeyDown(KeyCode.G))
            {
                Vector3 pos = GameObject.FindWithTag("Player").transform.position;
                controller.EnemySpawn(Enemy, pos);
            }





            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                controller.RestartGame();
            }
        }
    }
}
