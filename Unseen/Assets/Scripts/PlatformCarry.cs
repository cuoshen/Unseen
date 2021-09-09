using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCarry : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {

        if (collision.transform.tag == "Player")
        {
            collision.collider.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        collision.collider.transform.SetParent(null);
    }
}
