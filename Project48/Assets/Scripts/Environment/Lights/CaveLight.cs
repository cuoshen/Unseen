using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class CaveLight : TriggerLight
{
    [SerializeField]
    GameObject fire;
    Vector3 fireBaseScale;
    
    new void Start()
    {
        base.Start();

        fireBaseScale = fire.transform.localScale;
    }

    new void Update()
    {
        base.Update();

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
