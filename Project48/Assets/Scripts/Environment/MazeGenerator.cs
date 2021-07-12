using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using static Algorithms;

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

    [Header("General")]
    [SerializeField]
    Vector2Int initMapSize;
    [SerializeField]
    Transform startPrefab;
    [SerializeField]
    Transform endPrefab;
    [SerializeField]
    Transform connectorPrefab;

    [Header("Maze Variation Creation Chance")]
    [SerializeField]
    float caveChance;
    [SerializeField]
    float _2x2ColumnChance;

    [Header("Kiwi")]
    [SerializeField]
    float minKiwiSeparation;
    [SerializeField]
    Transform kiwiPrefab;

    [Header("Corridor Generation")]
    [SerializeField]
    Transform corridorWallPrefab;
    [SerializeField]
    Transform corridorFloorPrefab;
    [SerializeField]
    Transform corridorLightPrefab;

    [Header("Cave Generation")]
    [SerializeField]
    int caveCellInitChance;
    [SerializeField]
    int caveCellBirthLimit;
    [SerializeField]
    int caveCellDdeathLimit;
    [SerializeField]
    int cellularPassEpoch;
    [SerializeField]
    int minCaveLightSeparation;
    [SerializeField]
    Transform caveWallPrefab;
    [SerializeField]
    Transform caveLightPrefab;

    [Header("Columns Generation")]
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
        // UnityEngine.Random.InitState(33);
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
            if (UnityEngine.Random.Range(0f, 1f) < caveChance)
            {
                Vector2Int mapSize = (initMapSize + new Vector2Int(UnityEngine.Random.Range(-2, 0), UnityEngine.Random.Range(-2, 0))) * 10;
                GenerateCaveMaze(mapSize);
                //Vector2Int mapSize = initMapSize + new Vector2Int(UnityEngine.Random.Range(-2, 0), UnityEngine.Random.Range(-2, 0));
                //GenerateColumnarMaze(mapSize);
            }
            else
            {
                Vector2Int mapSize = initMapSize + new Vector2Int(UnityEngine.Random.Range(level, 2 + 2 * level), UnityEngine.Random.Range(level, 2 + 2 * level));
                GenerateCorridorMaze(mapSize);
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
    /// <param name="startCoord"> Local x-z coordinate of the entrance of current maze </param>
    /// <param name="endCoord"> Local x-z coordinate of the exit of current maze </param>
    Transform GenerateBaseMaze(Vector2Int mapSize, out Vector2Int startCoord, out Vector2Int endCoord, int boundary = 0)
    {
        startCoord = new Vector2Int(UnityEngine.Random.Range(boundary, mapSize.x - boundary), 0);
        endCoord = new Vector2Int(UnityEngine.Random.Range(boundary, mapSize.x - boundary), mapSize.y - 1);

        // Generate empty parent object
        GameObject mazeGO = new GameObject("Maze Room");
        Transform mazeTransform = mazeGO.transform;
        mazeTransform.SetParent(transform);
        mazeTransform.localPosition = new Vector3(mapSize.x / 2 - startCoord.x, 0, mapSize.y / 2 - startCoord.y) + lastEndPos;

        // Generate floor
        Transform floor = Instantiate(corridorFloorPrefab, mazeTransform);
        if (mapSize.x % 2 == 0)
            floor.localPosition += new Vector3(-0.5f, 0, 0);
        if (mapSize.y % 2 == 0)
            floor.localPosition += new Vector3(0, 0, -0.5f);
        floor.localScale = new Vector3(mapSize.x * 0.1f, 1, mapSize.y * 0.1f);

        // Generate walls with openings at startPos and endPos
        // UP
        for (int i = 0; i < mapSize.x; i++)
        {
            if (i != endCoord.x)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 + i, 0, -mapSize.y / 2 + mapSize.y - 1 + 0.5f);
                Vector3 angle = new Vector3(0, 0, 0);

                Transform newWall = Instantiate(corridorWallPrefab, mazeTransform);
                newWall.localPosition = position;
                newWall.localEulerAngles = angle;
            }
        }

        // LEFT
        for (int j = 0; j < mapSize.y; j++)
        {
            Vector3 position = new Vector3(-mapSize.x / 2 - 0.5f, 0, -mapSize.y / 2 + j);
            Vector3 angle = new Vector3(0, -90, 0);

            Transform newWall = Instantiate(corridorWallPrefab, mazeTransform);
            newWall.localPosition = position;
            newWall.localEulerAngles = angle;
        }

        // DOWN
        for (int i = 0; i < mapSize.x; i++)
        {
            if (i != startCoord.x)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 + i, 0, -mapSize.y / 2 - 0.5f);
                Vector3 angle = new Vector3(0, 180, 0);

                Transform newWall = Instantiate(corridorWallPrefab, mazeTransform);
                newWall.localPosition = position;
                newWall.localEulerAngles = angle;
            }
        }

        // RIGHT
        for (int j = 0; j < mapSize.y; j++)
        {
            Vector3 position = new Vector3(-mapSize.x / 2 + mapSize.x - 1 + 0.5f, 0, -mapSize.y / 2 + j);
            Vector3 angle = new Vector3(0, 90, 0);

            Transform newWall = Instantiate(corridorWallPrefab, mazeTransform);
            newWall.localPosition = position;
            newWall.localEulerAngles = angle;
        }

        lastEndPos += new Vector3(endCoord.x - startCoord.x, 0, endCoord.y - startCoord.y + 1);

        // Connector to next maze
        Transform connector = Instantiate(connectorPrefab, transform);
        connector.localPosition = lastEndPos;
        lastEndPos += new Vector3(0, 0, 1);

        return mazeTransform;
    }

    /// <summary>
    /// Generate caves by CA and connect them. Grid size is (0.5, 0.5).
    /// </summary>
    void GenerateCaveMaze(Vector2Int mapSize)
    {
        Transform mazeTransform = GenerateBaseMaze(mapSize / 2, out Vector2Int startPos, out Vector2Int endPos, 2);
        int[,][] setting = new int[mapSize.x, mapSize.y][];
        for (int x = 0; x < mapSize.x; x++)
            for (int y = 0; y < mapSize.y; y++)
            {
                setting[x,y] = new int[] { caveCellInitChance, caveCellBirthLimit, caveCellDdeathLimit };
            }
        int[,] maze = Cellular(mapSize, setting, cellularPassEpoch);
        maze = ConnectRegions(0, maze, 2, 8, 15, 20, 10000, false);

        // A straight tunnel from start to the first empty space it encounters
        Vector2Int tunnelFronStart = startPos * 2;
        while (CheckInMapRange(tunnelFronStart, mapSize) && maze[tunnelFronStart.x, tunnelFronStart.y] == 1 && maze[tunnelFronStart.x + 1, tunnelFronStart.y] == 1)
        {
            maze[tunnelFronStart.x, tunnelFronStart.y] = 0;
            maze[tunnelFronStart.x + 1, tunnelFronStart.y] = 0;
            tunnelFronStart += new Vector2Int(0, 1);
        }

        // A straight tunnel from end to the first empty space it encounters
        Vector2Int tunnelFronEnd = endPos * 2 + new Vector2Int(0, 1);
        while (CheckInMapRange(tunnelFronEnd, mapSize) && maze[tunnelFronEnd.x, tunnelFronEnd.y] == 1 && maze[tunnelFronEnd.x + 1, tunnelFronEnd.y] == 1)
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

        // Generate lights
        List<Region> regions = GetRegions(0, maze);
        List<Vector3> lightPositions = new List<Vector3>();
        foreach (DirectionalTile outline in regions[0].Outline)
        {
            Vector3 position = new Vector3(-mapSize.x / 4 + outline.Position.x / 2f - 0.25f, 0.5f, -mapSize.y / 4 + outline.Position.y / 2f - 0.25f)
                + Coord2PosXZ(Offset4[GetOpposite(outline.Direction)]) * 0.25f;
            if (CheckMinSeparation(lightPositions, position, minCaveLightSeparation))
            {
                Transform newLight = Instantiate(caveLightPrefab, mazeTransform);
                newLight.localPosition = position;
                newLight.localEulerAngles = Angle4[outline.Direction];
                lightPositions.Add(position);
            }
        }
    }

    /// <summary>
    /// Generate corridors by RB
    /// </summary>
    void GenerateCorridorMaze(Vector2Int mapSize)
    {
        Transform mazeTransform = GenerateBaseMaze(mapSize, out Vector2Int startCoord, out Vector2Int endCoord);
        Direction4[,] maze = RecursiveBacktracker(mapSize);
        List<Vector3> kiwiPositions = new List<Vector3>();

        for (int i = 0; i < mapSize.x; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 + i, 0, -mapSize.y / 2 + j);
                List<Vector3> wallPosList = new List<Vector3>();
                List<Vector3> wallAngleList = new List<Vector3>();

                // Generate walls
                foreach (Direction4 dir in Enum.GetValues(typeof(Direction4)))
                {
                    if (Offset4.ContainsKey(dir) && maze[i, j].HasFlag(dir))
                    {
                        Vector3 newPos = position + Coord2PosXZ(Offset4[dir]) * 0.5f;
                        Vector3 newAngle = Angle4[dir];
                        wallPosList.Add(newPos);
                        wallAngleList.Add(newAngle);

                        if (CheckInMapRange(new Vector2Int(i, j) + Offset4[dir], mapSize))
                        {
                            Transform newWall = Instantiate(corridorWallPrefab, mazeTransform);
                            newWall.localPosition = newPos;
                            newWall.localEulerAngles = newAngle;
                        }
                    }
                }

                // Generate lights
                if (wallPosList.Count != 0 && new Vector2Int(i, j) != startCoord && new Vector2Int(i, j) != endCoord)
                {
                    Transform newLight = Instantiate(corridorLightPrefab, mazeTransform);
                    int index = UnityEngine.Random.Range(0, wallPosList.Count);
                    newLight.localPosition = wallPosList[index] + new Vector3(0, 0.5f, 0);
                    newLight.localEulerAngles = wallAngleList[index];
                }

                //Generate enemies
                Vector2Int disFromStart = new Vector2Int(i, j) - startCoord;
                Vector2Int disFromEnd = new Vector2Int(i, j) - endCoord;
                if (wallPosList.Count == 3 && disFromStart.magnitude > minKiwiSeparation && disFromEnd.magnitude > minKiwiSeparation
                    && CheckMinSeparation(kiwiPositions, position, minKiwiSeparation))
                {
                    Transform newKiwi = Instantiate(kiwiPrefab, mazeTransform);
                    newKiwi.localPosition = position;
                    kiwiPositions.Add(position);
                }
            }
    }

    /// <summary>
    /// Generate columns. Grid size is (4, 4).
    /// </summary>
    void GenerateColumnarMaze(Vector2Int mapSize)
    {
        Transform mazeTransform = GenerateBaseMaze(mapSize * 4, out _, out _);
        int[,] maze = Columnar(mapSize, _2x2ColumnChance);

        for (int i = 0; i < mapSize.x; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                int col = maze[i, j];
                Vector3 position = new Vector3(-mapSize.x * 2 + 1.5f + i * 4, 0, -mapSize.y * 2 + 1.5f + j * 4);

                if (col == 1)
                {
                    Transform newWall = Instantiate(_1x1ColPrefab, mazeTransform);
                    Vector3 offset = new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), 0, UnityEngine.Random.Range(-1.5f, 1.5f));
                    newWall.localPosition = position + offset;
                }
                else if (col == 2)
                {
                    Transform newWall = Instantiate(_2x2ColPrefab, mazeTransform);
                    Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
                    newWall.localPosition = position + offset;
                }
            }
    }
    #endregion
}
