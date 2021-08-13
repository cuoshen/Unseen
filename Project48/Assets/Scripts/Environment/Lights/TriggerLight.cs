using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLight : MonoBehaviour
{
    [SerializeField]
    protected bool detectPlayerAcrossWall;
    [SerializeField]
    protected float detectPlayerRange;

    [SerializeField]
    protected AnimationCurve on_curve, on_curve_enemy, off_curve, off_curve_enemy;

    [SerializeField]
    protected Light lampLight;

    GameObject player;

    protected float maxIntensity;
    protected float distanceToPlayer;
    protected float time_elapsed;
    protected bool is_on;
    protected bool is_flicker;

    protected readonly float enableRange = 10;
    protected readonly float insectThingVisionRange = 1.5f;

    protected void Start()
    {
        on_curve.postWrapMode = WrapMode.Clamp;
        on_curve_enemy.postWrapMode = WrapMode.Clamp;
        off_curve.postWrapMode = WrapMode.Clamp;
        off_curve_enemy.postWrapMode = WrapMode.Clamp;

        maxIntensity = lampLight.intensity;
        time_elapsed = 2;
    }

    protected void Update()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        else
        {
            distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            
            if (distanceToPlayer < enableRange)
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
            }
            else
            {
                lampLight.intensity = 0;
            }
        }
    }

    void DetectObjects()
    {
        bool is_player = false;
        bool is_enemy = false;
        LayerMask maze_layer = 1 << LayerMask.NameToLayer("Maze");
        LayerMask insect_layer = 1 << LayerMask.NameToLayer("InsectThing");
        LayerMask giant_layer = 1 << LayerMask.NameToLayer("GiantThing");

        if (distanceToPlayer < detectPlayerRange &&
            !Physics.Linecast(transform.position, player.transform.position, out _, giant_layer) &&
            (detectPlayerAcrossWall || !Physics.Linecast(transform.position, player.transform.position, out _, maze_layer)))
            is_player = true;

        // Get all colliders in range and look for player
        Collider[] allOverlappingColliders = Physics.OverlapSphere(transform.position, insectThingVisionRange, insect_layer);

        // Look for player and enemy
        foreach (Collider collider in allOverlappingColliders)
            if (!Physics.Linecast(transform.position, collider.transform.position, out _, maze_layer))
                is_enemy = true;

        // Reset time_elapsed when switching states
        if ((!is_on && is_player) || (is_on && !is_player) || (!is_flicker && is_enemy))
            time_elapsed = 0;

        // Set states
        is_on = is_player;
        is_flicker = is_enemy;
    }
}