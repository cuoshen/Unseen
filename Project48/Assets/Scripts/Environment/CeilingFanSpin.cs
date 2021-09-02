using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeilingFanSpin : MonoBehaviour
{
    public float spinSpeed;
    void Update()
    {
        transform.localEulerAngles += new Vector3(0, spinSpeed, 0);
    }
}
