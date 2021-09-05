using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainAffectLight : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Light l = other.GetComponent<Light>();
        if (l != null)
            l.intensity = 0;
    }
    void OnTriggerExit(Collider other)
    {
        Light l = other.GetComponent<Light>();
        if (l != null)
            l.intensity = 0.1f;
    }
}
