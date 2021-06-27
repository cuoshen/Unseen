using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Always update the player position to current position of the player
/// </summary>
[RequireComponent(typeof(Renderer))]
public class WallVisibility1 : MonoBehaviour
{
    private Material material;
    [SerializeField]
    private Transform player;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        material.SetVector("PlayerPosition", player.position);
    }
}
