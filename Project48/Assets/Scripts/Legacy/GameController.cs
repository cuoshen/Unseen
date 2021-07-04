﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Legacy
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
        public GameObject WinConPrefab { get; set; }
        public Image BlackScreen { get; set; }
        public AudioClip FallSound { get; set; }
        public AudioClip EatenSound { get; set; }
        public GameObject CreditsScreen { get; set; }
        public DelayedActivationController delayedActivationController { get; set; }
        public GameObject InvisibleEnemy { get; set; }

        private AudioSource persistentSound;
        private MapGenerator.MapDescription map;
        private NavMeshData navMeshData;
        private NavMeshDataInstance navMeshDataInstance;
        private List<GameObject> enemies = new List<GameObject>();

        public void StartGame()
        {
            map = MapGenerator.GenerateMap();
            Player.gameObject.SetActive(true);
            Player.enabled = true;
            Player.WinCon = GameObject.Instantiate(WinConPrefab, new Vector3(0, 2, 0), Quaternion.identity);
            Player.Character.enabled = false;
            Player.gameObject.transform.position = map.startingPoint.room.transform.position + new Vector3(0.0f, 1.0f, 0.0f);
            Player.Character.enabled = true;

            RebakeNavmesh();

            BlackScreen.gameObject.SetActive(false);
            CreditsScreen.SetActive(false);

            persistentSound = Camera.main.GetComponent<AudioSource>();
            if (!persistentSound.isPlaying)
            {
                persistentSound.Play();
            }
            Camera.main.GetComponent<CameraMovement>().DragToVantagePoint();

            enemies = SpawnInvisibleEnemies(5);
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
            for (int i = 0; i < enemies.Count; i++)
            {
                GameObject.Destroy(enemies[i]);
            }
        }

        public List<GameObject> SpawnInvisibleEnemies(int count)
        {
            List<GameObject> enemies = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-3.0f, 3.0f), 0, UnityEngine.Random.Range(-3.0f, 3.0f));
                GameObject room = map.rooms[UnityEngine.Random.Range(1, map.rooms.Count)];
                GameObject newSpawn = 
                    GameObject.Instantiate(InvisibleEnemy, room.transform.position + randomOffset, room.transform.rotation);
                enemies.Add(newSpawn);
            }
            return enemies;
        }

        private void RebakeNavmesh()
        {
            if (navMeshDataInstance.valid)
            {
                NavMesh.RemoveNavMeshData(navMeshDataInstance);
            }
            NavMeshBuildSettings settings = new NavMeshBuildSettings();
            settings.agentClimb = 0.75f;
            settings.agentHeight = 2.0f;
            settings.agentRadius = 0.5f;
            settings.agentSlope = 45.0f;
            List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
            List<Mesh> roomMeshes = new List<Mesh>();
            // Build sources
            foreach (GameObject roomObj in map.rooms)
            {
                NavMeshBuildSource src = new NavMeshBuildSource();
                src.transform = roomObj.transform.localToWorldMatrix;
                src.shape = NavMeshBuildSourceShape.Mesh;
                Mesh roomMesh = roomObj.GetComponent<MeshFilter>().mesh;
                roomMeshes.Add(roomMesh);
                src.sourceObject = roomMesh;
                src.size = new Vector3(10, 10, 10);
                sources.Add(src);
            }
            navMeshData = NavMeshBuilder.BuildNavMeshData(settings, sources, new Bounds(), Vector3.zero, Quaternion.identity);
            navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData);
            foreach (Mesh m in roomMeshes)
            {
                m.UploadMeshData(true);
            }
        }

        public void WinGame()
        {
            persistentSound.Stop();
            foreach (GameObject enemy in enemies)
            {
                enemy.GetComponent<AudioSource>().enabled = false;
            }
            Player.WinCon.GetComponent<AudioSource>().Play();
            Player.enabled = false;
            Debug.Log("You win the game");
            // Delay activasion credits screen
            delayedActivationController.DelayedActivation(3.0f);
        }

        public void PlayerFallToDeath()
        {
            HandlePlayerDeath();
            persistentSound.PlayOneShot(FallSound);
        }

        public void PlayerGotEaten()
        {
            HandlePlayerDeath();
            persistentSound.PlayOneShot(EatenSound);
        }

        private void HandlePlayerDeath()
        {
            Player.enabled = false;
            BlackScreen.gameObject.SetActive(true);
            persistentSound.Stop();
        }
    }
}