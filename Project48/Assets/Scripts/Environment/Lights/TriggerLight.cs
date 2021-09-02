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
    protected AnimationCurve on_curve, flicker_curve, off_curve;

    [SerializeField]
    protected Light lampLight;

    GameObject player;

    protected float maxIntensity;
    protected float onSwitchStateIntensity;
    protected float distanceToPlayer;
    protected float time_elapsed;
    protected float currentCurveValue;
    protected bool is_on;
    protected bool is_flicker;

    protected readonly float enableRange = 10;
    protected readonly float insectThingVisionRange = 1.5f;

    protected void Start()
    {
        on_curve.postWrapMode = WrapMode.Clamp;
        flicker_curve.postWrapMode = WrapMode.Clamp;
        off_curve.postWrapMode = WrapMode.Clamp;

        maxIntensity = lampLight.intensity;
        time_elapsed = 2;
        currentCurveValue = 0;
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
                time_elapsed += Time.deltaTime;

                // Control light acording to triggered condition
                if (is_on)
                {
                    currentCurveValue = on_curve.Evaluate(time_elapsed);
                    lampLight.intensity = currentCurveValue * (maxIntensity - onSwitchStateIntensity) + onSwitchStateIntensity;
                }
                else
                {
                    currentCurveValue = off_curve.Evaluate(time_elapsed);
                    lampLight.intensity = currentCurveValue * onSwitchStateIntensity;
                }

                if (is_flicker)
                    lampLight.intensity *= flicker_curve.Evaluate(time_elapsed % 1);
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
        bool is_giant = false;
        LayerMask maze_layer = 1 << LayerMask.NameToLayer("Maze");
        LayerMask insect_layer = 1 << LayerMask.NameToLayer("InsectThing");
        LayerMask giant_layer = 1 << LayerMask.NameToLayer("GiantThing");

        if (Physics.Linecast(transform.position, player.transform.position, out _, giant_layer))
            is_giant = true;

        Collider[] allGiantCollidersInRange = Physics.OverlapSphere(transform.position, 0.2f, giant_layer);
        if (allGiantCollidersInRange.Length != 0)
        {
            lampLight.intensity = 0;
            is_giant = true;
        }

        if (distanceToPlayer < detectPlayerRange && !is_giant &&
            (detectPlayerAcrossWall || !Physics.Linecast(transform.position, player.transform.position, out _, maze_layer)))
            is_player = true;

        Collider[] allKiwiCollidersInRange = Physics.OverlapCapsule(transform.position + new Vector3(0, 5, 0), transform.position + new Vector3(0, -5, 0), insectThingVisionRange, insect_layer);
        foreach (Collider collider in allKiwiCollidersInRange)
            if (!Physics.Linecast(transform.position, collider.transform.position, out _, maze_layer))
                is_enemy = true;

        // Reset time_elapsed when switching states
        if ((!is_on && is_player) || (is_on && !is_player) || (!is_flicker && is_enemy))
        {
            onSwitchStateIntensity = lampLight.intensity;
            time_elapsed = 0;
        }

        // Set states
        is_on = is_player;
        is_flicker = is_enemy;
    }
}