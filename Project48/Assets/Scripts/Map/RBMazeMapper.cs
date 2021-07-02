using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Flags]
public enum WallState
{
    // 0000 -> NO WALLS
    // 1111 -> LEFT,RIGHT,UP,DOWN
    LEFT = 1, // 0001
    RIGHT = 2, // 0010
    UP = 4, // 0100
    DOWN = 8, // 1000

    VISITED = 128, // 1000 0000
}

public struct Neighbour
{
    public Vector2Int Position;
    public WallState SharedWall;
}

public static class RBMazeMapper
{

    static WallState GetOppositeWall(WallState wall)
    {
        switch (wall)
        {
            case WallState.RIGHT: return WallState.LEFT;
            case WallState.LEFT: return WallState.RIGHT;
            case WallState.UP: return WallState.DOWN;
            case WallState.DOWN: return WallState.UP;
            default: return WallState.LEFT;
        }
    }

    static WallState[,] ApplyRecursiveBacktracker(WallState[,] maze)
    {
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);
        Stack<Vector2Int> posStack = new Stack<Vector2Int>();
        Vector2Int initPos = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
        Vector2Int position = initPos;

        maze[position.x, position.y] |= WallState.VISITED;  // 1000 1111
        posStack.Push(position);

        while (posStack.Count > 0)
        {
            Vector2Int current = posStack.Pop();
            List<Neighbour> neighbours = GetUnvisitedNeighbours(current, maze);

            if (neighbours.Count > 0)
            {
                posStack.Push(current);

                int randIndex = UnityEngine.Random.Range(0, neighbours.Count);
                Neighbour randomNeighbour = neighbours[randIndex];

                Vector2Int nPos = randomNeighbour.Position;
                maze[current.x, current.y] &= ~randomNeighbour.SharedWall;
                maze[nPos.x, nPos.y] &= ~GetOppositeWall(randomNeighbour.SharedWall);
                maze[nPos.x, nPos.y] |= WallState.VISITED;

                posStack.Push(nPos);
            }
        }

        return maze;
    }

    static List<Neighbour> GetUnvisitedNeighbours(Vector2Int p, WallState[,] maze)
    {
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);
        List<Neighbour> list = new List<Neighbour>();

        if (p.x > 0) // LEFT
        {
            if (!maze[p.x - 1, p.y].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    Position = new Vector2Int(p.x - 1, p.y),
                    SharedWall = WallState.LEFT
                });
            }
        }

        if (p.x < width - 1) // RIGHT
        {
            if (!maze[p.x + 1, p.y].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    Position = new Vector2Int(p.x + 1, p.y),
                    SharedWall = WallState.RIGHT
                });
            }
        }

        if (p.y > 0) // DOWN
        {
            if (!maze[p.x, p.y - 1].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    Position = new Vector2Int(p.x, p.y - 1),
                    SharedWall = WallState.DOWN
                });
            }
        }

        if (p.y < height - 1) // UP
        {
            if (!maze[p.x, p.y + 1].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    Position = new Vector2Int(p.x, p.y + 1),
                    SharedWall = WallState.UP
                });
            }
        }

        return list;
    }

    public static WallState[,] CreateMap(Vector2Int mapSize)
    {
        WallState[,] maze = new WallState[mapSize.x, mapSize.y];
        WallState initial = WallState.RIGHT | WallState.LEFT | WallState.UP | WallState.DOWN;
        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                maze[i, j] = initial;  // 1111
            }
        }
        
        return ApplyRecursiveBacktracker(maze);
    }
}
