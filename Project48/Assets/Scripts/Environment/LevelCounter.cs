using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCounter : MonoBehaviour
{
    protected static LevelCounter s_Instance;
    public static LevelCounter Instance
    {
        get
        {
            if (s_Instance != null)
                return s_Instance;
            s_Instance = FindObjectOfType<LevelCounter>();

            return s_Instance;
        }
    }

    [SerializeField]
    private int level;
    public int Level
    {
        get { return level; }
        set { level = value; }
    }

    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        s_Instance = this;
        DontDestroyOnLoad(this);

        Level = 0;
    }

    public int IncrementLevel()
    {
        Level++;
        return Level;
    }
}
