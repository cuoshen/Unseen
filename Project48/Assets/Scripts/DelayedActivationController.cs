using System;
using System.Collections;
using UnityEngine;

namespace Jail
{
    /// <summary>
    /// For given action, delay it by a certain period of time
    /// </summary>
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
