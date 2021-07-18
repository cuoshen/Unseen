using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Legacy
{
    class MainMenu : MonoBehaviour
    {
        public void OnGameStart()
        {
            GetComponent<AudioSource>().Play();
            SceneManager.LoadScene(1);
        }
    }
}
