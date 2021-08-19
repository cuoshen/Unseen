using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TrainShakeScreen : MonoBehaviour
{
    void ShakeScreen()
    {
        GetComponentInChildren<CinemachineImpulseSource>().GenerateImpulse();
    }
}
