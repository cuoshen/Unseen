using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColumnSize
{
    _1x1,
    _2x2
}

public static class ColumnarMazeMapper
{
    public static ColumnSize[,] CreateMap(Vector2Int mapSize, float _2x2ColumnChance)
    {
        ColumnSize[,] maze = new ColumnSize[mapSize.x, mapSize.y];

        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                if (Random.Range(0f, 1f) < _2x2ColumnChance)
                {
                    maze[i, j] = ColumnSize._2x2;
                }
                else
                {
                    maze[i, j] = ColumnSize._1x1;
                }
            }
        }

        return maze;
    }
}
