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

    // Level 6 with seed 88 with caveChance and dollChance both 0 for a Compartments right at the start
    public int seed;
    public bool useSeed;
    int level;
    MeshGenerator meshGenerator;
    Vector3 lastEndPos;

    #region General Parameters
    [Header("General")]
    [SerializeField]
    Transform roomPrefab;
    [SerializeField]
    Transform startPrefab;
    [SerializeField]
    Transform endPrefab;
    [SerializeField]
    Transform connectorPrefab;
    [SerializeField]
    Transform trainPrefab;
    #endregion
    #region Maze Variation Creation Chance
    [Header("Maze Variation Creation Chance")]
    [SerializeField]
    float trainChance;
    [SerializeField]
    float caveChance;
    [SerializeField]
    float ascentChance;
    #endregion
    #region Enemy Parameters
    [Header("Enemy")]
    [SerializeField]
    int minInsectSeparation;
    [SerializeField]
    Transform insectPrefab;
    [SerializeField]
    int minGiantHalfSeparation;
    [SerializeField]
    Transform giantPrefab;
    #endregion
    #region Corridor Generation Parameters
    [Header("Corridor Generation")]
    [SerializeField]
    Transform corridorFloorPrefab;
    [SerializeField]
    Transform corridorWallPrefab;
    [SerializeField]
    Transform corridorFenceDoorPrefab;
    [SerializeField]
    Transform corridorLightPrefab;
    [SerializeField]
    Transform voidFencePrefab;
    [SerializeField]
    Transform voidLightPrefab;
    [SerializeField]
    int minCorridorLightSeparation;
    [SerializeField]
    int maxVoidSize;
    [SerializeField]
    float voidChance;
    [SerializeField]
    float fenceDoorChance;
    #endregion
    #region Cave Generation Parameters
    [Header("Cave Generation")]
    [SerializeField]
    Transform caveFloorPrefab;
    [SerializeField]
    Transform caveWallPrefab;
    [SerializeField]
    Transform caveRoofMeshPrefab;
    [SerializeField]
    Transform caveWallMeshPrefab;
    [SerializeField]
    Transform caveDummyPrefab;
    [SerializeField]
    Transform caveLightPrefab;
    [SerializeField]
    Transform caveLightTallPrefab;
    [SerializeField]
    int minCaveLightSeparation;

    int caveCellInitChance = 25;
    int caveCellBirthLimit = 3;
    int caveCellDeathLimit = 2;
    int cellularPassEpoch = 6;
    #endregion
    #region Compartments Generation Parameters
    [Header("Compartments Generation")]
    [SerializeField]
    Transform compWallPrefab;
    [SerializeField]
    Transform compDoorPrefab;
    [SerializeField]
    Transform compLightPrefab;
    [SerializeField]
    Transform compAreaPrefab;
    [SerializeField]
    int minCompHalfLength;
    [SerializeField]
    int maxCompHalfLength;
    [SerializeField]
    float additionalUnlockChance;
    #endregion
    #region Ascent Generation Parameters
    [Header("Ascent Generation")]
    [SerializeField]
    Transform ascentStairsPrefab;
    [SerializeField]
    Transform ascentCornerPrefab;
    [SerializeField]
    Transform ascentLightPrefab;
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
        if (useSeed)
            UnityEngine.Random.InitState(seed);

        meshGenerator = GetComponent<MeshGenerator>();
        NextLevel();
    }

    #region Parameter Growth Functions
    int RoomCount()
    {
        return UnityEngine.Random.Range(Mathf.CeilToInt(level / 2f), Mathf.CeilToInt(level / 1.5f));
    }

    Vector2Int CorridorSize()
    {
        return new Vector2Int(UnityEngine.Random.Range(4 + Mathf.CeilToInt(level / 2f), 6 + Mathf.CeilToInt(level / 1.5f)), UnityEngine.Random.Range(4 + Mathf.CeilToInt(level / 2f), 6 + Mathf.CeilToInt(level / 1.5f)));
    }

    int CompartmentsAttempts(Vector2Int mazeSize)
    {
        return UnityEngine.Random.Range(Math.Min(mazeSize.x, mazeSize.y), Math.Max(mazeSize.x, mazeSize.y)) / 3 - 2;
    }

    Vector2Int AscentSize()
    {
        return new Vector2Int(UnityEngine.Random.Range(4 + Mathf.CeilToInt(level / 2f), 6 + Mathf.CeilToInt(level / 1.5f)), UnityEngine.Random.Range(4 + Mathf.CeilToInt(level / 2f), 6 + Mathf.CeilToInt(level / 1.5f)));
    }

    Vector2Int CaveSize()
    {
        // Parity causes grid offset issues
        return new Vector2Int(UnityEngine.Random.Range(7, 10), UnityEngine.Random.Range(7, 12)) * 4;
    }
    #endregion

    #region Level Generation
    void GenerateLevel()
    {
        lastEndPos = new Vector3(0, 0, 0);

        Transform start = Instantiate(startPrefab, transform);
        start.localPosition = lastEndPos;
        lastEndPos += new Vector3(-1, 0, 4);

        for (int i = 0; i < RoomCount(); i++)
        {
            if (UnityEngine.Random.Range(0f, 1f) < ascentChance)
                GenerateAscent(AscentSize());
            else if (UnityEngine.Random.Range(0f, 1f) < caveChance)
                GenerateCave(CaveSize());
            else
                GenerateCorridorMaze(CorridorSize());
        }

        GenerateConnector();

        Transform end = Instantiate(endPrefab, transform);
        end.localPosition = lastEndPos;
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
        Debug.Log("Next level");
        level = LevelCounter.Instance.IncrementLevel();
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
    /// <param name="separation"> Separation of start/end spawn </param>
    Transform GenerateBasic(Transform floorprefab, Transform wallPrefab, AudioReverbPreset reverbPreset, Vector2Int mapSize, out Vector2Int startCoord, out Vector2Int endCoord, CoordCriteria startCriteria, CoordCriteria endCriteria, int border = 0, int separation = 2)
    {

        if (UnityEngine.Random.Range(0f, 1f) < trainChance)
            GenerateTrainTrack();
        else
            GenerateConnector();

        do
            startCoord = new Vector2Int(UnityEngine.Random.Range(border, mapSize.x - border), 0);
        while (!startCriteria(startCoord));
        do
            endCoord = new Vector2Int(UnityEngine.Random.Range(border, mapSize.x - border), mapSize.y - 1);
        while (!endCriteria(endCoord) || Math.Abs(endCoord.x - startCoord.x) < separation);

        // Generate empty parent object
        Transform mapTransform = Instantiate(roomPrefab, transform);
        mapTransform.localPosition = new Vector3(mapSize.x / 2 - startCoord.x, 0, mapSize.y / 2 - startCoord.y) + lastEndPos;
        mapTransform.GetComponent<Room>().reverbPreset = reverbPreset;
        mapTransform.GetComponent<BoxCollider>().size = new Vector3(mapSize.x, 1, mapSize.y);

        // Generate floor
        if (floorprefab)
        {
            Transform floor = Instantiate(floorprefab, mapTransform);
            if (mapSize.x % 2 == 0)
                floor.localPosition += new Vector3(-0.5f, 0, 0);
            if (mapSize.y % 2 == 0)
                floor.localPosition += new Vector3(0, 0, -0.5f);
            floor.localScale = new Vector3(mapSize.x, 1, mapSize.y);
        }

        // Generate walls with openings at startPos and endPos
        if (wallPrefab)
        {
            // UP
            for (int i = 0; i < mapSize.x; i++)
            {
                if (i != endCoord.x)
                {
                    Vector3 position = new Vector3(-mapSize.x / 2 + i, 0, -mapSize.y / 2 + mapSize.y - 1 + 0.5f);
                    Vector3 angle = new Vector3(0, 0, 0);

                    Transform newWall = Instantiate(wallPrefab, mapTransform);
                    newWall.localPosition = position;
                    newWall.localEulerAngles = angle;
                }
            }

            // LEFT
            for (int j = 0; j < mapSize.y; j++)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 - 0.5f, 0, -mapSize.y / 2 + j);
                Vector3 angle = new Vector3(0, -90, 0);

                Transform newWall = Instantiate(wallPrefab, mapTransform);
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

                    Transform newWall = Instantiate(wallPrefab, mapTransform);
                    newWall.localPosition = position;
                    newWall.localEulerAngles = angle;
                }
            }

            // RIGHT
            for (int j = 0; j < mapSize.y; j++)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 + mapSize.x - 1 + 0.5f, 0, -mapSize.y / 2 + j);
                Vector3 angle = new Vector3(0, 90, 0);

                Transform newWall = Instantiate(wallPrefab, mapTransform);
                newWall.localPosition = position;
                newWall.localEulerAngles = angle;
            }
        }

        lastEndPos += new Vector3(endCoord.x - startCoord.x, 0, endCoord.y - startCoord.y + 1);

        return mapTransform;
    }

    void GenerateCorridorMaze(Vector2Int mazeSize)
    {
        Direction4[,] maze = RecursiveBacktracker(mazeSize);
        int[,] map = FattenMaze(maze);

        map = RoomInMaze(map, minCompHalfLength, maxCompHalfLength, CompartmentsAttempts(mazeSize), new List<Region>(), out List<Region> allRooms, out List<RectRoom> newRooms);
        map = ConnectRegions(0, map, 1, 1, 1, 1, out _);
        map = OpenDeadEnds(0, map);

        Vector2Int mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));
        Transform mapTransform = GenerateBasic(null, null, AudioReverbPreset.Hallway, mapSize, out Vector2Int startCoord, out Vector2Int endCoord, start => start.x % 2 == 1, end => end.x % 2 == 1, 2);

        // Generate void
        List<Region> impassableRegions = GetRegions(1, map);

        foreach (Region region in impassableRegions)
        {
            if (region.Area.Count <= maxVoidSize && UnityEngine.Random.Range(0f, 1f) < voidChance)
            {
                foreach (Vector2Int tile in region.Area)
                {
                    map[tile.x, tile.y] = 2;
                }
                foreach (DirectionalTile tile in region.Outline)
                {
                    Vector3 newPos = new Vector3(-mapSize.x / 2 + tile.Position.x, 0, -mapSize.y / 2 + tile.Position.y)
                        + Coord2PosXZ(Offset4[GetOpposite(tile.Direction)]) * 0.5f;
                    Vector3 newAngle = Angle4[GetOpposite(tile.Direction)];

                    // Make Fence
                    Transform newFence = Instantiate(voidFencePrefab, mapTransform);
                    newFence.localPosition = newPos;
                    newFence.localEulerAngles = newAngle;
                }
            }
        }

        Vector2Int s = new Vector2Int(startCoord.x, startCoord.y + 1);
        Vector2Int e = new Vector2Int(endCoord.x, endCoord.y - 1);
        List<Vector2Int> pathToEnd = new Astar(Astar.ConvertToBoolArray(map), s, e).Result;

        List<Vector2Int> lightCoords = new List<Vector2Int>();
        List<Vector2Int> kiwiCoords = new List<Vector2Int>();

        for (int i = 0; i < mapSize.x; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                Vector2Int coord = new Vector2Int(i, j);
                Vector3 position = new Vector3(-mapSize.x / 2 + i, 0, -mapSize.y / 2 + j);

                if (map[i, j] != 2)
                {
                    // Generate floors
                    Transform newFloor = Instantiate(corridorFloorPrefab, mapTransform);
                    newFloor.localPosition = position;
                }

                if (map[i, j] == 1 && coord != startCoord && coord != endCoord)
                {
                    // Generate walls
                    Transform newWall = Instantiate(corridorWallPrefab, mapTransform);
                    newWall.localPosition = position;
                }

                // Do not generate light or enemies inside walls, at start and end, or within rooms
                if (map[i, j] == 0 && coord + new Vector2Int(0, -1) != startCoord && coord + new Vector2Int(0, 1) != endCoord
                    && allRooms.FindIndex(d => d.Area.Contains(coord)) == -1)
                {
                    List<int> wallTypeList = new List<int>();
                    List<Vector3> wallPosList = new List<Vector3>();
                    List<Vector3> wallAngleList = new List<Vector3>();
                    foreach (DirectionalTile p in GetNeighbours4(coord, mapSize))
                    {
                        if (map[p.Position.x, p.Position.y] != 0)
                        {
                            Vector3 newPos = position + Coord2PosXZ(Offset4[p.Direction]) * 0.5f;
                            Vector3 newAngle = Angle4[p.Direction];
                            wallTypeList.Add(map[p.Position.x, p.Position.y]);
                            wallPosList.Add(newPos);
                            wallAngleList.Add(newAngle);
                        }
                    }

                    // Generate lights, but not too close to other lights
                    if (wallPosList.Count != 0 && CheckMinSeparation(lightCoords, coord, minCorridorLightSeparation))
                    {
                        int randIndex = UnityEngine.Random.Range(0, wallPosList.Count);
                        Transform newLight = null;
                        if (wallTypeList[randIndex] == 1)
                            newLight = Instantiate(corridorLightPrefab, mapTransform);
                        else if (wallTypeList[randIndex] == 2)
                            newLight = Instantiate(voidLightPrefab, mapTransform);
                        newLight.localPosition = wallPosList[randIndex];
                        newLight.localEulerAngles = wallAngleList[randIndex];
                        lightCoords.Add(coord);
                    }

                    //Generate insect things, but not too close to other insect things, or on the critical path, or on lights, or on the outline of rooms
                    Vector2Int disFromStart = coord - startCoord;
                    Vector2Int disFromEnd = coord - endCoord;
                    if (disFromStart.magnitude > minInsectSeparation && disFromEnd.magnitude > minInsectSeparation
                        && !pathToEnd.Contains(coord) && !lightCoords.Contains(coord)
                        && CheckMinSeparation(kiwiCoords, coord, minInsectSeparation)
                        && allRooms.FindIndex(d => d.Outline.FindIndex(e => e.Position == coord) != -1) == -1)
                    {
                        Transform newInsect = Instantiate(insectPrefab, mapTransform);
                        newInsect.localPosition = position;
                        kiwiCoords.Add(coord);
                    }
                }
            }

        // Chance to generate fence door below lights on the visible wall without connection to another room.
        for (int j = 0; j < mapSize.y; j++)
        {
            Vector3 position = new Vector3(-mapSize.x / 2, 0, -mapSize.y / 2 + j);

            if (!CheckMinSeparation(lightCoords, new Vector2Int(0, j), 1.1f) && UnityEngine.Random.Range(0f, 1f) < fenceDoorChance)
            {
                Transform newFenceDoor = Instantiate(corridorFenceDoorPrefab, mapTransform);
                newFenceDoor.localPosition = position;
                newFenceDoor.localEulerAngles = new Vector3(0, -90, 0);
            }
        }
        
        // Generate compartments
        foreach (RectRoom room in newRooms)
        {
            Transform compArea = Instantiate(compAreaPrefab, mapTransform);
            compArea.localPosition = new Vector3(-mapSize.x / 2 + room.BottomLeft.x + room.Size.x / 2, 0, -mapSize.y / 2 + room.BottomLeft.y + room.Size.y / 2);
            compArea.GetComponent<BoxCollider>().size = new Vector3(room.Size.x, 1, room.Size.y);

            GenerateCompartments(room.Size, compArea);

            // Add doors to compartment openings
            foreach (DirectionalTile tile in room.RoomRegion.Outline)
            {
                Vector3 newPos = new Vector3(-mapSize.x / 2 + tile.Position.x, 0, -mapSize.y / 2 + tile.Position.y)
                    + Coord2PosXZ(Offset4[GetOpposite(tile.Direction)]) * 0.5f;
                Vector3 newAngle = Angle4[GetOpposite(tile.Direction)];
                if (map[tile.Position.x, tile.Position.y] == 0)
                {
                    // Make door and unlock them
                    Transform newWall = Instantiate(compDoorPrefab, mapTransform);
                    newWall.localPosition = newPos;
                    newWall.localEulerAngles = newAngle;
                    Door door = newWall.GetComponentInChildren<Door>();
                    door.Unlock();
                }
                else if (map[tile.Position.x, tile.Position.y] == 1)
                {
                    // Make wall
                    Transform newWall = Instantiate(compWallPrefab, mapTransform);
                    newWall.localPosition = newPos;
                    newWall.localEulerAngles = newAngle;
                }
            }
        }
    }

    void GenerateCave(Vector2Int mapSize)
    {
        Transform mapTransform = GenerateBasic(caveFloorPrefab, caveWallPrefab, AudioReverbPreset.Cave, mapSize / 2, out Vector2Int startPos, out Vector2Int endPos, start => true, end => true, 2);

        int[,][] setting = new int[mapSize.x, mapSize.y][];
        for (int i = 0; i < mapSize.x; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                setting[i, j] = new int[] { caveCellInitChance, caveCellBirthLimit, caveCellDeathLimit };
            }
        int[,] map = Cellular(mapSize, setting, cellularPassEpoch);
        map = ConnectRegions(0, map, 2, 8, 5, 10, out _);

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

        // Generate cave mesh
        Transform caveRoof = Instantiate(caveRoofMeshPrefab, mapTransform);
        Transform caveWall = Instantiate(caveWallMeshPrefab, mapTransform);
        meshGenerator.cave = caveRoof.GetComponent<MeshFilter>();
        meshGenerator.walls = caveWall.GetComponent<MeshFilter>();
        meshGenerator.GenerateMesh(map, 0.5f);

        // Generate dummy colliders for detection
        for (int i = 0; i < mapSize.x; i++)
            for (int j = 0; j < mapSize.y; j++)
            {
                if (map[i, j] == 1)
                {
                    Vector3 position = (new Vector3(-mapSize.x / 2 + i + 0.5f, 0, -mapSize.y / 2 + j + 0.5f)) * 0.5f + new Vector3(-0.5f, -0.1f, -0.5f);
                    Transform newLight = Instantiate(caveDummyPrefab, mapTransform);
                    newLight.localPosition = position;
                }
            }

        // Generate lights
        List<Vector2Int> lightCoords = GenerateLightOnOutlineByAstarSeparation(map, mapTransform, caveLightPrefab, minCaveLightSeparation, 0.5f);

        // Generate additional light randomly if there exists an empty enough coord space with no light
        for (int k = 0; k < 1000; k++)
        {
            int x = UnityEngine.Random.Range(minCaveLightSeparation, mapSize.x - minCaveLightSeparation);
            int y = UnityEngine.Random.Range(minCaveLightSeparation, mapSize.y - minCaveLightSeparation);

            bool hasLight = false;
            int emptyTileCount = 0;
            for (int i = x - minCaveLightSeparation; i <= x + minCaveLightSeparation; i++)
                for (int j = y - minCaveLightSeparation; j <= y + minCaveLightSeparation; j++)
                {
                    if (lightCoords.Contains(new Vector2Int(i, j)))
                    {
                        hasLight = true;
                        break;
                    }
                    if (map[i, j] == 0)
                        emptyTileCount++;
                }

            bool tooCloseToWall = false;
            for (int i = x - 1; i <= x + 1; i++)
                for (int j = y - 1; j <= y + 1; j++)
                {
                    if (map[i, j] == 1)
                        tooCloseToWall = true;
                }

            if (!hasLight && !tooCloseToWall && emptyTileCount >= 60 && map[x, y] == 0)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 + x + 0.5f, 0, -mapSize.y / 2 + y + 0.5f) * 0.5f + new Vector3(-0.5f, 0, -0.5f);
                Transform newLight = Instantiate(caveLightTallPrefab, mapTransform);
                newLight.localPosition = position;
                lightCoords.Add(new Vector2Int(x, y));

            }
        }

        // Generate giant things randomly if there exists an empty enough coord space
        List<Vector2Int> giantCoords = new List<Vector2Int>();
        int minGiantSeparation = minGiantHalfSeparation * 2 + 1;
        for (int k = 0; k < 1000; k++)
        {
            int x = UnityEngine.Random.Range(0, mapSize.x - minGiantSeparation);
            int y = UnityEngine.Random.Range(0, mapSize.y - minGiantSeparation);

            int emptyTileCount = 0;
            for (int i = x; i < x + minGiantSeparation; i++)
                for (int j = y; j < y + minGiantSeparation; j++)
                {
                    if (map[i, j] == 0)
                        emptyTileCount++;
                }

            Vector2Int newGiantCoord = new Vector2Int(x + minGiantHalfSeparation, y + minGiantHalfSeparation);
            if (emptyTileCount >= 150 && map[newGiantCoord.x, newGiantCoord.y] == 0)
            {
                Vector3 position = new Vector3(-mapSize.x / 2 + newGiantCoord.x + 0.5f, 0, -mapSize.y / 2 + newGiantCoord.y + 0.5f) * 0.5f + new Vector3(-0.5f, 0, -0.5f);
                // Generate giant things
                if (CheckMinSeparation(giantCoords, newGiantCoord, minGiantSeparation))
                {
                    Transform newGiant = Instantiate(giantPrefab, mapTransform);
                    newGiant.localPosition = position;
                    giantCoords.Add(newGiantCoord);
                }
            }
        }
    }

    void GenerateCompartments(Vector2Int roomSize, Transform compArea)
    {
        Direction4[,] maze = RecursiveBacktracker(roomSize);

        for (int i = 0; i < roomSize.x; i++)
            for (int j = 0; j < roomSize.y; j++)
            {
                Vector3 position = new Vector3(-roomSize.x / 2 + i, 0, -roomSize.y / 2 + j);

                if ((i + j) % 2 == 0)
                {
                    // Generate walls
                    foreach (Direction4 dir in Enum.GetValues(typeof(Direction4)))
                    {
                        if (Offset4.ContainsKey(dir) && CheckInMapRange(new Vector2Int(i, j) + Offset4[dir], roomSize))
                        {
                            Vector3 newPos = position + Coord2PosXZ(Offset4[dir]) * 0.5f;
                            Vector3 newAngle = Angle4[dir];

                            if (maze[i, j].HasFlag(dir))
                            {
                                if (UnityEngine.Random.Range(0f, 1f) < additionalUnlockChance)
                                {
                                    // Make door and unlock them
                                    Transform newWall = Instantiate(compDoorPrefab, compArea);
                                    newWall.localPosition = newPos;
                                    newWall.localEulerAngles = newAngle;
                                    Door door = newWall.GetComponentInChildren<Door>();
                                    door.Unlock();
                                }
                                else
                                {
                                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                                    {
                                        // Make door but lock them
                                        Transform newWall = Instantiate(compDoorPrefab, compArea);
                                        newWall.localPosition = newPos;
                                        newWall.localEulerAngles = newAngle;
                                        Door door = newWall.GetComponentInChildren<Door>();
                                        door.Lock();
                                    }
                                    else
                                    {
                                        // Make wall
                                        Transform newWall = Instantiate(compWallPrefab, compArea);
                                        newWall.localPosition = newPos;
                                        newWall.localEulerAngles = newAngle;
                                    }
                                }
                            }
                            else
                            {
                                // Make door and unlock them
                                Transform newWall = Instantiate(compDoorPrefab, compArea);
                                newWall.localPosition = newPos;
                                newWall.localEulerAngles = newAngle;
                                Door door = newWall.GetComponentInChildren<Door>();
                                door.Unlock();
                            }
                        }
                    }
                }

                // Generate lights
                Transform newLight = Instantiate(compLightPrefab, compArea);
                newLight.localPosition = position;
            }
    }

    void GenerateAscent(Vector2Int mapSize)
    {
        int[,] map = SinglePassway(mapSize, out Vector2Int startCoord, out Vector2Int endCoord, out List<DirectionalTile> passway, 1, 3, 5, 10);
        Transform mapTransform = GenerateBasic(null, null, AudioReverbPreset.Mountains, mapSize, out _, out _, start => start == startCoord, end => end == endCoord, 0, 0);

        int elevation = 0;

        Transform newPath;
        newPath = Instantiate(ascentCornerPrefab, mapTransform);
        newPath.localPosition = new Vector3(-mapSize.x / 2 + startCoord.x, elevation, -mapSize.y / 2 + startCoord.y);

        int gridsFromCorner = 1;

        for (int i = 0; i < passway.Count; i++)
        {
            Vector3 position = new Vector3(-mapSize.x / 2 + passway[i].Position.x, elevation, -mapSize.y / 2 + passway[i].Position.y);
            Vector3 angle = Angle4[passway[i].Direction];

            // Generate floors
            if (i == passway.Count - 1 || (passway[i].Direction == passway[i + 1].Direction))
            {
                newPath = Instantiate(ascentStairsPrefab, mapTransform);
                elevation++;
                gridsFromCorner++;
            }
            else
            {
                newPath = Instantiate(ascentCornerPrefab, mapTransform);
                gridsFromCorner = 0;
            }

            if(gridsFromCorner == 0)
            {
                List<Direction4> dirs = new List<Direction4>() { Direction4.LEFT, Direction4.RIGHT, Direction4.DOWN, Direction4.UP };
                dirs.Remove(passway[i + 1].Direction);
                dirs.Remove(GetOpposite(passway[i].Direction));
                Transform newLight = Instantiate(ascentLightPrefab, mapTransform);
                newLight.localPosition = position + new Vector3(0, -1, 0);
                foreach (Direction4 dir in dirs)
                {
                    newLight.localPosition += Coord2PosXZ(Offset4[dir]);
                }
            }
            else if (gridsFromCorner % 2 == 0)
            {
                List<Direction4> dirs = new List<Direction4>() { Direction4.LEFT, Direction4.RIGHT, Direction4.DOWN, Direction4.UP };
                dirs.Remove(passway[i].Direction);
                dirs.Remove(GetOpposite(passway[i].Direction));
                foreach (Direction4 dir in dirs)
                {
                    Transform newLight = Instantiate(ascentLightPrefab, mapTransform);
                    newLight.localPosition = position + Coord2PosXZ(Offset4[dir]);
                }
            }

            newPath.localPosition = position;
            newPath.localEulerAngles = angle;
        }

        lastEndPos += new Vector3(0, elevation, 0);
    }

    #endregion

    #region Misc Generation

    void GenerateConnector()
    {
        Transform connector = Instantiate(connectorPrefab, transform);
        connector.localPosition = lastEndPos;
        lastEndPos += new Vector3(0, 0.12f, 1.6f);
    }

    void GenerateTrainTrack()
    {
        Transform trainTrack = Instantiate(trainPrefab, transform);
        trainTrack.localPosition = lastEndPos;
        trainTrack.localEulerAngles = new Vector3(0, 180, 0);
        lastEndPos += new Vector3(0, 0, 1);
    }

    List<Vector2Int> GenerateLightOnOutlineBySeparation(int[,] map, Transform mapTransform, Transform lightPrefab, int minSeparation, float scale = 1)
    {
        Vector2Int mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));

        List<Region> regions = GetRegions(0, map);
        List<Vector2Int> lightCoords = new List<Vector2Int>();
        foreach (DirectionalTile outline in regions[0].Outline)
        {
            Vector2Int lightCoord = outline.Position + Offset4[GetOpposite(outline.Direction)];
            if (CheckMinSeparation(lightCoords, lightCoord, minSeparation))
            {
                Vector3 position = (new Vector3(-mapSize.x / 2 + lightCoord.x + 0.5f, 0, -mapSize.y / 2 + lightCoord.y + 0.5f)
                    - Coord2PosXZ(Offset4[GetOpposite(outline.Direction)]) * 0.5f) * scale + new Vector3(-0.5f, 0, -0.5f);

                Transform newLight = Instantiate(lightPrefab, mapTransform);
                newLight.localPosition = position;
                newLight.localEulerAngles = Angle4[outline.Direction];
                lightCoords.Add(lightCoord);
            }
        }

        return lightCoords;
    }

    List<Vector2Int> GenerateLightOnOutlineByAstarSeparation(int[,] map, Transform mapTransform, Transform lightPrefab, int minSeparation, float scale = 1)
    {
        Vector2Int mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));

        List<Region> regions = GetRegions(0, map);
        List<Vector2Int> lightCoords = new List<Vector2Int>();
        foreach (DirectionalTile outline in regions[0].Outline)
        {
            Vector2Int lightCoord = outline.Position + Offset4[GetOpposite(outline.Direction)];
            if (CheckMinAstarSeparation(map, lightCoords, lightCoord, minSeparation))
            {
                Vector3 position = (new Vector3(-mapSize.x / 2 + lightCoord.x + 0.5f, 0, -mapSize.y / 2 + lightCoord.y + 0.5f)
                    - Coord2PosXZ(Offset4[GetOpposite(outline.Direction)]) * 0.5f) * scale + new Vector3(-0.5f, 0, -0.5f);

                Transform newLight = Instantiate(lightPrefab, mapTransform);
                newLight.localPosition = position;
                newLight.localEulerAngles = Angle4[outline.Direction];
                lightCoords.Add(lightCoord);
            }
        }

        return lightCoords;
    }

    #endregion
}
