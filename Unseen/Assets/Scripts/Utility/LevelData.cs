using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Data", menuName = "Level Data", order = 1)]
public class LevelData : ScriptableObject, ISavedObject
{
    public int Level
    {
        get { return level; }
    }

    [SerializeField]
    private int level;

    public int IncrementLevel()
    {
        level++;
        Save();

        return Level;
    }

    public void Save()
    {
        string jsonData = JsonUtility.ToJson(this, true);
        PlayerPrefs.SetString(name, jsonData);
        PlayerPrefs.Save();
    }

    void OnEnable()
    {
        JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(name), this);
    }

    void OnDisable()
    {
        Save();
    }
}
