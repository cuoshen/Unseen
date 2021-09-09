using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

/// <summary>
/// Stores information of neighbours, edges, RB tiles, etc.
/// </summary>
public class DirectionalTile
{
	public Vector2Int Position;
	public Direction4 Direction;

	public DirectionalTile(Vector2Int pos, Direction4 dir)
    {
		Position = pos;
		Direction = dir;
    }

	public void Shift(Vector2Int displacement)
    {
		Position += displacement;
    }
}

/// <summary>
/// Stores the tiles in a region and tiles in its outline pointing outwards
/// </summary>
public class Region
{
	public List<Vector2Int> Area;
	public List<DirectionalTile> Outline;

	public Region(List<Vector2Int> area, List<DirectionalTile> outline)
    {
		Area = area;
		Outline = outline;
    }

	public void Shift(Vector2Int displacement)
	{
		for (int i = 0; i < Area.Count; i++)
			Area[i] += displacement;
		for (int i = 0; i < Outline.Count; i++)
			Outline[i].Shift(displacement);
	}
}

/// <summary>
/// Stores the location and size of a rectangular room
/// </summary>
public class RectRoom
{
	public Vector2Int BottomLeft;
	public Vector2Int Size;
	public Region RoomRegion;

	public RectRoom(Vector2Int bottomleft, Vector2Int size, Region roomRegion)
    {
		BottomLeft = bottomleft;
		Size = size;
		RoomRegion = roomRegion;
    }

	public void Shift(Vector2Int displacement)
    {
		BottomLeft += displacement;
		RoomRegion.Shift(displacement);
	}
}

#region Direction Lookups
/// <summary>
/// Stores 4 directions in the form of flags.
/// </summary>
[Flags]
public enum Direction4
{
	NONE  = 0b0000,
	LEFT  = 0b0001,
	RIGHT = 0b0010,
	DOWN  = 0b0100,
	UP    = 0b1000,
	ALL   = 0b1111,
}

/// <summary>
/// Stores 8 directions in the form of flags.
/// </summary>
[Flags]
public enum Direction8
{
	NONE = 0b00000000,
	W    = 0b00000001,
	E    = 0b00000010,
	S    = 0b00000100,
	N    = 0b00001000,
	SW   = 0b00010000,
	SE   = 0b00100000,
	NW   = 0b01000000,
	NE   = 0b10000000,
	ALL  = 0b11111111,
}
#endregion

/// <summary>
/// <br>Collection of map generation algorithms</br>
/// <br>Naming conventions:</br>
/// <br>"coord" refers to  matrix coordinate in Vector2Int</br>
/// <br>"pos" refers to position in game in Vector3</br>
/// </summary>
public static class Algorithms
{
	#region Utility
	public static readonly Dictionary<Direction4, Vector2Int> Offset4 = new Dictionary<Direction4, Vector2Int>()
	{
		{ Direction4.LEFT, new Vector2Int(-1, 0) },
		{ Direction4.RIGHT, new Vector2Int(1,0) },
		{ Direction4.DOWN, new Vector2Int( 0,-1) },
		{ Direction4.UP, new Vector2Int( 0, 1) }
	};

	public static readonly Dictionary<Direction4, Vector3> Angle4 = new Dictionary<Direction4, Vector3>()
	{
		{Direction4.LEFT, new Vector3(0, -90, 0)},
		{Direction4.RIGHT, new Vector3(0, 90, 0)},
		{Direction4.DOWN, new Vector3(0, 180, 0)},
		{Direction4.UP, new Vector3(0, 0, 0)}
	};

	public static readonly Dictionary<Direction8, Vector2Int> Offset8 = new Dictionary<Direction8, Vector2Int>()
	{
		{ Direction8.W, new Vector2Int(-1, 0) },
		{ Direction8.E, new Vector2Int( 1, 0) },
		{ Direction8.S, new Vector2Int( 0,-1) },
		{ Direction8.N, new Vector2Int( 0, 1) },
		{ Direction8.SW, new Vector2Int(-1,-1) },
		{ Direction8.SE, new Vector2Int(-1, 1) },
		{ Direction8.NW, new Vector2Int( 1,-1) },
		{ Direction8.NE, new Vector2Int( 1, 1) }
	};

	public static Direction8 Dir4ToClosedDir8(Direction4 dir4)
    {
		Direction8 dir8 = (Direction8)dir4;
		if (dir8.HasFlag(Direction8.S) && dir8.HasFlag(Direction8.W))
			dir8 |= Direction8.SW;
		if (dir8.HasFlag(Direction8.S) && dir8.HasFlag(Direction8.E))
			dir8 |= Direction8.SE;
		if (dir8.HasFlag(Direction8.N) && dir8.HasFlag(Direction8.W))
			dir8 |= Direction8.NW;
		if (dir8.HasFlag(Direction8.N) && dir8.HasFlag(Direction8.E))
			dir8 |= Direction8.NE;
		return dir8;
    }

	public static Direction4 GetOpposite(Direction4 dir)
	{
		return dir switch
		{
			Direction4.NONE => Direction4.ALL,
			Direction4.LEFT => Direction4.RIGHT,
			Direction4.RIGHT => Direction4.LEFT,
			Direction4.DOWN => Direction4.UP,
			Direction4.UP => Direction4.DOWN,
			Direction4.ALL => Direction4.NONE,
			_ => Direction4.NONE,
		};
	}

	public static Vector3 Coord2PosXZ(Vector2Int coord)
    {
		return new Vector3(coord.x, 0, coord.y);
    }

	/// <summary>
	/// Return true if in map range
	/// </summary>
	public static bool CheckInMapRange(Vector2Int coord, Vector2Int mapSize, int border = 0)
	{
		return coord.x >= border && coord.x < mapSize.x - border && coord.y >= border && coord.y < mapSize.y - border;
	}

	/// <summary>
	/// Return true if outside of min separation.
	/// </summary>
	public static bool CheckMinSeparation(List<Vector2Int> positions, Vector2Int newPosition, float minSeparation)
    {
		if (positions != null)
		{
			foreach (Vector2Int pos in positions)
			{
				if (Vector2Int.Distance(pos, newPosition) < minSeparation)
					return false;
			}
		}
		return true;
	}

	public static bool CheckMinAstarSeparation(int[,] map, List<Vector2Int> positions, Vector2Int newPosition, float minSeparation)
    {
		if (positions != null)
		{
			bool[][] BAmap = Astar.ConvertToBoolArray(map);
			foreach (Vector2Int pos in positions)
			{
				List<Vector2Int> path = new Astar(BAmap, newPosition, pos).Result;
				if (path.Count < minSeparation)
					return false;
			}
		}
		return true;
    }

	public static List<DirectionalTile> GetNeighbours4(Vector2Int coord, Vector2Int mapSize)
    {
		return Offset4.Select(d => new DirectionalTile( new Vector2Int(coord.x + d.Value.x, coord.y + d.Value.y), d.Key))
		.Where(d => CheckInMapRange(d.Position, mapSize)).ToList();
	}

	public static List<Vector2Int> MatchFilter(int[,] map, int[,] filter)
    {
		Vector2Int mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));
		Vector2Int filterSize = new Vector2Int(filter.GetLength(0), filter.GetLength(1));
		List<Vector2Int> matchedBottomLeft = new List<Vector2Int>();

		bool[,] checkMap = new bool[mapSize.x, mapSize.y];
		for (int i = 0; i < mapSize.x - filterSize.x; i++)
			for (int j = 0; j < mapSize.y - filterSize.y; j++)
            {
				bool matched = true;
				for (int x = 0; x < filterSize.x; x++)
					for (int y = 0; y < filterSize.y; y++)
					{
						if (map[i + x, j + y] != filter[x, y] || checkMap[i + x, j + y])
							matched = false;
					}

				if (matched)
				{
					matchedBottomLeft.Add(new Vector2Int(i, j));
					for (int x = 0; x < filterSize.x; x++)
						for (int y = 0; y < filterSize.y; y++)
						{
							checkMap[i + x, j + y] = true;
						}
				}
			}
		return matchedBottomLeft;
	}

	public static int[,] StretchMap(int[,] oldMap, int stretchAxis, int stretchPosition, int stretchLength)
	{
		Vector2Int oldMapSize = new Vector2Int(oldMap.GetLength(0), oldMap.GetLength(1));
		Vector2Int newMapSize;
		int[,] newMap;
		if (stretchAxis == 0)
		{
			newMapSize = new Vector2Int(oldMap.GetLength(0) + stretchLength, oldMap.GetLength(1));
			newMap = new int[newMapSize.x, newMapSize.y];

			for (int j = 0; j < oldMapSize.y; j++)
			{
				for (int i = 0; i < stretchPosition; i++)
				{
					newMap[i, j] = oldMap[i, j];
				}

				for (int i = 0; i < stretchLength; i++)
                {
					if (oldMap[stretchPosition, j] == 0)
						newMap[stretchPosition + i, j] = 0;
					else
						newMap[stretchPosition + i, j] = 2;
				}

				for (int i = stretchPosition; i < oldMapSize.x; i++)
                {
					newMap[i + stretchLength, j] = oldMap[i, j];
				}
			}
		}
        else
        {
			newMapSize = new Vector2Int(oldMap.GetLength(0), oldMap.GetLength(1) + stretchLength);
			newMap = new int[newMapSize.x, newMapSize.y];

			for (int i = 0; i < oldMapSize.x; i++)
			{
				for (int j = 0; j < stretchPosition; j++)
				{
					newMap[i, j] = oldMap[i, j];
				}

				for (int j = 0; j < stretchLength; j++)
				{
					if (oldMap[stretchPosition, j] == 0)
						newMap[i, stretchPosition + j] = 0;
					else
						newMap[i, stretchPosition + j] = 2;
				}

				for (int j = stretchPosition; j < oldMapSize.y; j++)
				{
					newMap[i, j + stretchLength] = oldMap[i, j];
				}
			}
		}

		return newMap;
	}
	#endregion

	#region Perlin Noise
	// Doubled permutation to avoid overflow
	static readonly int[] p = { 151,160,137,91,90,15,
		131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
		190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
		88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
		77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
		102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
		135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
		5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
		223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
		129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
		251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
		49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
		138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
		151,160,137,91,90,15,
		131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
		190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
		88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
		77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
		102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
		135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
		5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
		223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
		129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
		251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
		49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
		138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
	};

	/// <summary>
	/// 6t^5 - 15t^4 + 10t^3
	/// </summary>
	static float Fade(float t)
	{
		return (float)(t * t * t * (t * (t * 6 - 15) + 10));
	}

	///<summary>
	/// Returning the dot product of gradient vector and the distance vector
	///</summary>
	static float GradientDotDistance(int hash, float x, float y)
	{
		switch (hash & 0x7) // bitwise AND operation of the binary value of hash and 7 (0000 0111) to give a value between 0 and 7
		{
			// gradient vectors are normalised to the same length
			case 0x0: return x * Mathf.Sqrt(2);
			case 0x1: return -x * Mathf.Sqrt(2);
			case 0x2: return x + y;
			case 0x3: return -x + y;
			case 0x4: return y * Mathf.Sqrt(2);
			case 0x5: return -y * Mathf.Sqrt(2);
			case 0x6: return x - y;
			case 0x7: return -x - y;
			default: return 0; // never happens
		}
	}

	public static float PerlinAtTile(float x, float y)
	{
		// Taking absolute value of the coordinates so negative values won't cause a glitch
		x = Mathf.Abs(x);
		y = Mathf.Abs(y);

		// Find the unit grid
		int xi = (int)x & 255;
		int yi = (int)y & 255;

		// Then find the distance
		float dx = x - Mathf.FloorToInt(x);
		float dy = y - Mathf.FloorToInt(y);

		//Prepare the values for interpolation
		//X-direction significance and Y-direction significance
		float xDirSig = Fade(dx);
		float yDirSig = Fade(dy);

		/* Then we need to get the gradient values for all 4 surrounding points
		The points are:
		(xi,yi)
		(xi,yi+1)
		(xi+1,yi)
		(xi+1,yi+1)
		First get the hash value, then interpret as vector

		Remember that, picking vector from hash and dot it with distance is integrated into a single function grad
		*/

		int hash_00, hash_10, hash_01, hash_11;
		hash_00 = p[p[xi] + yi];
		hash_10 = p[p[xi + 1] + yi];
		hash_01 = p[p[xi] + yi + 1];
		hash_11 = p[p[xi + 1] + yi + 1];

		float x0, x1;

		// The improved interpolation function is 6t^5 - 15t^4 + 10t^3
		// This function is used because it has 0 first and second order derivative at x=0 and x=1,
		// giving continuous values of first and second order derivatives
		x0 = Mathf.Lerp(GradientDotDistance(hash_00, dx, dy), GradientDotDistance(hash_10, dx - 1, dy), xDirSig);
		x1 = Mathf.Lerp(GradientDotDistance(hash_01, dx, dy - 1), GradientDotDistance(hash_11, dx - 1, dy - 1), xDirSig);

		return (Mathf.Lerp(x0, x1, yDirSig) + 1) / 2;
	}

	public static float[,] Perlin(Vector2Int mapSize, Vector2Int offset, int scale)
	{
		float[,] heights = new float[mapSize.x, mapSize.y];
		for (int x = 0; x < mapSize.x; x++)
			for (int y = 0; y < mapSize.y; y++)
			{
				float xCoord = (float)x / mapSize.x * scale + offset.x;
				float yCoord = (float)y / mapSize.y * scale + offset.y;
				heights[x, y] = PerlinAtTile(xCoord, yCoord);
			}
		return heights;
	}
	#endregion

	#region Regions & Connectors
	public static void FloodFill(Vector2Int coord, int[,] map, ref Region region)
	{
		if (region.Area.Count == 0)
			region.Area.Add(coord);

		Vector2Int mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));
		int tile = map[coord.x, coord.y];
		foreach (DirectionalTile p in GetNeighbours4(coord, mapSize))
		{
			bool IsNewTile = false;
			if (map[p.Position.x, p.Position.y] == tile && !region.Area.Contains(p.Position))
			{
				region.Area.Add(p.Position);
				IsNewTile = true;
			}

			// Outline from a different direction at the same location is kept, resulting in two outline entries in one coordinate
			if (map[p.Position.x, p.Position.y] != tile && !region.Outline.Contains(p))
			{
				region.Outline.Add(p);
			}

			if (IsNewTile)
			{
				FloodFill(p.Position, map, ref region);
			}
		}
	}

	/// <summary>
	/// Find all regions for a specific type of tiles.
	/// </summary>
	/// <param name="type"> Get all regions for this type of tiles </param>
	public static List<Region> GetRegions(int type, int[,] map)
	{
		Vector2Int mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));
		List<Region> regions = new List<Region>();

		Region region;
		Vector2Int coord;

		//examine each cell in the map
		for (int i = 0; i < mapSize.x; i++)
			for (int j = 0; j < mapSize.y; j++)
			{
				coord = new Vector2Int(i, j);

				if (map[i, j] == type && regions.Count(s => s.Area.Contains(coord)) == 0)
				{
					//if the coord is of correct value, and coord doesn't occur in the list of regions, then it is a new region
					region = new Region(new List<Vector2Int>(), new List<DirectionalTile>());

					//launch the recursive
					FloodFill(coord, map, ref region);
					regions.Add(region);
				}
			}
		return regions;
	}

	/// <summary>
	/// Attempts a connector. This function does not change the values of the map. If isOdd, then length is used as half length.
	/// </summary>
	/// <param name="type"> Connect regions of this type of tiles </param>
	static List<DirectionalTile> ConnectorAttempt(int type, DirectionalTile start, int[,] map, int minSegmentLength, int maxSegmentLength, int minTurnLimit, int maxTurnLimit, out bool isDeadEnd, bool isOdd = false)
	{
		Vector2Int mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));
		List<DirectionalTile> corridor = new List<DirectionalTile>();
		int turns = UnityEngine.Random.Range(minTurnLimit, maxTurnLimit);

		DirectionalTile current = start;
		corridor.Add(current);

		while (turns >= 0)
        {
			turns--;
			int segmentLength;
			if (isOdd)
				segmentLength = UnityEngine.Random.Range(minSegmentLength, maxSegmentLength) * 2 + 2;
			else
				segmentLength = UnityEngine.Random.Range(minSegmentLength, maxSegmentLength);

			while (segmentLength > 0)
            {
				// move forward in current direction
				segmentLength--;
				current.Position += Offset4[current.Direction];

				if (CheckInMapRange(current.Position, mapSize))
                {
					corridor.Add(current);

					// finish if a tile of the desired type is reached
					if (map[current.Position.x, current.Position.y] == type)
					{
						isDeadEnd = false;
						return corridor;
					}
				}
                else
				{
					isDeadEnd = true;
					return null;
				}
			}

			if (turns > 1)
			{
				// make a random turn
				List<Direction4> dirs = new List<Direction4>() { Direction4.LEFT, Direction4.RIGHT, Direction4.DOWN, Direction4.UP };
				dirs.Remove(current.Direction);
				dirs.Remove(GetOpposite(current.Direction));
				int randIndex = UnityEngine.Random.Range(0, dirs.Count);
				current.Direction = dirs[randIndex];
            }
		}

		isDeadEnd = true;
		return corridor;
    }

	/// <summary>
	/// Connect all regions for a specific type of tiles.
	/// </summary>
	/// <param name="type"> Connect all regions for this type of tiles </param>
	public static int[,] ConnectRegions(int type, int[,] map, int minSegmentLength, int maxSegmentLength, int minTurnLimit, int maxTurnLimit, out List<List<DirectionalTile>> connectorList, bool isOdd = false, bool infiniteAttempts = false, List<Region> unconnectedRegions = null)
    {
		if (unconnectedRegions == null)
			unconnectedRegions = GetRegions(type, map);
		List<Region> connectedRegions = new List<Region>();
		connectorList = new List<List<DirectionalTile>>();

		if (unconnectedRegions.Count > 0)
		{
			// start off a a random region
			int randIndex = UnityEngine.Random.Range(0, unconnectedRegions.Count);
			Region region = unconnectedRegions[randIndex];
			unconnectedRegions.Remove(region);
			connectedRegions.Add(region);

			int attempts = 1000000;
			int counter = 0;
			while (unconnectedRegions.Count > 0 && (counter++ < attempts || infiniteAttempts))
			{
				// start off a random point on the outline of a random connected region
				randIndex = UnityEngine.Random.Range(0, connectedRegions.Count);
				region = connectedRegions[randIndex];
				randIndex = UnityEngine.Random.Range(0, region.Outline.Count);
				DirectionalTile start = region.Outline[randIndex];

                List<DirectionalTile> connector = ConnectorAttempt(type, start, map, minSegmentLength, maxSegmentLength, minTurnLimit, maxTurnLimit, out bool isDeadEnd, isOdd);

                if (connector != null && !isDeadEnd)
                {
					// check if connector leads to another region
					for (int i = 0; i < unconnectedRegions.Count; i++)
                    {
                        if (unconnectedRegions[i].Area.Contains(connector.Last().Position))
						{
							// draw connector onto map
							foreach (DirectionalTile tile in connector)
								map[tile.Position.x, tile.Position.y] = type;

							// update connected and unconnected regions and corridor list
							connectedRegions.Add(unconnectedRegions[i]);
							unconnectedRegions.RemoveAt(i);
							connectorList.Add(connector);

							break;
                        }
                    }
                }
			}
		}
		
		return map;
    }

	/// <summary>
	/// Open up all tiles neighboured by 3 or more different tiles
	/// </summary>
	/// <param name="type"> Open up dead ends of this type of tiles </param>
	public static int[,] OpenDeadEnds(int type, int[,] map)
	{
		Vector2Int mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));
		//examine each cell in the map
		for (int i = 0; i < mapSize.x; i++)
			for (int j = 0; j < mapSize.y; j++)
			{
				// Note that we do not want to open neighbours on the edge of the map, but they still count as closed neighbours
				List<Vector2Int> openableClosedNeighbours = new List<Vector2Int>();
				int closedNeighbourCount = 0;
				foreach(Vector2Int offset in Offset4.Values)
                {
					Vector2Int neighbour = new Vector2Int(i, j) + offset;
					if (CheckInMapRange(neighbour, mapSize) && map[neighbour.x, neighbour.y] != type)
					{
						closedNeighbourCount++;
						if (CheckInMapRange(neighbour, mapSize, 1))
						{
							openableClosedNeighbours.Add(neighbour);
						}
					}
                }

				if (closedNeighbourCount > 2)
                {
					int randIndex = UnityEngine.Random.Range(0, openableClosedNeighbours.Count);
					Vector2Int randNeighbour = openableClosedNeighbours[randIndex];
					map[randNeighbour.x, randNeighbour.y] = type;
				}
			}

		return map;
	}
	#endregion

    #region Cellular Automata
    static int[,] InitCellularMap(Vector2Int mapSize, int[,][] setting)
	{
		int[,] map = new int[mapSize.x, mapSize.y];

		for (int i = 0; i < mapSize.x; i++)
			for (int j = 0; j < mapSize.y; j++)
			{
				map[i, j] = UnityEngine.Random.Range(1, 101) < setting[i, j][0] ? 1 : 0;
			}

		return map;
	}

	/// <param name="setting"> [initChance, birthLimit, deathLimit] </param>
	static int[,] CellularRun(int[,] oldMap, Vector2Int mapSize, int[,][] setting)
	{
		int[,] newMap = new int[mapSize.x, mapSize.y];
		BoundsInt myB = new BoundsInt(-1, -1, 0, 3, 3, 1);

		for (int i = 0; i < mapSize.x; i++)
			for (int j = 0; j < mapSize.y; j++)
			{
				int neighbourCount = 0;
				foreach (var b in myB.allPositionsWithin)
				{
					if (b.x == 0 && b.y == 0) continue;

					// Boundary counts as valid neighbour
					neighbourCount += CheckInMapRange(new Vector2Int(i + b.x, j + b.y), mapSize) ? oldMap[i + b.x, j + b.y] : 1;
				}

				if (oldMap[i, j] == 0)
					newMap[i, j] = neighbourCount > setting[i, j][1] ? 1 : 0;

				if (oldMap[i, j] == 1)
					newMap[i, j] = neighbourCount < setting[i, j][2] ? 0 : 1;
			}
		return newMap;
	}

	public static int[,] CellularSmooth(int[,] oldMap)
    {
		return oldMap;
    }

	public static int[,] CellularFillHole(int[,] oldMap)
	{
		return oldMap;
	}

	/// <summary>
	/// CA with possibly different settings for each tile.
	/// </summary>
	/// <param name="setting"> [initChance, birthLimit, deathLimit] </param>
	public static int[,] Cellular(Vector2Int mapSize, int[,][] setting, int epoch)
	{
		int[,] map = InitCellularMap(mapSize, setting);

		for (int i = 0; i < epoch; i++)
		{
			map = CellularRun(map, mapSize, setting);
		}
		return map;
	}
	#endregion

	#region Recursive Backtracker
	/// <summary>
	/// Return neighbours that have walls on all four sides
	/// </summary>
	static List<DirectionalTile> GetUnvisitedNeighbours(Vector2Int coord, Direction4[,] maze)
	{
		Vector2Int mapSize = new Vector2Int(maze.GetLength(0), maze.GetLength(1));
		List<DirectionalTile> neighbs = GetNeighbours4(coord, mapSize);
		List<DirectionalTile> unvisitedNeighbs = new List<DirectionalTile>();
		foreach (DirectionalTile neighb in neighbs)
        {
			Vector2Int ncoord = neighb.Position;
            if (maze[ncoord.x, ncoord.y].Equals(Direction4.ALL))
            {
				unvisitedNeighbs.Add(neighb);
			}
        }

		return unvisitedNeighbs;
	}

	static Direction4[,] InitRBMaze(Vector2Int mapSize)
	{
		Direction4[,] maze = new Direction4[mapSize.x, mapSize.y];
		for (int i = 0; i < mapSize.x; i++)
			for (int j = 0; j < mapSize.y; j++)
			{
				maze[i, j] = Direction4.ALL;
			}

		return maze;
	}

	/// <summary>
	/// Generate RB maze where every tile is open and walls exist between tiles
	/// </summary>
	public static Direction4[,] RecursiveBacktracker(Vector2Int mapSize)
	{
		Direction4[,] maze = InitRBMaze(mapSize);

		Stack<Vector2Int> posStack = new Stack<Vector2Int>();
		Vector2Int initPos = new Vector2Int(UnityEngine.Random.Range(0, mapSize.x), UnityEngine.Random.Range(0, mapSize.y));
		posStack.Push(initPos);

		while (posStack.Count > 0)
		{
			Vector2Int currentPos = posStack.Pop();
			List<DirectionalTile> neighbs = GetUnvisitedNeighbours(currentPos, maze);

			if (neighbs.Count > 0)
			{
				posStack.Push(currentPos);

				int randIndex = UnityEngine.Random.Range(0, neighbs.Count);
				DirectionalTile randNeighb = neighbs[randIndex];

				Vector2Int nPos = randNeighb.Position;
				maze[currentPos.x, currentPos.y] &= ~randNeighb.Direction;
				maze[nPos.x, nPos.y] &= ~GetOpposite(randNeighb.Direction);

				posStack.Push(nPos);
			}
		}

		return maze;
	}

	/// <summary>
	/// Fatten a maze so that walls now occupies an entire tile
	/// </summary>
	public static int[,] FattenMaze(Direction4[,] maze)
    {
		Vector2Int mazeSize = new Vector2Int(maze.GetLength(0), maze.GetLength(1));
		int[,] map = new int[mazeSize.x * 2 + 1, mazeSize.y * 2 + 1];
		Vector2Int mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));

		// write open paths with 1
		for (int i = 0; i < mazeSize.x; i++)
			for (int j = 0; j < mazeSize.y; j++)
			{
				Vector2Int mapCoord = new Vector2Int(i * 2 + 1, j * 2 + 1);
				map[mapCoord.x, mapCoord.y] = 1;
				foreach (Direction4 dir in Enum.GetValues(typeof(Direction4)))
				{
					if (Offset4.ContainsKey(dir) && !maze[i, j].HasFlag(dir))
					{
						Vector2Int mapNeighbourCoord = mapCoord + Offset4[dir];

						if (CheckInMapRange(mapNeighbourCoord, mapSize))
						{
							map[mapNeighbourCoord.x, mapNeighbourCoord.y] = 1;
						}
					}
				}
			}

		// reverse
		for (int i = 0; i < mapSize.x; i++)
			for (int j = 0; j < mapSize.y; j++)
			{
				map[i, j] = map[i, j] == 1 ? 0 : 1;
			}
		
		return map;
	}

	/// <summary>
	/// Insert rooms into a maze. Rooms are always odd-sized on odd coords.
	/// They are also walled-off and need to be opened up, probably by ConnectRegions and/or OpenDeadEnds. NIR stands for Non Intersectable Regions.
	/// </summary>
	public static int[,] RoomInMaze(int[,] map, int minHalfLength, int maxHalfLength, int attempts, List<Region> existingNIR, out List<Region> allNIR, out List<RectRoom> newRooms, int border = 0)
	{
		Vector2Int mapSize = new Vector2Int(map.GetLength(0), map.GetLength(1));
		allNIR = existingNIR;
		newRooms = new List<RectRoom>();
		int counter = 0;

		while (counter++ < attempts)
		{
			// Pick a random room size. The funny math here does two things:
			// - It makes sure rooms are odd-sized to line up with maze.
			// - It avoids creating rooms that are too rectangular: too tall and
			//   narrow or too wide and flat.
			// TODO: This isn't very flexible or tunable. Do something better here.
			Vector2Int roomSize = new Vector2Int(UnityEngine.Random.Range(minHalfLength, maxHalfLength),
				UnityEngine.Random.Range(minHalfLength, maxHalfLength)) * 2 + new Vector2Int(1, 1);
			Vector2Int minBLCoord = new Vector2Int(border, border);
			Vector2Int maxBLCoord = mapSize - roomSize - new Vector2Int(border, border);
			Vector2Int bottomLeft = new Vector2Int(UnityEngine.Random.Range(minBLCoord.x, maxBLCoord.x),
				UnityEngine.Random.Range(minBLCoord.y, maxBLCoord.y)) / 2 * 2 + new Vector2Int(1, 1);
			Vector2Int topRight = bottomLeft + roomSize;

			Region newRegion = new Region(new List<Vector2Int>(), new List<DirectionalTile>());
			bool canPlace = true;

			for (int x = bottomLeft.x - 1; x < topRight.x + 1; x++)
				for (int y = bottomLeft.y - 1; y < topRight.y + 1; y++)
				{
					Vector2Int tile = new Vector2Int(x, y);

					foreach (Region room in allNIR)
						if (room.Area.Contains(tile))
						{
							canPlace = false;
							break;
						}

					if (!canPlace)
						break;

					if (x < bottomLeft.x)
						newRegion.Outline.Add(new DirectionalTile(tile, Direction4.LEFT));
					else if (x >= topRight.x)
						newRegion.Outline.Add(new DirectionalTile(tile, Direction4.RIGHT));
					else if (y < bottomLeft.y)
						newRegion.Outline.Add(new DirectionalTile(tile, Direction4.DOWN));
					else if (y >= topRight.y)
						newRegion.Outline.Add(new DirectionalTile(tile, Direction4.UP));
					else
						newRegion.Area.Add(tile);
				}

			if (canPlace)
            {
				allNIR.Add(newRegion);
				newRooms.Add(new RectRoom(bottomLeft, roomSize, newRegion));
				foreach (Vector2Int tile in newRegion.Area)
					map[tile.x, tile.y] = 0;
				foreach (DirectionalTile dirTile in newRegion.Outline)
					map[dirTile.Position.x, dirTile.Position.y] = 1;
			}
		}
		return map;
	}
	#endregion

	#region Single Passway
	public static int[,] SinglePassway(Vector2Int mapSize, out Vector2Int startCoord, out Vector2Int endCoord, out List<DirectionalTile> passway, int minHalfSegmentLength, int maxHalfSegmentLength, int minTurnLimit, int maxTurnLimit)
	{
		int[,] map;

		do
		{
			map = new int[mapSize.x, mapSize.y];

			for (int i = 0; i < mapSize.x; i++)
				for (int j = 1; j < mapSize.y - 1; j++)
					map[i, j] = 1;

			map = ConnectRegions(0, map, minHalfSegmentLength, maxHalfSegmentLength, minTurnLimit, maxTurnLimit, out List<List<DirectionalTile>> passwayList, true, true);
			passway = passwayList[0];
		} while (passway[0].Position.y > passway.Last().Position.y);

		startCoord = passway[0].Position + new Vector2Int(0, -1);
		endCoord = passway.Last().Position;

		return map;
	}
    #endregion

    #region Border
	public static int[,] Border(Vector2Int mapSize, int thickness)
    {
		int[,] map = new int[mapSize.x, mapSize.y];
		
		for (int i = thickness; i < mapSize.x - thickness; i++)
			for (int j = thickness; j < mapSize.y - thickness; j++)
				map[i, j] = 1;

		return map;
	}
    #endregion
}
