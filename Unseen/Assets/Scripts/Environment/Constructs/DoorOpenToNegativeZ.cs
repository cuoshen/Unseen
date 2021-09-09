using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpenToNegativeZ : MonoBehaviour
{
    public Door door;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.tag == "Player")
            door.OpenNegative();
    }
}
