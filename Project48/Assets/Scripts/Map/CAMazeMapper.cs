using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CAMazeMapper
{
	/// <param name="setting"> setting by [initChance, birthLimit, deathLimit] </param>
	public static int[,] CellularRun(int[,] oldMap, Vector2Int mapSize, int[] setting)
	{
		int[,] newMap = new int[mapSize.x, mapSize.y];
		int neighb;
		BoundsInt myB = new BoundsInt(-1, -1, 0, 3, 3, 1);

		for (int x = 0; x < mapSize.x; x++)
		{
			for (int y = 0; y < mapSize.y; y++)
			{
				neighb = 0;
				foreach (var b in myB.allPositionsWithin)
				{
					if (b.x == 0 && b.y == 0) continue;

					if (x + b.x >= 0 && x + b.x < mapSize.x && y + b.y >= 0 && y + b.y < mapSize.y)
						neighb += oldMap[x + b.x, y + b.y];
					else neighb++;
				}

				if (oldMap[x, y] == 0)
				{
					if (neighb > setting[1])
						newMap[x, y] = 1;
					else newMap[x, y] = 0;
				}

				if (oldMap[x, y] == 1)
				{
					if (neighb < setting[2])
						newMap[x, y] = 0;
					else newMap[x, y] = 1;
				}
			}
		}
		return newMap;
	}

	/// <param name="setting"> setting by [initChance, birthLimit, deathLimit] </param>
	public static int[,] CreateMap(Vector2Int mapSize, int[] setting, int epoch)
	{
		int[,] map = new int[mapSize.x, mapSize.y];

		for (int x = 0; x < mapSize.x; x++)
		{
			for (int y = 0; y < mapSize.y; y++)
			{
				map[x, y] = Random.Range(1, 101) < setting[0] ? 1 : 0;
			}
		}

		for (int i = 0; i < epoch; i++)
		{
			map = CellularRun(map, mapSize, setting);
		}
		return map;
	}
}
