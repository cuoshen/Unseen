using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLight : MonoBehaviour
{
    [SerializeField]
    bool detectPlayerAcrossWall;
    [SerializeField]
    float detectPlayerRange;
    [SerializeField]
    float maxIntensity;

    [SerializeField]
    AnimationCurve on_curve, on_curve_enemy, off_curve, off_curve_enemy;

    [SerializeField]
    Light lampLight;
    [SerializeField]
    int emission_material_index;

    Material emission_material;

    GameObject player;

    float distanceToPlayer;
    float time_elapsed;
    bool is_on;
    bool is_flicker;

    readonly float enableRange = 10;
    readonly float insectThingVisionRange = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        on_curve.postWrapMode = WrapMode.Clamp;
        on_curve_enemy.postWrapMode = WrapMode.Clamp;
        off_curve.postWrapMode = WrapMode.Clamp;
        off_curve_enemy.postWrapMode = WrapMode.Clamp;

        time_elapsed = 2;
        if (emission_material_index != -1)
            emission_material = GetComponent<MeshRenderer>().materials[emission_material_index];
    }

    void Update()
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
            if (emission_material)
                emission_material.SetColor("_EmissionColor", lampLight.color * lampLight.intensity / maxIntensity);
        }
    }

    void DetectObjects()
    {
        bool is_player = false;
        bool is_enemy = false;
        LayerMask maze_layer = 1 << LayerMask.NameToLayer("Maze");
        LayerMask enemy_layer = 1 << LayerMask.NameToLayer("Enemy");

        if (distanceToPlayer < detectPlayerRange && 
            (detectPlayerAcrossWall || !Physics.Linecast(transform.position, player.transform.position, out _, maze_layer)))
            is_player = true;

        // Get all colliders in range and look for player
        Collider[] allOverlappingColliders = Physics.OverlapSphere(transform.position, insectThingVisionRange, enemy_layer);

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