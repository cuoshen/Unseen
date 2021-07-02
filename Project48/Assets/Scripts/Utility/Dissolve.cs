using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dissolve script controls the dissolve shader
/// </summary>
[RequireComponent(typeof(Renderer))]
public class Dissolve : MonoBehaviour
{
    private Material material;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        var time = Time.time * Mathf.PI * 0.75f;
        float height = 10 * Mathf.Sin(time) + 5.0f;
        SetHeight(height);
    }

    private void SetHeight(float height)
    {
        material.SetFloat("_CutoffHeight", height);
    }
}