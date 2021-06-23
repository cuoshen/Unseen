using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jail
{

    [RequireComponent(typeof(Light))]
    [RequireComponent(typeof(SphereCollider))]

    public class LampLightTrigger : MonoBehaviour
    {

        Light light;
        SphereCollider sphereCollider;

        public float trigger_radius;
        //public bool instant_on;
        //public bool instant_off;
        public float min_intensity;
        public float max_intensity;

        public AnimationCurve on_curve, off_curve;
        private float on_time_elapsed;
        private float off_time_elapsed;

        private bool triggered;

        // Start is called before the first frame update
        void Start()
        {
            // init gameObject references
            light = GetComponent<Light>();
            sphereCollider = GetComponent<SphereCollider>();

            // force collider to be trigger
            sphereCollider.isTrigger = true;

            // set trigger radius
            if (trigger_radius >= 0)
            {
                sphereCollider.radius = trigger_radius;
            }

            on_curve.postWrapMode = WrapMode.Clamp;
            off_curve.postWrapMode = WrapMode.Clamp;
        }

        private void Update()
        {
            // control light acording to triggered condition
            if (triggered)
            {
                on_time_elapsed += Time.deltaTime;
                light.intensity = min_intensity + on_curve.Evaluate(on_time_elapsed) * max_intensity;
                //if (instant_on)
                //{
                //    // instant react
                //    light.intensity = max_intensity;
                //}
                //else
                //{
                //    // smooth react
                //    light.intensity = Mathf.Lerp(light.intensity, max_intensity, Time.deltaTime);
                //}
            }
            else
            {
                off_time_elapsed += Time.deltaTime;
                light.intensity = min_intensity + off_curve.Evaluate(off_time_elapsed) * max_intensity;
                //if (instant_off)
                //{
                //    // instant react
                //    light.intensity = min_intensity;
                //}
                //else
                //{
                //    // smooth react
                //    light.intensity = Mathf.Lerp(light.intensity, min_intensity, Time.deltaTime);
                //}
            }
        }

        // object enter
        private void OnTriggerEnter(Collider other)
        {
            off_time_elapsed = 0;
            triggered = true;
        }

        // object exit
        private void OnTriggerExit(Collider other)
        {
            on_time_elapsed = 0;
            triggered = false;
        }
    }
}
