using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    protected static MazeGenerator s_Instance;
    public static MazeGenerator Instance
    {
        get
        {
            if (s_Instance != null)
                return s_Instance;
            s_Instance = FindObjectOfType<MazeGenerator>();

            return s_Instance;
        }
    }

    System.Random rng;
    int initWidth = 5;
    int initHeight = 5;
    int level = 0;
    GameObject player;

    [SerializeField]
    Transform wallPrefab;
    [SerializeField]
    Transform floorPrefab;
    [SerializeField]
    Transform lightPrefab;
    [SerializeField]
    Transform startPrefab;
    [SerializeField]
    Transform endPrefab;
    [SerializeField]
    Transform connectorPrefab;
    [SerializeField]
    Transform kiwiPrefab;

    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        s_Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rng = new System.Random(/*seed*/);
        NextLevel();
    }

    void GenerateLevel(int width, int height)
    {
        Vector3 lastEndPos = new Vector3(0, 0, 0);

        Transform start = Instantiate(startPrefab, transform);
        start.localPosition = lastEndPos + new Vector3(0, -0.5f, -1f);

        for (int i = 0; i < level; i++)
        {
            WallState[,] maze = RBMazeMapper.CreateMap(rng, width, height);

            Vector2Int startPos = new Vector2Int(rng.Next(width), 0);
            Vector2Int endPos = new Vector2Int(rng.Next(width), height - 1);

            GenerateRBMaze(lastEndPos, startPos, endPos, maze);
            lastEndPos += new Vector3(endPos.x - startPos.x, 0, endPos.y - startPos.y + 1);

            Transform connector = Instantiate(connectorPrefab, transform);
            connector.localPosition = lastEndPos;
            lastEndPos += new Vector3(0, 0, 1);
        }

        Transform end = Instantiate(endPrefab, transform);
        end.localPosition = lastEndPos + new Vector3(0, 0.5f, 0);
    }

    /// <summary>
    /// Generate an individual rectangular RB maze, its entrance connected to the exit of the previous maze
    /// </summary>
    /// <param name="lastEndPos"> Global position of the exit of previous maze</param>
    /// <param name="startPos"> Local position of the entrance of current maze</param>
    /// <param name="endPos"> Local position of  the exit of current maze</param>
    /// <param name="maze"> Matrix representing current maze</param>
    void GenerateRBMaze(Vector3 lastEndPos, Vector2Int startPos, Vector2Int endPos, WallState[,] maze)
    {
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);

        GameObject mazeGO = new GameObject("RB Maze");
        Transform mazeTransform = mazeGO.transform;
        mazeTransform.SetParent(transform);
        mazeTransform.localPosition = new Vector3(width / 2 - startPos.x, 0, height / 2 - startPos.y) + lastEndPos;

        Transform floor = Instantiate(floorPrefab, mazeTransform);
        if (width % 2 == 0)
            floor.localPosition += new Vector3(-0.5f, 0, 0);
        if (height % 2 == 0)
            floor.localPosition += new Vector3(0, 0, -0.5f);
        floor.localScale = new Vector3(width*0.1f, 1, height*0.1f);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                WallState cell = maze[i, j];
                List<Vector3> wallPosList = new List<Vector3>();
                List<Vector3> wallAngleList = new List<Vector3>();
                Vector3 position = new Vector3(-width / 2 + i, 0, -height / 2 + j);

                if (cell.HasFlag(WallState.UP) && new Vector2Int(i,j) != endPos)
                {
                    Vector3 newPos = position + new Vector3(0, 0.5f, 0.5f);
                    Vector3 newAngle = new Vector3(0, 0, 0);
                    wallPosList.Add(newPos);
                    wallAngleList.Add(newAngle);

                    Transform newWall = Instantiate(wallPrefab, mazeTransform);
                    newWall.localPosition = newPos;
                    newWall.localEulerAngles = newAngle;
                }

                if (cell.HasFlag(WallState.LEFT))
                {
                    Vector3 newPos = position + new Vector3(-0.5f, 0.5f, 0);
                    Vector3 newAngle = new Vector3(0, -90, 0);
                    wallPosList.Add(newPos);
                    wallAngleList.Add(newAngle);

                    Transform newWall = Instantiate(wallPrefab, mazeTransform);
                    newWall.localPosition = newPos;
                    newWall.localEulerAngles = newAngle;
                }

                if (cell.HasFlag(WallState.DOWN) && new Vector2Int(i, j) != startPos)
                {
                    Vector3 newPos = position + new Vector3(0, 0.5f, -0.5f);
                    Vector3 newAngle = new Vector3(0, 180, 0);
                    wallPosList.Add(newPos);
                    wallAngleList.Add(newAngle);

                    if (j == 0)
                    {
                        Transform newWall = Instantiate(wallPrefab, mazeTransform);
                        newWall.localPosition = newPos;
                        newWall.localEulerAngles = newAngle;
                    }
                }

                if (cell.HasFlag(WallState.RIGHT))
                {
                    Vector3 newPos = position + new Vector3(0.5f, 0.5f, 0);
                    Vector3 newAngle = new Vector3(0, 90, 0);
                    wallPosList.Add(newPos);
                    wallAngleList.Add(newAngle);

                    if (i == width - 1)
                    {
                        Transform newWall = Instantiate(wallPrefab, mazeTransform);
                        newWall.localPosition = newPos;
                        newWall.localEulerAngles = newAngle;
                    }
                }

                if (wallPosList.Count != 0 && (i + j) % 3 == 0)
                {
                    Transform newLight = Instantiate(lightPrefab, mazeTransform);
                    int index = rng.Next(wallPosList.Count);
                    newLight.localPosition = wallPosList[index] + new Vector3(0, 0, -0.05f);
                    newLight.localEulerAngles = wallAngleList[index];
                    // Debug.Log(newLight.localPosition + ": " + cell);
                }

                if (wallPosList.Count == 3)
                {
                    Transform newKiwi = Instantiate(kiwiPrefab, mazeTransform);
                    newKiwi.localPosition = position;
                }
            }
        }
    }

    IEnumerator NextLevelCoroutine()
    {
        level++;
        if (transform.childCount != 0)
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
        }

        int width = rng.Next(initWidth + level, initWidth + 2 + 2 * level);
        int height = rng.Next(initHeight + level, initHeight + 2 + 2 * level);

        GenerateLevel(width, height);
        yield return null;
        player.transform.position = new Vector3(0, 0, 0);
    }

    public void NextLevel()
    {
        StartCoroutine("NextLevelCoroutine");
    }
}
