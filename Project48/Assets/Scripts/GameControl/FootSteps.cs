using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jail
{
    public class FootSteps : MonoBehaviour
    {

        public AudioClip[] clips;
        private AudioSource audioSource;
        // Start is called before the first frame update
        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Step()
        {
            AudioClip clip = GetRandomClip();
            audioSource.PlayOneShot(clip);
        }

        AudioClip GetRandomClip()
        {
            return clips[Random.Range(0, clips.Length)];
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

