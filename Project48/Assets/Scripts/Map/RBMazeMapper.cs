using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// <br>Stores directions in the form of flags.</br>
/// <br>LEFT: 1, RIGHT: 2, UP: 4, DOWN: 8</br>
/// </summary>
[Flags]
public enum Directions
{
    NONE  = 0b0000,
    LEFT  = 0b0001,
    RIGHT = 0b0010,
    UP    = 0b0100,
    DOWN  = 0b1000,
    ALL   = 0b1111,
}

public struct Neighbour
{
    public Vector2Int Position;
    public Directions SharedWall;
}

public static class RBMazeMapper
{

    static Directions GetOpposite(Directions dir)
    {
        return dir switch
        {
            Directions.NONE => Directions.ALL,
            Directions.LEFT => Directions.RIGHT,
            Directions.RIGHT => Directions.LEFT,
            Directions.UP => Directions.DOWN,
            Directions.DOWN => Directions.UP,
            Directions.ALL => Directions.NONE,
            _ => Directions.ALL,
        };
    }

    static Directions[,] ApplyRecursiveBacktracker(Directions[,] maze)
    {
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);

        Stack<Vector2Int> posStack = new Stack<Vector2Int>();
        Vector2Int initPos = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
        posStack.Push(initPos);

        while (posStack.Count > 0)
        {
            Vector2Int currentPos = posStack.Pop();
            List<Neighbour> neighbours = GetUnvisitedNeighbours(currentPos, maze);

            if (neighbours.Count > 0)
            {
                posStack.Push(currentPos);

                int randIndex = UnityEngine.Random.Range(0, neighbours.Count);
                Neighbour randomNeighbour = neighbours[randIndex];

                Vector2Int nPos = randomNeighbour.Position;
                maze[currentPos.x, currentPos.y] &= ~randomNeighbour.SharedWall;
                maze[nPos.x, nPos.y] &= ~GetOpposite(randomNeighbour.SharedWall);

                posStack.Push(nPos);
            }
        }

        return maze;
    }

    /// <summary>
    /// Return neighbours that have walls on all four sides
    /// </summary>
    static List<Neighbour> GetUnvisitedNeighbours(Vector2Int position, Directions[,] maze)
    {
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);
        List<Neighbour> list = new List<Neighbour>();

        if (position.x > 0) // LEFT
        {
            if (maze[position.x - 1, position.y].Equals(Directions.ALL))
            {
                list.Add(new Neighbour
                {
                    Position = new Vector2Int(position.x - 1, position.y),
                    SharedWall = Directions.LEFT
                });
            }
        }

        if (position.x < width - 1) // RIGHT
        {
            if (maze[position.x + 1, position.y].Equals(Directions.ALL))
            {
                list.Add(new Neighbour
                {
                    Position = new Vector2Int(position.x + 1, position.y),
                    SharedWall = Directions.RIGHT
                });
            }
        }

        if (position.y > 0) // DOWN
        {
            if (maze[position.x, position.y - 1].Equals(Directions.ALL))
            {
                list.Add(new Neighbour
                {
                    Position = new Vector2Int(position.x, position.y - 1),
                    SharedWall = Directions.DOWN
                });
            }
        }

        if (position.y < height - 1) // UP
        {
            if (maze[position.x, position.y + 1].Equals(Directions.ALL))
            {
                list.Add(new Neighbour
                {
                    Position = new Vector2Int(position.x, position.y + 1),
                    SharedWall = Directions.UP
                });
            }
        }

        return list;
    }

    public static Directions[,] CreateMap(Vector2Int mapSize)
    {
        Directions[,] maze = new Directions[mapSize.x, mapSize.y];
        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                maze[i, j] = Directions.ALL;
            }
        }
        
        return ApplyRecursiveBacktracker(maze);
    }
}
