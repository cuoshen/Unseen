using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveMode{
    constant,
    lerp
}

public class PlatformMove : MonoBehaviour
{

    public Transform[] route;
    public MoveMode mode;
    public float speed;

    private int route_index;

    public Transform platform;

    // Start is called before the first frame update
    void Start()
    {
        route_index = 0;
        platform.position = route[route_index].position;
    }

    private void FixedUpdate()
    {
        if ((platform.position - route[route_index].position).sqrMagnitude < 0.01f)
        {
            route_index++;
            if (route_index >= route.Length)
            {
                route_index = 0;
            }
        }

        //Vector3 v = Vector3.zero;
        if (mode == MoveMode.constant)
        {
            platform.position = Vector3.MoveTowards(platform.position, route[route_index].position, Time.fixedDeltaTime * speed);
        } else if (mode == MoveMode.lerp)
        {
            platform.position = Vector3.Lerp(platform.position, route[route_index].position, Time.fixedDeltaTime * speed);
        }

    }
}
