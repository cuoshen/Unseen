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
    public static ColumnSize[,] CreateMap(System.Random rng, int width, int height)
    {
        ColumnSize[,] maze = new ColumnSize[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (rng.Next(5) > 1)
                {
                    maze[i, j] = ColumnSize._1x1;
                }
                else
                {
                    maze[i, j] = ColumnSize._2x2;
                }
            }
        }

        return maze;
    }
}
