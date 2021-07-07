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

    Vector3 lastEndPos;
    GameObject player;
    int level = 0;

    [SerializeField]
    Vector2Int initMapSize;

    [SerializeField]
    float caveChance;
    [SerializeField]
    float _2x2ColumnChance;

    [SerializeField]
    int initChance;
    [SerializeField]
    int birthLimit;
    [SerializeField]
    int deathLimit;
    [SerializeField]
    int epoch;

    [SerializeField]
    float minKiwiSpawnDistance;

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
    [SerializeField]
    Transform caveWallPrefab;

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
        NextLevel();
    }

    void GenerateLevel()
    {
        lastEndPos = new Vector3(0, 0, 0);

        Transform start = Instantiate(startPrefab, transform);
        start.localPosition = lastEndPos + new Vector3(0, -0.5f, -1f);

        for (int i = 0; i < level; i++)
        {
            if (Random.Range(0f, 1f) < caveChance)
            {
                Vector2Int mapSize = (initMapSize + new Vector2Int(Random.Range(-2, 0), Random.Range(-2, 0))) * 10;
                GenerateCAMaze(mapSize);
                //Vector2Int mapSize = initMapSize + new Vector2Int(Random.Range(-2, 0), Random.Range(-2, 0));
                //GenerateColumnarMaze(mapSize);
            }
            else
            {
                Vector2Int mapSize = initMapSize + new Vector2Int(Random.Range(level, 2 + 2 * level), Random.Range(level, 2 + 2 * level));
                GenerateRBMaze(mapSize);
            }
        }

        Transform end = Instantiate(endPrefab, transform);
        end.localPosition = lastEndPos + new Vector3(0, 0.5f, 0);
    }

    IEnumerator NextLevelCoroutine()
    {
        player.transform.position = new Vector3(0, 0.5f, -0.3f);
        yield return null;

        level++;
        if (transform.childCount != 0)
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
        }
        GenerateLevel();

        yield return null;
        player.transform.position = new Vector3(0, 0.5f, -0.3f);
    }

    public void NextLevel()
    {
        StartCoroutine("NextLevelCoroutine");
    }

    #region Individual Maze Generators

    /// <summary>
    /// Generate an empty maze with floors and surrounding walls, its entrance connected to the exit of the previous maze
    /// </summary>
    /// <param name="mapSize"> Size of maze </param>
    /// <param name="boundary"> Boundary of start/end spawn </param>
    /// <param name="startPos"> Local position of the entrance of current maze </param>
    /// <param name="endPos"> Local position of the exit of current maze </param>
    Transform GenerateBaseMaze(Vector2Int mapSize, out Vector2Int startPos, out Vector2Int endPos, int boundary = 0)
    {
        startPos = new Vector2Int(Random.Range(boundary, mapSize.x - boundary), 0);
        endPos = new Vector2Int(Random.Range(boundary, mapSize.x - boundary), mapSize.y - 1);

        // Generate empty parent object
        GameObject mazeGO = new GameObject("Maze Room");
        Transform mazeTransform = mazeGO.transform;
        mazeTransform.SetParent(transform);
        mazeTransform.localPosition = new Vector3(mapSize.x / 2 - startPos.x, 0, mapSize.y / 2 - startPos.y) + lastEndPos;

        // Generate floor
        Transform floor = Instantiate(floorPrefab, mazeTransform);
        if (mapSize.x % 2 == 0)
            floor.localPosition += new Vector3(-0.5f, 0, 0);
        if (mapSize.y % 2 == 0)
            floor.localPosition += new Vector3(0, 0, -0.5f);
        floor.localScale = new Vector3(mapSize.x * 0.1f, 1, mapSize.y * 0.1f);

        // Generate walls with openings at startPos and endPos
        // UP
        for (int i = 0; i < mapSize.x; i++)
        {
            if (i != endPos.x)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 + i, 0, -mapSize.y / 2 + mapSize.y - 1 + 0.5f);
                Vector3 angle = new Vector3(0, 0, 0);

                Transform newWall = Instantiate(wallPrefab, mazeTransform);
                newWall.localPosition = position;
                newWall.localEulerAngles = angle;
            }
        }

        // LEFT
        for (int j = 0; j < mapSize.y; j++)
        {
            Vector3 position = new Vector3(-mapSize.x / 2 - 0.5f, 0, -mapSize.y / 2 + j);
            Vector3 angle = new Vector3(0, -90, 0);

            Transform newWall = Instantiate(wallPrefab, mazeTransform);
            newWall.localPosition = position;
            newWall.localEulerAngles = angle;
        }

        // DOWN
        for (int i = 0; i < mapSize.x; i++)
        {
            if (i != startPos.x)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 + i, 0, -mapSize.y / 2 - 0.5f);
                Vector3 angle = new Vector3(0, 180, 0);

                Transform newWall = Instantiate(wallPrefab, mazeTransform);
                newWall.localPosition = position;
                newWall.localEulerAngles = angle;
            }
        }

        // RIGHT
        for (int j = 0; j < mapSize.y; j++)
        {
            Vector3 position = new Vector3(-mapSize.x / 2 + mapSize.x - 1 + 0.5f, 0, -mapSize.y / 2 + j);
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

    void GenerateCAMaze(Vector2Int mapSize)
    {
        Transform mazeTransform = GenerateBaseMaze(mapSize / 2, out Vector2Int startPos, out Vector2Int endPos, 2);
        int[,][] setting = new int[mapSize.x, mapSize.y][];
        for (int x = 0; x < mapSize.x; x++)
            for (int y = 0; y < mapSize.y; y++)
            {
                setting[x,y] = new int[] { initChance, birthLimit, deathLimit };
            }
        int[,] maze = Algorithms.Cellular(mapSize, setting, epoch);

        Vector2Int tunnelFronStart = startPos * 2;
        while (Algorithms.IsInMapRange(tunnelFronStart, mapSize) && maze[tunnelFronStart.x, tunnelFronStart.y] == 1 && maze[tunnelFronStart.x + 1, tunnelFronStart.y] == 1)
        {
            maze[tunnelFronStart.x, tunnelFronStart.y] = 0;
            maze[tunnelFronStart.x + 1, tunnelFronStart.y] = 0;
            tunnelFronStart += new Vector2Int(0, 1);
        }

        Vector2Int tunnelFronEnd = endPos * 2 + new Vector2Int(0, 1);
        while (Algorithms.IsInMapRange(tunnelFronEnd, mapSize) && maze[tunnelFronEnd.x, tunnelFronEnd.y] == 1 && maze[tunnelFronEnd.x + 1, tunnelFronEnd.y] == 1)
        {
            maze[tunnelFronEnd.x, tunnelFronEnd.y] = 0;
            maze[tunnelFronEnd.x + 1, tunnelFronEnd.y] = 0;
            tunnelFronEnd += new Vector2Int(0, -1);
        }

        for (int i = 0; i < mapSize.x; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                if (maze[i, j] == 1)
                {
                    Transform newCube = Instantiate(caveWallPrefab, mazeTransform);
                    Vector3 position = new Vector3(-mapSize.x / 4 + i / 2f - 0.25f, 0, -mapSize.y / 4 + j / 2f - 0.25f);
                    newCube.localPosition = position;
                }
            }
    }

    void GenerateRBMaze(Vector2Int mapSize)
    {
        Transform mazeTransform = GenerateBaseMaze(mapSize, out Vector2Int startPos, out Vector2Int endPos);
        Direction4[,] maze = Algorithms.RecursiveBacktracker(mapSize);
        List<Transform> kiwis = new List<Transform>();

        for (int i = 0; i < mapSize.x; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                Direction4 cell = maze[i, j];
                Vector3 position = new Vector3(-mapSize.x / 2 + i, 0, -mapSize.y / 2 + j);
                List<Vector3> wallPosList = new List<Vector3>();
                List<Vector3> wallAngleList = new List<Vector3>();

                if (cell.HasFlag(Direction4.UP))
                {
                    Vector3 newPos = position + new Vector3(0, 0, 0.5f);
                    Vector3 newAngle = new Vector3(0, 0, 0);
                    wallPosList.Add(newPos);
                    wallAngleList.Add(newAngle);

                    if (j != mapSize.y - 1)
                    {
                        Transform newWall = Instantiate(wallPrefab, mazeTransform);
                        newWall.localPosition = newPos;
                        newWall.localEulerAngles = newAngle;
                    }
                }

                if (cell.HasFlag(Direction4.LEFT))
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

                if (cell.HasFlag(Direction4.DOWN))
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

                if (cell.HasFlag(Direction4.RIGHT))
                {
                    Vector3 newPos = position + new Vector3(0.5f, 0, 0);
                    Vector3 newAngle = new Vector3(0, 90, 0);
                    wallPosList.Add(newPos);
                    wallAngleList.Add(newAngle);

                    if (i != mapSize.x - 1)
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
                    int index = Random.Range(0, wallPosList.Count);
                    newLight.localPosition = wallPosList[index] + new Vector3(0, 0.5f, -0.05f);
                    newLight.localEulerAngles = wallAngleList[index];
                }

                Vector2Int disFromStart = new Vector2Int(i, j) - startPos;
                Vector2Int disFromEnd = new Vector2Int(i, j) - endPos;
                //Generate Enemies
                if (wallPosList.Count == 3 && disFromStart.magnitude > minKiwiSpawnDistance && disFromEnd.magnitude > minKiwiSpawnDistance)
                {
                    bool TooClose = false;
                    if(kiwis != null)
                    {
                        foreach(Transform kiwi in kiwis)
                        {
                            Vector3 disFromEnemy = kiwi.position - position;
                            if(disFromEnemy.magnitude < minKiwiSpawnDistance)
                            {
                                TooClose = true;
                            }
                        }
                    }
                    if (!TooClose)
                    {
                        Transform newKiwi = Instantiate(kiwiPrefab, mazeTransform);
                        newKiwi.localPosition = position;
                        kiwis.Add(newKiwi);
                    }
                }
            }
    }

    void GenerateColumnarMaze(Vector2Int mapSize)
    {
        Transform mazeTransform = GenerateBaseMaze(mapSize * 4, out _, out _);
        int[,] maze = Algorithms.Columnar(mapSize, _2x2ColumnChance);

        for (int i = 0; i < mapSize.x; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                int col = maze[i, j];
                Vector3 position = new Vector3(-mapSize.x * 2 + 1.5f + i * 4, 0, -mapSize.y * 2 + 1.5f + j * 4);

                if (col == 1)
                {
                    Transform newWall = Instantiate(_1x1ColPrefab, mazeTransform);
                    Vector3 offset = new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));
                    newWall.localPosition = position + offset;
                }
                else if (col == 2)
                {
                    Transform newWall = Instantiate(_2x2ColPrefab, mazeTransform);
                    Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                    newWall.localPosition = position + offset;
                }
            }
    }
    #endregion
}
