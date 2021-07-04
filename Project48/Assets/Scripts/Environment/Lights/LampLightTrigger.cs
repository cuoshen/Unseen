using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
[RequireComponent(typeof(SphereCollider))]

public class LampLightTrigger : MonoBehaviour
{

    Light light;
    SphereCollider sphereCollider;
        

    private Material material;

    public float trigger_radius;
    public float min_intensity;
    public float max_intensity;
    // Color of children <lamp sphere>
    public Color color;

    public AnimationCurve on_curve, on_curve_enemy, off_curve;
    private float on_time_elapsed;
    private float off_time_elapsed;

    private bool triggered;
    private bool is_enemy;

    // Start is called before the first frame update
    void Start()
    {
        // init gameObject references
        light = GetComponent<Light>();
        light.type = LightType.Point;
        light.color = color;

        sphereCollider = GetComponent<SphereCollider>();
        // force collider to be trigger
        sphereCollider.isTrigger = true;
        // set trigger radius
        if (trigger_radius >= 0)
        {
            sphereCollider.radius = trigger_radius;
        }

        material = GetComponent<MeshRenderer>().materials[0];
        material.SetColor("_EmissionColor", color * light.intensity / 10);

        on_curve.postWrapMode = WrapMode.Clamp;
        on_curve_enemy.postWrapMode = WrapMode.Clamp;
        off_curve.postWrapMode = WrapMode.Clamp;
        triggered = false;
        is_enemy = false;

    }

    private void Update()
    {
        
        // control light acording to triggered condition
        if (triggered)
        {
            on_time_elapsed += Time.deltaTime;
            AnimationCurve curve = on_curve;
            if (is_enemy)
            {
                curve = on_curve_enemy;
                if (on_time_elapsed >= 1.0)
                {
                    on_time_elapsed = 0;
                }
                
            }                
            light.intensity = min_intensity + curve.Evaluate(on_time_elapsed) * max_intensity;
        }
        else
        {
            off_time_elapsed += Time.deltaTime;
            light.intensity = min_intensity + off_curve.Evaluate(off_time_elapsed) * max_intensity;
        }
        color.a = light.intensity;
        material.color = color;
        
        GetComponent<MeshRenderer>().materials[0].SetColor("_EmissionColor", color * light.intensity / 10);
    }

    // object enter
    private void OnTriggerEnter(Collider other)
    {
        
        
    }

    private void OnTriggerStay(Collider other)
    {
        float distance = Vector3.Distance(other.transform.position, transform.position);
        RaycastHit hit;
        LayerMask maze_layer = 1 << LayerMask.NameToLayer("Maze");
        if (!Physics.Linecast(transform.position, other.transform.position, out hit, maze_layer))
        {

            if (other.tag == "Player")
            {
                off_time_elapsed = 0;
                triggered = true;
            }

            if (triggered && other.tag == "Enemy")
            {
                is_enemy = true;
            }

        }
        else if (Physics.Linecast(transform.position, other.transform.position, out hit, maze_layer))
        {
            if (other.tag == "Player")
            {
                triggered = false;
                on_time_elapsed = 0;
            }
            if (other.tag == "Enemy")
            {
                is_enemy = false;
                on_time_elapsed = 0;
                triggered = false;
            }
            

        }
    }

    // object exit
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            is_enemy = false;
        }
        on_time_elapsed = 0;
        triggered = false;


    }
}
