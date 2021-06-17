using System;
using System.Collections;
using UnityEngine;

namespace Jail
{
    class DelayedActivationController : MonoBehaviour
    {
        private float t = 0.0f;
        public GameObject obj;

        public void DelayedActivation(float t)
        {
            this.t = t;
            StartCoroutine(LateActivation());
        }

        private IEnumerator LateActivation()
        {
            yield return new WaitForSeconds(t);

            obj.SetActive(true);
        }
    }
}
