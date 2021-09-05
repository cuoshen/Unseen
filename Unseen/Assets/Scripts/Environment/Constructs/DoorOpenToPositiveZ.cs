using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpenToPositiveZ : MonoBehaviour
{
    public Door door;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.tag == "Player")
            door.OpenPositive();
    }
}
