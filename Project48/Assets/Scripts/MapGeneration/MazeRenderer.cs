using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MazeRenderer : MonoBehaviour
{
    int width = 10;
    int height = 10;
    int level = 3;

    [SerializeField]
    Transform wallPrefab = null;
    [SerializeField]
    Transform floorPrefab = null;

    // Start is called before the first frame update
    void Start()
    {
        DrawLevel();
    }

    void DrawLevel()
    {
        Vector3 lastEndPos = new Vector3(0, 0, 0);

        for (int i = 0; i < level; i++)
        {
            WallState[,] maze = RBMazeGenerator.Generate(width, height);

            System.Random rng = new System.Random(/*seed*/);
            Vector2Int startPos = new Vector2Int(rng.Next(width), 0);
            Vector2Int endPos = new Vector2Int(rng.Next(width), height - 1);

            DrawRBMaze(lastEndPos, startPos, endPos, maze);
            lastEndPos += new Vector3(endPos.x - startPos.x, 0, endPos.y - startPos.y + 1);
        }
    }

    void DrawRBMaze(Vector3 lastEndPos, Vector2Int startPos, Vector2Int endPos, WallState[,] maze)
    {
        GameObject mazeGO = new GameObject("RB Maze");
        Transform mazeTransform = mazeGO.transform;
        mazeTransform.SetParent(transform);
        mazeTransform.localPosition = new Vector3(width / 2 - startPos.x, 0, height / 2 - startPos.y) + lastEndPos;

        Transform floor = Instantiate(floorPrefab, mazeTransform);
        floor.localPosition = new Vector3(-0.5f, 0, -0.5f);
        floor.localScale = new Vector3(width*0.1f, 1, height*0.1f);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                WallState cell = maze[i, j];
                Vector3 position = new Vector3(-width / 2 + i, 0, -height / 2 + j);

                if (cell.HasFlag(WallState.UP) && new Vector2Int(i,j) != endPos)
                {
                    Vector3 newPos = position + new Vector3(0, 0.5f, 0.5f);
                    if(!Physics.CheckSphere(mazeTransform.position + newPos, 0.4f))
                    {
                        Transform newWall = Instantiate(wallPrefab, mazeTransform);
                        newWall.position = mazeTransform.position + newPos;
                    }
                }

                if (cell.HasFlag(WallState.LEFT))
                {
                    Vector3 newPos = position + new Vector3(-0.5f, 0.5f, 0);
                    if (!Physics.CheckSphere(mazeTransform.position + newPos, 0.4f))
                    {
                        Transform newWall = Instantiate(wallPrefab, mazeTransform);
                        newWall.position = mazeTransform.position + newPos;
                        newWall.eulerAngles = new Vector3(0, 90, 0);
                    }
                }

                if (j == 0)
                {
                    if (cell.HasFlag(WallState.DOWN) && new Vector2Int(i, j) != startPos)
                    {
                        Vector3 newPos = position + new Vector3(0, 0.5f, -0.5f);
                        if (!Physics.CheckSphere(mazeTransform.position + newPos, 0.4f))
                        {
                            Transform newWall = Instantiate(wallPrefab, mazeTransform);
                            newWall.position = mazeTransform.position + newPos;
                        }
                    }
                }

                if (i == width - 1)
                {
                    if (cell.HasFlag(WallState.RIGHT))
                    {
                        Vector3 newPos = position + new Vector3(0.5f, 0.5f, 0);
                        if (!Physics.CheckSphere(mazeTransform.position + newPos, 0.4f))
                        {
                            Transform newWall = Instantiate(wallPrefab, mazeTransform);
                            newWall.position = mazeTransform.position + newPos;
                            newWall.eulerAngles = new Vector3(0, 90, 0);
                        }
                    }
                }
            }

        }

    }
}
