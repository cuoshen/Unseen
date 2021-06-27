using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MazeGenerator : MonoBehaviour
{
    int width = 10;
    int height = 10;
    int level = 3;

    [SerializeField]
    Transform wallPrefab = null;
    [SerializeField]
    Transform floorPrefab = null;
    [SerializeField]
    Transform lightPrefab = null;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("GenerateLevel");
    }

    IEnumerator GenerateLevel()
    {
        Vector3 lastEndPos = new Vector3(0, 0, 0);
        System.Random rng = new System.Random(/*seed*/);

        for (int i = 0; i < level; i++)
        {
            WallState[,] maze = RBMazeMapper.CreateMap(rng, width, height);

            Vector2Int startPos = new Vector2Int(rng.Next(width), 0);
            Vector2Int endPos = new Vector2Int(rng.Next(width), height - 1);

            GenerateRBMaze(lastEndPos, startPos, endPos, maze);
            lastEndPos += new Vector3(endPos.x - startPos.x, 0, endPos.y - startPos.y + 1);
            yield return null; // Physics.CheckSphere cannot detect colliders generated in the same tick
        }
    }

    /// <summary>
    /// Generate an individual rectangular RB maze, its entrance connected to the exit of the previous maze
    /// </summary>
    /// <param name="lastEndPos"> Global position of the exit of previous maze</param>
    /// <param name="startPos"> Local position of the entrance of current maze</param>
    /// <param name="endPos"> Local position of  the exit of current maze</param>
    /// <param name="maze"> Matrix representing current maze</param>
    void GenerateRBMaze(Vector3 lastEndPos, Vector2Int startPos, Vector2Int endPos, WallState[,] maze)
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
                    if(!Physics.CheckSphere(mazeTransform.position + newPos, 0.1f))
                    {
                        Transform newWall = Instantiate(wallPrefab, mazeTransform);
                        newWall.localPosition = newPos;
                        Transform newLight = Instantiate(lightPrefab, newWall);
                    }
                }

                if (cell.HasFlag(WallState.LEFT))
                {
                    Vector3 newPos = position + new Vector3(-0.5f, 0.5f, 0);
                    if (!Physics.CheckSphere(mazeTransform.position + newPos, 0.1f))
                    {
                        Transform newWall = Instantiate(wallPrefab, mazeTransform);
                        newWall.localPosition = newPos;
                        newWall.eulerAngles = new Vector3(0, 90, 0);
                        Transform newLight = Instantiate(lightPrefab, newWall);
                    }
                }

                if (j == 0)
                {
                    if (cell.HasFlag(WallState.DOWN) && new Vector2Int(i, j) != startPos)
                    {
                        Vector3 newPos = position + new Vector3(0, 0.5f, -0.5f);
                        if (!Physics.CheckSphere(mazeTransform.position + newPos, 0.1f))
                        {
                            Transform newWall = Instantiate(wallPrefab, mazeTransform);
                            newWall.localPosition = newPos;
                            Transform newLight = Instantiate(lightPrefab, newWall);
                        }
                    }
                }

                if (i == width - 1)
                {
                    if (cell.HasFlag(WallState.RIGHT))
                    {
                        Vector3 newPos = position + new Vector3(0.5f, 0.5f, 0);
                        if (!Physics.CheckSphere(mazeTransform.position + newPos, 0.1f))
                        {
                            Transform newWall = Instantiate(wallPrefab, mazeTransform);
                            newWall.localPosition = newPos;
                            newWall.eulerAngles = new Vector3(0, 90, 0);
                            Transform newLight = Instantiate(lightPrefab, newWall);
                        }
                    }
                }
            }

        }

    }
}
