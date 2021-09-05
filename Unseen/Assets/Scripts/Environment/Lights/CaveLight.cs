using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class CaveLight : TriggerLight
{
    [SerializeField]
    GameObject fire;
    Vector3 fireBaseScale;

    float flickerPerlinOffset;
    [SerializeField]
    float flickerRate;
    [SerializeField]
    float flickerAmplitude;

    //[SerializeField]
    float perlin;

    new void Start()
    {
        base.Start();

        fireBaseScale = fire.transform.localScale;
        flickerPerlinOffset = UnityEngine.Random.Range(0f, 100f);
    }

    new void Update()
    {
        base.Update();

        if (is_on && !is_flicker && time_elapsed > 1)
        {
            perlin = Algorithms.PerlinAtTile(Time.deltaTime * flickerRate + flickerPerlinOffset, 0);
            lampLight.intensity = maxIntensity * (flickerAmplitude * (perlin - 0.5f) + 1);
        }

        if (lampLight.intensity == 0)
        {
            fire.SetActive(false);
        }
        else
        {
            fire.SetActive(true);
            fire.transform.localScale = lampLight.intensity / maxIntensity * fireBaseScale;
        }
    }
}
