using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Listener : MonoBehaviour
{
    protected static Listener s_Instance;
    public static Listener Instance
    {
        get
        {
            if (s_Instance != null)
                return s_Instance;
            s_Instance = FindObjectOfType<Listener>();

            return s_Instance;
        }
    }

    GameObject player;

    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        s_Instance = this;
        DontDestroyOnLoad(this);
    }

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        else
        {
            transform.position = player.transform.position;
        }
    }
}
