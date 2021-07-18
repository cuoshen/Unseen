using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]

public class TriggerLight : MonoBehaviour
{
    [SerializeField]
    float detectPlayerRange;
    [SerializeField]
    float maxIntensity;
    [SerializeField]
    Color lampSphereColor;

    [SerializeField]
    AnimationCurve on_curve, on_curve_enemy, off_curve, off_curve_enemy;

    Light lampLight;
    Material material;

    float time_elapsed;

    bool is_on;
    bool is_flicker;

    // Start is called before the first frame update
    void Start()
    {
        on_curve.postWrapMode = WrapMode.Clamp;
        on_curve_enemy.postWrapMode = WrapMode.Clamp;
        off_curve.postWrapMode = WrapMode.Clamp;
        off_curve_enemy.postWrapMode = WrapMode.Clamp;

        // init gameObject references
        lampLight = GetComponent<Light>();
        lampLight.type = LightType.Point;
        lampLight.color = lampSphereColor;

        material = GetComponent<MeshRenderer>().materials[0];

        time_elapsed = 2;
    }

    void Update()
    {
        DetectObjects();

        // Control light acording to triggered condition
        AnimationCurve curve;
        if (is_on)
        {
            curve = on_curve;
            if (is_flicker)
            {
                curve = on_curve_enemy;
                if (time_elapsed >= 1.0f)
                {
                    time_elapsed = 0;
                }
            }
        }
        else
        {
            curve = off_curve;
            if (is_flicker)
            {
                curve = off_curve_enemy;
            }
        }

        time_elapsed += Time.deltaTime;
        lampLight.intensity = curve.Evaluate(time_elapsed) * maxIntensity;
        material.SetColor("_EmissionColor", lampSphereColor * lampLight.intensity / maxIntensity);
    }

    void DetectObjects()
    {
        bool is_player = false;
        bool is_enemy = false;
        LayerMask maze_layer = 1 << LayerMask.NameToLayer("Maze");

        // Get all colliders in range and look for player
        Collider[] allOverlappingColliders = Physics.OverlapSphere(transform.position, detectPlayerRange);

        // Look for player and enemy
        foreach (Collider collider in allOverlappingColliders)
        {
            if (collider.tag == "Player" && !Physics.Linecast(transform.position, collider.transform.position, out _, maze_layer))
                is_player = true;

            if (collider.tag == "Enemy" && !Physics.Linecast(transform.position, collider.transform.position, out _, maze_layer)
                && Vector3.Distance(collider.transform.position, transform.position) <= collider.GetComponent<InsectThing>().visionRange)
                is_enemy = true;
        }

        // Reset time_elapsed when switching states
        if ((!is_on && is_player) || (is_on && !is_player) || (!is_flicker && is_enemy))
            time_elapsed = 0;

        // Set states
        is_on = is_player;
        is_flicker = is_enemy;
    }
}
