using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using static Algorithms;

public delegate bool CoordCriteria(Vector2Int coord);

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

    #region General Parameters
    [Header("General")]
    [SerializeField]
    Vector2Int initMapSize;
    [SerializeField]
    Transform startPrefab;
    [SerializeField]
    Transform endPrefab;
    [SerializeField]
    Transform connectorPrefab;
    #endregion
    #region Maze Variation Creation Chance
    [Header("Maze Variation Creation Chance")]
    [SerializeField]
    float caveChance;
    [SerializeField]
    float _2x2ColumnChance;
    #endregion
    #region Kiwi Parameters
    [Header("Kiwi")]
    [SerializeField]
    float minKiwiSeparation;
    [SerializeField]
    Transform kiwiPrefab;
    #endregion
    #region Corridor Generation Parameters
    [Header("Corridor Generation")]
    [SerializeField]
    Transform corridorWallPrefab;
    [SerializeField]
    Transform corridorFloorPrefab;
    [SerializeField]
    Transform corridorLightPrefab;
    #endregion
    #region Cave Generation Parameters
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
    #endregion
    #region Columns Generation Parameters
    [Header("Columns Generation")]
    [SerializeField]
    Transform _1x1ColPrefab;
    [SerializeField]
    Transform _2x2ColPrefab;
    #endregion

    [SerializeField]
    Transform cubePrefab;

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
        UnityEngine.Random.InitState(33);
        player = GameObject.FindWithTag("Player");
        NextLevel();
    }

    #region Level Generation
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
                GenerateCave(mapSize);
                //Vector2Int mapSize = initMapSize + new Vector2Int(UnityEngine.Random.Range(-2, 0), UnityEngine.Random.Range(-2, 0));
                //GenerateColumnarMaze(mapSize);
            }
            else
            {
                Vector2Int mapSize = initMapSize + new Vector2Int(UnityEngine.Random.Range(level, 2 + 2 * level), UnityEngine.Random.Range(level, 2 + 2 * level)) * 2;
                GenerateCorridorMaze(mapSize);
                //Vector2Int mapSize = initMapSize + new Vector2Int(UnityEngine.Random.Range(level, 2 + 2 * level), UnityEngine.Random.Range(level, 2 + 2 * level));
                //GenerateDollRoom(mapSize);
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
    #endregion

    #region Individual Maze Generation

    /// <summary>
    /// Generate an empty room with floors and surrounding walls, its entrance connected to the exit of the previous maze
    /// </summary>
    /// <param name="mapSize"> Size of maze </param>
    /// <param name="startCoord"> Local x-z coordinate of the entrance of current maze </param>
    /// <param name="endCoord"> Local x-z coordinate of the exit of current maze </param>
    /// <param name="border"> Boundary of start/end spawn </param>
    Transform GenerateBasic(Vector2Int mapSize, out Vector2Int startCoord, out Vector2Int endCoord, CoordCriteria startEndCriteria, int border = 0, bool withWalls = true)
    {
        do
            startCoord = new Vector2Int(UnityEngine.Random.Range(border, mapSize.x - border), 0);
        while (!startEndCriteria(startCoord));
        do
            endCoord = new Vector2Int(UnityEngine.Random.Range(border, mapSize.x - border), mapSize.y - 1);
        while (!startEndCriteria(endCoord));

        // Generate empty parent object
        Transform mapTransform = new GameObject("Room").transform;
        mapTransform.SetParent(transform);
        mapTransform.localPosition = new Vector3(mapSize.x / 2 - startCoord.x, 0, mapSize.y / 2 - startCoord.y) + lastEndPos;

        // Generate floor
        Transform floor = Instantiate(corridorFloorPrefab, mapTransform);
        if (mapSize.x % 2 == 0)
            floor.localPosition += new Vector3(-0.5f, 0, 0);
        if (mapSize.y % 2 == 0)
            floor.localPosition += new Vector3(0, 0, -0.5f);
        floor.localScale = new Vector3(mapSize.x * 0.1f, 1, mapSize.y * 0.1f);

        // Generate walls with openings at startPos and endPos
        if (withWalls)
        {
            // UP
            for (int i = 0; i < mapSize.x; i++)
            {
                if (i != endCoord.x)
                {
                    Vector3 position = new Vector3(-mapSize.x / 2 + i, 0, -mapSize.y / 2 + mapSize.y - 1 + 0.5f);
                    Vector3 angle = new Vector3(0, 0, 0);

                    Transform newWall = Instantiate(corridorWallPrefab, mapTransform);
                    newWall.localPosition = position;
                    newWall.localEulerAngles = angle;
                }
            }

            // LEFT
            for (int j = 0; j < mapSize.y; j++)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 - 0.5f, 0, -mapSize.y / 2 + j);
                Vector3 angle = new Vector3(0, -90, 0);

                Transform newWall = Instantiate(corridorWallPrefab, mapTransform);
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

                    Transform newWall = Instantiate(corridorWallPrefab, mapTransform);
                    newWall.localPosition = position;
                    newWall.localEulerAngles = angle;
                }
            }

            // RIGHT
            for (int j = 0; j < mapSize.y; j++)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 + mapSize.x - 1 + 0.5f, 0, -mapSize.y / 2 + j);
                Vector3 angle = new Vector3(0, 90, 0);

                Transform newWall = Instantiate(corridorWallPrefab, mapTransform);
                newWall.localPosition = position;
                newWall.localEulerAngles = angle;
            }
        }

        lastEndPos += new Vector3(endCoord.x - startCoord.x, 0, endCoord.y - startCoord.y + 1);

        // Connector to next maze
        Transform connector = Instantiate(connectorPrefab, transform);
        connector.localPosition = lastEndPos;
        lastEndPos += new Vector3(0, 0, 1);

        return mapTransform;
    }

    /// <summary>
    /// Generate corridors by RB
    /// </summary>
    void GenerateCorridorMaze(Vector2Int mazeSize)
    {
        Direction4[,] maze = RecursiveBacktracker(mazeSize);
        int[,] map = FattenMaze(maze);
        map = RoomInMaze(map, 3, 5, 5, new List<Region>(), out _, out _);
        map = OpenDeadEnds(0, map);
        map = ConnectRegions(0, map, 1, 1, 1, 1, 10000, false);

        Vector2Int mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));
        Transform mapTransform = GenerateBasic(mapSize, out Vector2Int startCoord, out Vector2Int endCoord, d => d.x % 2 == 1, 2, false);

        for (int i = 0; i < mapSize.x; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                if (map[i, j] == 1 && new Vector2Int(i, j) != startCoord && new Vector2Int(i, j) != endCoord)
                {
                    Transform newCube = Instantiate(cubePrefab, mapTransform);
                    Vector3 position = new Vector3(-mapSize.x / 2 + i, 0, -mapSize.y / 2 + j);
                    newCube.localPosition = position;
                }
            }
    }

    /// <summary>
    /// Generate caves by CA and connect them. Grid size is (0.5, 0.5).
    /// </summary>
    void GenerateCave(Vector2Int mapSize)
    {
        Transform mapTransform = GenerateBasic(mapSize / 2, out Vector2Int startPos, out Vector2Int endPos, d => true, 2, false);
        int[,][] setting = new int[mapSize.x, mapSize.y][];
        for (int x = 0; x < mapSize.x; x++)
            for (int y = 0; y < mapSize.y; y++)
            {
                setting[x, y] = new int[] { caveCellInitChance, caveCellBirthLimit, caveCellDdeathLimit };
            }
        int[,] map = Cellular(mapSize, setting, cellularPassEpoch);
        map = ConnectRegions(0, map, 2, 8, 5, 10, 10000, false);

        // A straight tunnel from start to the first empty space it encounters
        Vector2Int tunnelFronStart = startPos * 2;
        while (CheckInMapRange(tunnelFronStart, mapSize) && map[tunnelFronStart.x, tunnelFronStart.y] == 1 && map[tunnelFronStart.x + 1, tunnelFronStart.y] == 1)
        {
            map[tunnelFronStart.x, tunnelFronStart.y] = 0;
            map[tunnelFronStart.x + 1, tunnelFronStart.y] = 0;
            tunnelFronStart += new Vector2Int(0, 1);
        }

        // A straight tunnel from end to the first empty space it encounters
        Vector2Int tunnelFronEnd = endPos * 2 + new Vector2Int(0, 1);
        while (CheckInMapRange(tunnelFronEnd, mapSize) && map[tunnelFronEnd.x, tunnelFronEnd.y] == 1 && map[tunnelFronEnd.x + 1, tunnelFronEnd.y] == 1)
        {
            map[tunnelFronEnd.x, tunnelFronEnd.y] = 0;
            map[tunnelFronEnd.x + 1, tunnelFronEnd.y] = 0;
            tunnelFronEnd += new Vector2Int(0, -1);
        }

        for (int i = 0; i < mapSize.x; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                if (map[i, j] == 1)
                {
                    Transform newCube = Instantiate(caveWallPrefab, mapTransform);
                    Vector3 position = new Vector3(-mapSize.x / 4 + i / 2f - 0.25f, 0, -mapSize.y / 4 + j / 2f - 0.25f);
                    newCube.localPosition = position;
                }
            }

        // Generate lights
        List<Region> regions = GetRegions(0, map);
        List<Vector3> lightPositions = new List<Vector3>();
        foreach (DirectionalTile outline in regions[0].Outline)
        {
            Vector3 position = new Vector3(-mapSize.x / 4 + outline.Position.x / 2f - 0.25f, 0.5f, -mapSize.y / 4 + outline.Position.y / 2f - 0.25f)
                + Coord2PosXZ(Offset4[GetOpposite(outline.Direction)]) * 0.25f;
            if (CheckMinSeparation(lightPositions, position, minCaveLightSeparation))
            {
                Transform newLight = Instantiate(caveLightPrefab, mapTransform);
                newLight.localPosition = position;
                newLight.localEulerAngles = Angle4[outline.Direction];
                lightPositions.Add(position);
            }
        }
    }

    /// <summary>
    /// Generate doll rooms by RB
    /// </summary>
    void GenerateDollRoom(Vector2Int mazaSize)
    {
        Transform mazeTransform = GenerateBasic(mazaSize, out Vector2Int startCoord, out Vector2Int endCoord, d => true);
        Direction4[,] maze = RecursiveBacktracker(mazaSize);
        List<Vector3> kiwiPositions = new List<Vector3>();

        for (int i = 0; i < mazaSize.x; i++)
            for (int j = 0; j < mazaSize.y; j++)
            {
                Vector3 position = new Vector3(-mazaSize.x / 2 + i, 0, -mazaSize.y / 2 + j);
                List<Vector3> wallPosList = new List<Vector3>();
                List<Vector3> wallAngleList = new List<Vector3>();

                if ((i + j) % 2 == 0)
                {
                    // Generate walls
                    foreach (Direction4 dir in Enum.GetValues(typeof(Direction4)))
                    {
                        if (Offset4.ContainsKey(dir) && maze[i, j].HasFlag(dir))
                        {
                            Vector3 newPos = position + Coord2PosXZ(Offset4[dir]) * 0.5f;
                            Vector3 newAngle = Angle4[dir];
                            wallPosList.Add(newPos);
                            wallAngleList.Add(newAngle);

                            if (CheckInMapRange(new Vector2Int(i, j) + Offset4[dir], mazaSize))
                            {
                                Transform newWall = Instantiate(corridorWallPrefab, mazeTransform);
                                newWall.localPosition = newPos;
                                newWall.localEulerAngles = newAngle;
                            }
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
        Transform mapTransform = GenerateBasic(mapSize * 4, out _, out _, d => true);
        int[,] maze = Columnar(mapSize, _2x2ColumnChance);

        for (int i = 0; i < mapSize.x; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                int col = maze[i, j];
                Vector3 position = new Vector3(-mapSize.x * 2 + 1.5f + i * 4, 0, -mapSize.y * 2 + 1.5f + j * 4);

                if (col == 1)
                {
                    Transform newWall = Instantiate(_1x1ColPrefab, mapTransform);
                    Vector3 offset = new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), 0, UnityEngine.Random.Range(-1.5f, 1.5f));
                    newWall.localPosition = position + offset;
                }
                else if (col == 2)
                {
                    Transform newWall = Instantiate(_2x2ColPrefab, mapTransform);
                    Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
                    newWall.localPosition = position + offset;
                }
            }
    }
    #endregion
}
