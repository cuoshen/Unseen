using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

    public void Lock()
    {
        transform.localEulerAngles = new Vector3(0, 0, 0);
        rb.freezeRotation = true;
    }
    public void Unlock()
    {
        transform.localEulerAngles = new Vector3(0, 0, 0);
        rb.freezeRotation = false;
    }
}
