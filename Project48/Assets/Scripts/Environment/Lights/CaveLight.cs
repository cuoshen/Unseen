using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveLight : TriggerLight
{
    [SerializeField]
    GameObject fire;
    
    new void Update()
    {
        base.Update();

        if (is_on)
            fire.SetActive(true);
        else
            fire.SetActive(false);
    }
}
