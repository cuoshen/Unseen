using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField]
    int level;

    #region General Parameters
    [Header("General")]
    [SerializeField]
    Transform floorPrefab;
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
    float dollChance;
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
    Transform corridorLightPrefab;
    [SerializeField]
    int minCorridorLightSeparation;
    #endregion
    #region Doll Room Generation Parameters
    [Header("Doll Room Generation")]
    [SerializeField]
    Transform dollWallPrefab;
    #endregion
    #region Cave Generation Parameters
    [Header("Cave Generation")]
    [SerializeField]
    int caveCellInitChance;
    [SerializeField]
    int caveCellBirthLimit;
    [SerializeField]
    int caveCellDeathLimit;
    [SerializeField]
    int cellularPassEpoch;
    [SerializeField]
    Transform caveWallPrefab;
    [SerializeField]
    Transform caveLightPrefab;
    [SerializeField]
    int minCaveLightSeparation;
    #endregion
    #region Columns Generation Parameters
    [Header("Columns Generation")]
    [SerializeField]
    Transform _1x1ColPrefab;
    [SerializeField]
    Transform _2x2ColPrefab;
    #endregion

    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        s_Instance = this;
    }

    void Start()
    {
        NextLevel();
    }

    #region Parameter Growth Functions
    int RoomCount()
    {
        return UnityEngine.Random.Range((int)(level / 1.5), level);
    }

    Vector2Int CorridorSize()
    {
        return new Vector2Int(UnityEngine.Random.Range(4 + level / 2, 6 + (int)(level / 1.5)), UnityEngine.Random.Range(4 + level / 2, 6 + (int)(level / 1.5)));
    }

    int DollRoomAttempts_Within(Vector2Int mazeSize)
    {
        return UnityEngine.Random.Range(Math.Min(mazeSize.x, mazeSize.y), Math.Max(mazeSize.x, mazeSize.y)) / 3 - 2;
    }

    Vector2Int DollRoomSize_Separate()
    {
        return new Vector2Int(UnityEngine.Random.Range(2, 5), UnityEngine.Random.Range(2, 5)) * 2;
    }

    Vector2Int CaveSize()
    {
        // Parity causes grid offset issues
        return new Vector2Int(UnityEngine.Random.Range(7, 10), UnityEngine.Random.Range(7, 12)) * 4;
    }

    Vector2Int ColumnarSize()
    {
        return new Vector2Int(UnityEngine.Random.Range(3, 5), UnityEngine.Random.Range(3, 5));
    }
    #endregion

    #region Level Generation
    void GenerateLevel()
    {
        lastEndPos = new Vector3(0, 0, 0);

        Transform start = Instantiate(startPrefab, transform);
        start.localPosition = lastEndPos + new Vector3(0, -0.5f, -1f);

        GenerateConnector();

        for (int i = 0; i <= RoomCount(); i++)
        {
            if (UnityEngine.Random.Range(0f, 1f) < caveChance)
                GenerateCave(CaveSize());
            else if (UnityEngine.Random.Range(0f, 1f) < dollChance)
                GenerateDollRoomSeparate(DollRoomSize_Separate());
            else
                GenerateCorridorMaze(CorridorSize());
        }

        Transform end = Instantiate(endPrefab, transform);
        end.localPosition = lastEndPos + new Vector3(0, 0.5f, 0);
    }

    public void ClearLevel()
    {
        if (transform.childCount != 0)
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
        }
    }

    public void NextLevel()
    {
        //UnityEngine.Random.InitState(33);
        level++;
        ClearLevel();
        GenerateLevel();
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
        while (!startEndCriteria(endCoord) && Math.Abs(endCoord.x - startCoord.x) < 2);

        // Generate empty parent object
        Transform mapTransform = new GameObject("Room").transform;
        mapTransform.SetParent(transform);
        mapTransform.localPosition = new Vector3(mapSize.x / 2 - startCoord.x, 0, mapSize.y / 2 - startCoord.y) + lastEndPos;

        // Generate floor
        Transform floor = Instantiate(floorPrefab, mapTransform);
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

                    Transform newWall = Instantiate(dollWallPrefab, mapTransform);
                    newWall.localPosition = position;
                    newWall.localEulerAngles = angle;
                }
            }

            // LEFT
            for (int j = 0; j < mapSize.y; j++)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 - 0.5f, 0, -mapSize.y / 2 + j);
                Vector3 angle = new Vector3(0, -90, 0);

                Transform newWall = Instantiate(dollWallPrefab, mapTransform);
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

                    Transform newWall = Instantiate(dollWallPrefab, mapTransform);
                    newWall.localPosition = position;
                    newWall.localEulerAngles = angle;
                }
            }

            // RIGHT
            for (int j = 0; j < mapSize.y; j++)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 + mapSize.x - 1 + 0.5f, 0, -mapSize.y / 2 + j);
                Vector3 angle = new Vector3(0, 90, 0);

                Transform newWall = Instantiate(dollWallPrefab, mapTransform);
                newWall.localPosition = position;
                newWall.localEulerAngles = angle;
            }
        }

        lastEndPos += new Vector3(endCoord.x - startCoord.x, 0, endCoord.y - startCoord.y + 1);

        GenerateConnector();

        return mapTransform;
    }

    /// <summary>
    /// Connector to next maze
    /// </summary>
    void GenerateConnector()
    {
        Transform connector = Instantiate(connectorPrefab, transform);
        connector.localPosition = lastEndPos;
        lastEndPos += new Vector3(0, 0, 1);
    }

    void GenerateLightOnOutlineBySeparation(int[,] map, Transform mapTransform, Transform lightPrefab, int minSeparation, float scale = 1)
    {
        Vector2Int mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));

        List<Region> regions = GetRegions(0, map);
        List<Vector3> lightPositions = new List<Vector3>();
        foreach (DirectionalTile outline in regions[0].Outline)
        {
            Vector3 position = (new Vector3(-mapSize.x / 2 + outline.Position.x + 0.5f, 0, -mapSize.y / 2 + outline.Position.y + 0.5f)
                + Coord2PosXZ(Offset4[GetOpposite(outline.Direction)]) * 0.5f) * scale + new Vector3(-0.5f, 0.5f, -0.5f);

            if (CheckMinSeparation(lightPositions, position, minSeparation))
            {
                Transform newLight = Instantiate(lightPrefab, mapTransform);
                newLight.localPosition = position;
                newLight.localEulerAngles = Angle4[outline.Direction];
                lightPositions.Add(position);
            }
        }
    }

    void GenerateCorridorMaze(Vector2Int mazeSize)
    {
        Direction4[,] maze = RecursiveBacktracker(mazeSize);
        int[,] map = FattenMaze(maze);

        map = RoomInMaze(map, 2, 5, DollRoomAttempts_Within(mazeSize), new List<Region>(), out List<Region> allRooms, out List<RectRoom> newRooms);
        map = ConnectRegions(0, map, 1, 1, 1, 1);
        map = OpenDeadEnds(0, map);

        Vector2Int mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));
        Transform mapTransform = GenerateBasic(mapSize, out Vector2Int startCoord, out Vector2Int endCoord, d => d.x % 2 == 1, 2, false);

        Vector2Int s = new Vector2Int(startCoord.x, startCoord.y + 1);
        Vector2Int e = new Vector2Int(endCoord.x, endCoord.y - 1);
        List<Vector2Int> pathToEnd = new Astar(Astar.ConvertToBoolArray(map), s, e).Result;

        List <Vector3> lightPositions = new List<Vector3>();
        List<Vector3> kiwiPositions = new List<Vector3>();

        for (int i = 0; i < mapSize.x; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                Vector2Int coord = new Vector2Int(i, j);
                Vector3 position = new Vector3(-mapSize.x / 2 + i, 0, -mapSize.y / 2 + j);

                if (map[i, j] == 1 && new Vector2Int(i, j) != startCoord && new Vector2Int(i, j) != endCoord)
                {
                    Transform newCube = Instantiate(corridorWallPrefab, mapTransform);
                    newCube.localPosition = position;
                }

                // Do not generate light or enemies inside walls, at start and end, or within rooms
                if (map[i, j] == 0 && new Vector2Int(i, j - 1) != startCoord && new Vector2Int(i, j + 1) != endCoord
                    && allRooms.Select(d => d.Area).Where(d => d.Contains(coord)).ToList().Count == 0)
                {
                    List<Vector3> wallPosList = new List<Vector3>();
                    List<Vector3> wallAngleList = new List<Vector3>();
                    foreach (DirectionalTile p in GetNeighbours4(coord, mapSize))
                    {
                        if (map[p.Position.x, p.Position.y] != 0)
                        {
                            Vector3 newPos = position + Coord2PosXZ(Offset4[p.Direction]) * 0.5f;
                            Vector3 newAngle = Angle4[p.Direction];
                            wallPosList.Add(newPos);
                            wallAngleList.Add(newAngle);
                        }
                    }

                    // Generate lights, but not too close to other lights
                    if (wallPosList.Count != 0 && CheckMinSeparation(lightPositions, position, minCorridorLightSeparation))
                    {
                        Transform newLight = Instantiate(corridorLightPrefab, mapTransform);
                        int randIndex = UnityEngine.Random.Range(0, wallPosList.Count);
                        newLight.localPosition = wallPosList[randIndex] + new Vector3(0, 0.5f, 0);
                        newLight.localEulerAngles = wallAngleList[randIndex];
                        lightPositions.Add(position);
                    }

                    //Generate enemies, but not too close to other enemies or on the critical path or on lights
                    Vector2Int disFromStart = coord - startCoord;
                    Vector2Int disFromEnd = coord - endCoord;
                    if (disFromStart.magnitude > minKiwiSeparation && disFromEnd.magnitude > minKiwiSeparation
                        && !pathToEnd.Contains(coord) && !lightPositions.Contains(position)
                        && CheckMinSeparation(kiwiPositions, position, minKiwiSeparation))
                    {
                        Transform newKiwi = Instantiate(kiwiPrefab, mapTransform);
                        newKiwi.localPosition = position;
                        kiwiPositions.Add(position);
                    }
                }
            }

        foreach (RectRoom newRoom in newRooms)
        {
            GenerateDollRoomWithin(mapSize, newRoom, mapTransform);
        }
    }

    void GenerateDollRoomSeparate(Vector2Int mazeSize)
    {
        Transform mazeTransform = GenerateBasic(mazeSize, out Vector2Int startCoord, out Vector2Int endCoord, d => true);
        Direction4[,] maze = RecursiveBacktracker(mazeSize);

        List<Vector3> kiwiPositions = new List<Vector3>();

        for (int i = 0; i < mazeSize.x; i++)
            for (int j = 0; j < mazeSize.y; j++)
            {
                Vector2Int coord = new Vector2Int(i, j);
                Vector3 position = new Vector3(-mazeSize.x / 2 + i, 0, -mazeSize.y / 2 + j);
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

                            if (CheckInMapRange(new Vector2Int(i, j) + Offset4[dir], mazeSize))
                            {
                                Transform newWall = Instantiate(dollWallPrefab, mazeTransform);
                                newWall.localPosition = newPos;
                                newWall.localEulerAngles = newAngle;
                            }
                        }
                    }
                }

                // Generate lights
                if (wallPosList.Count != 0 && coord != startCoord && coord != endCoord)
                {
                    Transform newLight = Instantiate(corridorLightPrefab, mazeTransform);
                    int randIndex = UnityEngine.Random.Range(0, wallPosList.Count);
                    newLight.localPosition = wallPosList[randIndex] + new Vector3(0, 0.5f, 0);
                    newLight.localEulerAngles = wallAngleList[randIndex];
                }

                //Generate enemies
                Vector2Int disFromStart = coord - startCoord;
                Vector2Int disFromEnd = coord - endCoord;
                if (wallPosList.Count == 3 && disFromStart.magnitude > minKiwiSeparation && disFromEnd.magnitude > minKiwiSeparation
                    && CheckMinSeparation(kiwiPositions, position, minKiwiSeparation))
                {
                    Transform newKiwi = Instantiate(kiwiPrefab, mazeTransform);
                    newKiwi.localPosition = position;
                    kiwiPositions.Add(position);
                }
            }
    }

    void GenerateDollRoomWithin(Vector2Int mapSize, RectRoom room, Transform mazeTransform)
    {
        Direction4[,] maze = RecursiveBacktracker(room.Size);

        for (int i = 0; i < room.Size.x; i++)
            for (int j = 0; j < room.Size.y; j++)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 + room.BottomLeft.x + i, 0, -mapSize.y / 2 + room.BottomLeft.y + j);
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

                            if (CheckInMapRange(new Vector2Int(i, j) + Offset4[dir], room.Size))
                            {
                                Transform newWall = Instantiate(dollWallPrefab, mazeTransform);
                                newWall.localPosition = newPos;
                                newWall.localEulerAngles = newAngle;
                            }
                        }
                    }
                }
            }
    }

    void GenerateCave(Vector2Int mapSize)
    {
        Debug.Log(mapSize);
        Transform mapTransform = GenerateBasic(mapSize / 2, out Vector2Int startPos, out Vector2Int endPos, d => true, 2, false);
        int[,][] setting = new int[mapSize.x, mapSize.y][];
        for (int x = 0; x < mapSize.x; x++)
            for (int y = 0; y < mapSize.y; y++)
            {
                setting[x, y] = new int[] { caveCellInitChance, caveCellBirthLimit, caveCellDeathLimit };
            }
        int[,] map = Cellular(mapSize, setting, cellularPassEpoch);
        map = ConnectRegions(0, map, 2, 8, 5, 10);

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
                    // scale down by 2 and move by (-0.25, -0.25) to center of grid
                    Vector3 position = new Vector3((-mapSize.x / 2f + i) / 2f - 0.25f, 0, (-mapSize.y / 2f + j) / 2f - 0.25f);
                    newCube.localPosition = position;
                }
            }

        // Generate lights
        GenerateLightOnOutlineBySeparation(map, mapTransform, caveLightPrefab, minCaveLightSeparation, 0.5f);
    }

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
