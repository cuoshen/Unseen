using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorridorLight : TriggerLight
{
    [SerializeField]
    int emission_material_index;

    Material emission_material;

    new void Start()
    {
        base.Start();

        if (emission_material_index != -1)
            emission_material = GetComponent<MeshRenderer>().materials[emission_material_index];
    }

    new void Update()
    {
        base.Update();

        if (emission_material)
            emission_material.SetColor("_EmissionColor", lampLight.color * lampLight.intensity / maxIntensity);
    }
}
