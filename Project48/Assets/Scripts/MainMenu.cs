using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jail
{
    class MainMenu : MonoBehaviour
    {
        public void OnGameStart()
        {
            SceneManager.LoadScene(1);
        }
    }
}
