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
    Vector3 lastEndPos;
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
    [SerializeField]
    Transform _1x1ColPrefab;
    [SerializeField]
    Transform _2x2ColPrefab;

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

    void GenerateLevel()
    {
        lastEndPos = new Vector3(0, 0, 0);

        Transform start = Instantiate(startPrefab, transform);
        start.localPosition = lastEndPos + new Vector3(0, -0.5f, -1f);

        for (int i = 0; i < level; i++)
        {
            if (rng.Next(5) > 1)
            {
                int width = rng.Next(initWidth + level, initWidth + 2 + 2 * level);
                int height = rng.Next(initHeight + level, initHeight + 2 + 2 * level);
                GenerateRBMaze(width, height);
            }
            else
            {
                int width = rng.Next(initWidth - 2 + level, initWidth + level);
                int height = rng.Next(initHeight - 2 + level, initHeight + level);
                GenerateSparseMaze(width, height);
            }
        }

        Transform end = Instantiate(endPrefab, transform);
        end.localPosition = lastEndPos + new Vector3(0, 0.5f, 0);
    }

    IEnumerator NextLevelCoroutine()
    {
        level++;
        if (transform.childCount != 0)
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
        }

        GenerateLevel();
        yield return null;
        player.transform.position = new Vector3(0, 0.1f, 0);
    }

    public void NextLevel()
    {
        StartCoroutine("NextLevelCoroutine");
    }

    #region Individual Maze Generators

    /// <summary>
    /// Generate an empty maze with floors and surrounding walls, its entrance connected to the exit of the previous maze
    /// </summary>
    /// <param name="lastEndPos"> Global position of the exit of previous maze</param>
    /// <param name="startPos"> Local position of the entrance of current maze</param>
    /// <param name="endPos"> Local position of  the exit of current maze</param>
    /// <param name="width"> Width of maze
    /// <param name="height"> Height of maze
    Transform GenerateBaseMaze(Vector2Int startPos, Vector2Int endPos, int width, int height)
    {

        // Generate empty parent object
        GameObject mazeGO = new GameObject("Maze Room");
        Transform mazeTransform = mazeGO.transform;
        mazeTransform.SetParent(transform);
        mazeTransform.localPosition = new Vector3(width / 2 - startPos.x, 0, height / 2 - startPos.y) + lastEndPos;

        // Generate floor
        Transform floor = Instantiate(floorPrefab, mazeTransform);
        if (width % 2 == 0)
            floor.localPosition += new Vector3(-0.5f, 0, 0);
        if (height % 2 == 0)
            floor.localPosition += new Vector3(0, 0, -0.5f);
        floor.localScale = new Vector3(width * 0.1f, 1, height * 0.1f);

        // Generate walls with openings at startPos and endPos
        // UP
        for (int i = 0; i < width; i++)
        {
            if (i != endPos.x)
            {
                Vector3 position = new Vector3(-width / 2 + i, 0, -height / 2 + height - 1 + 0.5f);
                Vector3 angle = new Vector3(0, 0, 0);

                Transform newWall = Instantiate(wallPrefab, mazeTransform);
                newWall.localPosition = position;
                newWall.localEulerAngles = angle;
            }
        }

        // LEFT
        for (int j = 0; j < height; j++)
        {
            Vector3 position = new Vector3(-width / 2 - 0.5f, 0, -height / 2 + j);
            Vector3 angle = new Vector3(0, -90, 0);

            Transform newWall = Instantiate(wallPrefab, mazeTransform);
            newWall.localPosition = position;
            newWall.localEulerAngles = angle;
        }

        // DOWN
        for (int i = 0; i < width; i++)
        {
            if (i != startPos.x)
            {
                Vector3 position = new Vector3(-width / 2 + i, 0, -height / 2 - 0.5f);
                Vector3 angle = new Vector3(0, 180, 0);

                Transform newWall = Instantiate(wallPrefab, mazeTransform);
                newWall.localPosition = position;
                newWall.localEulerAngles = angle;
            }
        }

        // RIGHT
        for (int j = 0; j < height; j++)
        {
            Vector3 position = new Vector3(-width / 2 + width - 1 + 0.5f, 0, -height / 2 + j);
            Vector3 angle = new Vector3(0, 90, 0);

            Transform newWall = Instantiate(wallPrefab, mazeTransform);
            newWall.localPosition = position;
            newWall.localEulerAngles = angle;
        }

        // Connector to next maze
        lastEndPos += new Vector3(endPos.x - startPos.x, 0, endPos.y - startPos.y + 1);

        Transform connector = Instantiate(connectorPrefab, transform);
        connector.localPosition = lastEndPos;
        lastEndPos += new Vector3(0, 0, 1);

        return mazeTransform;
    }

    /// <summary>
    /// Generate an individual rectangular RB maze
    /// </summary>
    void GenerateRBMaze(int width, int height)
    {
        // Generate random start and end positions
        Vector2Int startPos = new Vector2Int(rng.Next(width), 0);
        Vector2Int endPos = new Vector2Int(rng.Next(width), height - 1);

        Transform mazeTransform = GenerateBaseMaze(startPos, endPos, width, height);
        WallState[,] maze = RBMazeMapper.CreateMap(rng, width, height);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                WallState cell = maze[i, j];
                Vector3 position = new Vector3(-width / 2 + i, 0, -height / 2 + j);
                List<Vector3> wallPosList = new List<Vector3>();
                List<Vector3> wallAngleList = new List<Vector3>();

                if (cell.HasFlag(WallState.UP))
                {
                    Vector3 newPos = position + new Vector3(0, 0, 0.5f);
                    Vector3 newAngle = new Vector3(0, 0, 0);
                    wallPosList.Add(newPos);
                    wallAngleList.Add(newAngle);

                    if (j != height - 1)
                    {
                        Transform newWall = Instantiate(wallPrefab, mazeTransform);
                        newWall.localPosition = newPos;
                        newWall.localEulerAngles = newAngle;
                    }
                }

                if (cell.HasFlag(WallState.LEFT))
                {
                    Vector3 newPos = position + new Vector3(-0.5f, 0, 0);
                    Vector3 newAngle = new Vector3(0, -90, 0);
                    wallPosList.Add(newPos);
                    wallAngleList.Add(newAngle);

                    if (i != 0)
                    {
                        Transform newWall = Instantiate(wallPrefab, mazeTransform);
                        newWall.localPosition = newPos;
                        newWall.localEulerAngles = newAngle;
                    }
                }

                if (cell.HasFlag(WallState.DOWN))
                {
                    Vector3 newPos = position + new Vector3(0, 0, -0.5f);
                    Vector3 newAngle = new Vector3(0, 180, 0);
                    wallPosList.Add(newPos);
                    wallAngleList.Add(newAngle);

                    if (j != 0)
                    {
                        Transform newWall = Instantiate(wallPrefab, mazeTransform);
                        newWall.localPosition = newPos;
                        newWall.localEulerAngles = newAngle;
                    }
                }

                if (cell.HasFlag(WallState.RIGHT))
                {
                    Vector3 newPos = position + new Vector3(0.5f, 0, 0);
                    Vector3 newAngle = new Vector3(0, 90, 0);
                    wallPosList.Add(newPos);
                    wallAngleList.Add(newAngle);

                    if (i != width - 1)
                    {
                        Transform newWall = Instantiate(wallPrefab, mazeTransform);
                        newWall.localPosition = newPos;
                        newWall.localEulerAngles = newAngle;
                    }
                }

                // Generate light
                if (wallPosList.Count != 0 && (i + j) % 3 == 0 && new Vector2Int(i, j) != startPos && new Vector2Int(i, j) != endPos)
                {
                    Transform newLight = Instantiate(lightPrefab, mazeTransform);
                    int index = rng.Next(wallPosList.Count);
                    newLight.localPosition = wallPosList[index] + new Vector3(0, 0.5f, -0.05f);
                    newLight.localEulerAngles = wallAngleList[index];
                    // Debug.Log(newLight.localPosition + ": " + cell);
                }

                Vector2Int disFromStart = new Vector2Int(i, j) - startPos;
                //Generate Enemies
                if (wallPosList.Count == 3 && disFromStart.magnitude > 3)
                {
                    Transform newKiwi = Instantiate(kiwiPrefab, mazeTransform);
                    newKiwi.localPosition = position;
                }
            }
        }
    }

    void GenerateSparseMaze(int width, int height)
    {
        // Generate random start and end positions
        Vector2Int startPos = new Vector2Int(rng.Next(width * 4), 0);
        Vector2Int endPos = new Vector2Int(rng.Next(width) * 4, height * 4 - 1);

        Transform mazeTransform = GenerateBaseMaze(startPos, endPos, width * 4, height * 4);
        ColumnSize[,] maze = SparseMazeMapper.CreateMap(rng, width, height);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ColumnSize col = maze[i, j];
                Vector3 position = new Vector3(-width * 2 + 1.5f + i * 4, 0, -height * 2 + 1.5f + j * 4);

                if (col == ColumnSize._1x1)
                {
                    Transform newWall = Instantiate(_1x1ColPrefab, mazeTransform);
                    Vector3 offset = new Vector3((float)rng.NextDouble(-1.5, 1.5), 0, (float)rng.NextDouble(-1.5, 1.5));
                    newWall.localPosition = position + offset;
                }
                else if (col == ColumnSize._2x2)
                {
                    Transform newWall = Instantiate(_2x2ColPrefab, mazeTransform);
                    Vector3 offset = new Vector3((float)rng.NextDouble(-0.5, 0.5), 0, (float)rng.NextDouble(-0.5, 0.5));
                    newWall.localPosition = position + offset;
                }
            }
        }
    }
    #endregion
}
