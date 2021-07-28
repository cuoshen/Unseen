using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockDoorRotation : MonoBehaviour
{
    void LateUpdate()
    {
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }
}
