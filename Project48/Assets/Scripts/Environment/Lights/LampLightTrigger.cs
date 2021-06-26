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
        

        private Material material;

        public float trigger_radius;
        public float min_intensity;
        public float max_intensity;
        // Color of children <lamp sphere>
        public Color color;

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
            material = transform.Find("lamp_sphere").GetComponent<MeshRenderer>().material;
            material.color = color;

            light.type = LightType.Point;

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
            }
            else
            {
                off_time_elapsed += Time.deltaTime;
                light.intensity = min_intensity + off_curve.Evaluate(off_time_elapsed) * max_intensity;
            }
            color.a = light.intensity;
            material.color = color;
            transform.Find("lamp_sphere").GetComponent<MeshRenderer>().material = material;
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
