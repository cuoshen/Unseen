/*
Unity C# Port of Andrea Giammarchi's JavaScript A* algorithm (http://devpro.it/javascript_id_137.html)

Usage:

0 = walkable;
1 = wall;
 
int[][] map = new int[][]
{
    new int[] {0, 0, 1, 0, 0 },
    new int[] {0, 0, 1, 0, 0 },
    new int[] {0, 0, 1, 0, 0 },
    new int[] {0, 0, 0, 0, 0 },
};

Vector2Int start = new Vector2Int
{
    x = 0,
    y = 0,
};

Vector2Int end = new Vector2Int
{
    x = 4,
    y = 3,
};

List<Vector2Int> result = new Astar(Astar.ConvertToBoolArray(map), start, end).Result;
*/

using System;
using System.Collections.Generic;
using UnityEngine;

public class Astar
{
    public List<Vector2Int> Result { private set; get; }

    private delegate Node[] Find(bool bN, bool bS, bool bE, bool bW, int N, int S, int E, int W, bool[][] grid, int rows, int cols, Node[] result, int i);
    private delegate double Distance(Node start, Node end);

    private readonly Find m_Find;

    public enum Type
    {
        Manhattan,
        Diagonal,
        DiagonalFree,
        Euclidean,
        EuclideanFree
    }

    private class Node
    {
        public int x;
        public int y;
        public Node p;
        public double g;
        public double f;
        public int v;

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    private Node[] DiagonalSuccessors(bool bN, bool bS, bool bE, bool bW, int N, int S, int E, int W, bool[][] grid, int rows, int cols, Node[] result, int i)
    {
        if (bN)
        {
            if (bE && !grid[N][E])
            {
                result[i++] = new Node(E, N);
            }
            if (bW && !grid[N][W])
            {
                result[i++] = new Node(W, N);
            }
        }
        if (bS)
        {
            if (bE && !grid[S][E])
            {
                result[i++] = new Node(E, S);
            }
            if (bW && !grid[S][W])
            {
                result[i++] = new Node(W, S);
            }
        }
        return result;
    }

    private Node[] DiagonalSuccessorsFree(bool bN, bool bS, bool bE, bool bW, int N, int S, int E, int W, bool[][] grid, int rows, int cols, Node[] result, int i)
    {
        bN = N > -1;
        bS = S < rows;
        bE = E < cols;
        bW = W > -1;

        if (bE)
        {
            if (bN && !grid[N][E])
            {
                result[i++] = new Node(E, N);
            }
            if (bS && !grid[S][E])
            {
                result[i++] = new Node(E, S);
            }
        }
        if (bW)
        {
            if (bN && !grid[N][W])
            {
                result[i++] = new Node(W, N);
            }
            if (bS && !grid[S][W])
            {
                result[i++] = new Node(W, S);
            }
        }
        return result;
    }

    private Node[] NothingToDo(bool bN, bool bS, bool bE, bool bW, int N, int S, int E, int W, bool[][] grid, int rows, int cols, Node[] result, int i)
    {
        return result;
    }

    private Node[] Successors(int x, int y, bool[][] grid, int rows, int cols)
    {
        int N = y - 1;
        int S = y + 1;
        int E = x + 1;
        int W = x - 1;

        bool bN = N > -1 && !grid[N][x];
        bool bS = S < rows && !grid[S][x];
        bool bE = E < cols && !grid[y][E];
        bool bW = W > -1 && !grid[y][W];

        Node[] result = new Node[4];
        int i = 0;

        if (bN)
        {
            result[i++] = new Node(x, N);
        }
        if (bE)
        {
            result[i++] = new Node(E, y);
        }
        if (bS)
        {
            result[i++] = new Node(x, S);
        }
        if (bW)
        {
            result[i++] = new Node(W, y);
        }

        return m_Find(bN, bS, bE, bW, N, S, E, W, grid, rows, cols, result, i);
    }

    private double Diagonal(Node start, Node end)
    {
        return Math.Max(Math.Abs(start.x - end.x), Math.Abs(start.y - end.y));
    }

    private double Euclidean(Node start, Node end)
    {
        var x = start.x - end.x;
        var y = start.y - end.y;

        return Math.Sqrt(x * x + y * y);
    }

    private double Manhattan(Node start, Node end)
    {
        return Math.Abs(start.x - end.x) + Math.Abs(start.y - end.y);
    }

    public static bool[][] ConvertToBoolArray(int[][] grid)
    {
        bool[][] arr = new bool[grid.Length][];

        for (int y = 0; y < grid.Length; y++)
        {
            arr[y] = new bool[grid[0].Length];

            for (int x = 0; x < arr[y].Length; x++)
            {
                arr[y][x] = grid[y][x] == 1;
            }
        }

        return arr;
    }

    public static bool[][] ConvertToBoolArray(int[,] grid)
    {
        bool[][] arr = new bool[grid.GetLength(1)][];

        for (int y = 0; y < grid.GetLength(1); y++)
        {
            arr[y] = new bool[grid.GetLength(0)];

            for (int x = 0; x < arr[y].Length; x++)
            {
                arr[y][x] = grid[x,y] == 1;
            }
        }

        return arr;
    }

    public Astar(bool[][] grid, Vector2Int s, Vector2Int e, Type type = Type.Manhattan)
    {
        int cols = grid[0].Length;
        int rows = grid.Length;
        int limit = cols * rows;

        Result = new List<Vector2Int>();

        Dictionary<int, int> list = new Dictionary<int, int>();
        List<Node> open = new List<Node>(new Node[limit]);

        Node node = new Node(s.x, s.y)
        {
            f = 0,
            g = 0,
            v = s.x + s.y * cols
        };

        open.Insert(0, node);

        int length = 1;
        Node adj;

        int i;
        int j;
        double max;
        int min;

        Node current;
        Node[] next;
        Distance distance;

        Node end = new Node(e.x, e.y)
        {
            v = e.x + e.y * cols
        };

        if (type == Type.Diagonal)
        {
            m_Find = DiagonalSuccessors;
            distance = Diagonal;
        }
        else if (type == Type.DiagonalFree)
        {
            m_Find = DiagonalSuccessorsFree;
            distance = Diagonal;
        }
        else if (type == Type.Euclidean)
        {
            m_Find = DiagonalSuccessors;
            distance = Euclidean;
        }
        else if (type == Type.EuclideanFree)
        {
            m_Find = DiagonalSuccessorsFree;
            distance = Euclidean;
        }
        else
        {
            m_Find = NothingToDo;
            distance = Manhattan;
        }

        do
        {
            max = limit;
            min = 0;

            for (i = 0; i < length; i++)
            {
                double f = open[i].f;

                if (f < max)
                {
                    max = f;
                    min = i;
                }
            }

            current = open[min];
            open.RemoveRange(min, 1);

            if (current.v != end.v)
            {
                --length;
                next = Successors(current.x, current.y, grid, rows, cols);

                for (i = 0, j = next.Length; i < j; ++i)
                {
                    if (next[i] == null)
                    {
                        continue;
                    }

                    (adj = next[i]).p = current;
                    adj.f = adj.g = 0;
                    adj.v = adj.x + adj.y * cols;

                    if (!list.ContainsKey(adj.v))
                    {
                        adj.f = (adj.g = current.g + distance(adj, current)) + distance(adj, end);
                        open[length++] = adj;
                        list[adj.v] = 1;
                    }
                }
            }
            else
            {
                i = length = 0;

                do
                {
                    Vector2Int point = new Vector2Int(current.x, current.y);
                    Result.Add(point);
                }
                while ((current = current.p) != null);

                Result.Reverse();
            }
        }
        while (length != 0);
    }
}