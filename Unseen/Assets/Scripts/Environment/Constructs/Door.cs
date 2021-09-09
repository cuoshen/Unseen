using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isPlayerOnOpenToPositiveZ;
    public bool isPlayerOnOpenToNegativeZ;

    Animator animator;
    [SerializeField]
    bool isLocked;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Lock()
    {
        isLocked = true;
    }
    public void Unlock()
    {
        isLocked = false;
    }

    public void OpenPositive()
    {
        if (!isLocked)
        {
            Debug.Log("open+");
            animator.SetTrigger("OpenTo+veZ");
        }
    }

    public void OpenNegative()
    {
        if (!isLocked)
        {
            Debug.Log("open-");
            animator.SetTrigger("OpenTo-veZ");
        }
    }
}
