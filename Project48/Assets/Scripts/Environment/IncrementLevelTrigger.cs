using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncrementLevelTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        MazeGenerator.Instance.levelCounter.IncrementLevel();
    }
}
