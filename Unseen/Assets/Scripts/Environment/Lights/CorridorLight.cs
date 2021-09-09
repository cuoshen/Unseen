using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorridorLight : TriggerLight
{
    [SerializeField]
    Transform emission_material_transform;
    [SerializeField]
    int emission_material_index;

    Material emission_material;

    new void Start()
    {
        base.Start();

        if (emission_material_index != -1)
            emission_material = emission_material_transform.GetComponent<MeshRenderer>().materials[emission_material_index];
    }

    new void Update()
    {
        base.Update();

        if (emission_material)
            emission_material.SetColor("_EmissionColor", lampLight.color * lampLight.intensity / maxIntensity);
    }
}
