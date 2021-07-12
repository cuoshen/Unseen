using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        print("rua");
        if (other.gameObject.tag == "Player")
        {
            MazeGenerator.Instance.NextLevel();
        }
    }
}
