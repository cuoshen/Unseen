using System;
using System.Collections.Generic;
using UnityEngine;

namespace Legacy
{
    [RequireComponent(typeof(AudioSource))]
    class InvisibleEnemyController : MonoBehaviour
    {
        private AudioSource audioSource;
        public AudioClip[] clips;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)]);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TopDownPlayerController p = other.GetComponent<TopDownPlayerController>();
            if (p != null)
            {
                // Player got eaten by invisible monster
                GameController.Instance.PlayerGotEaten();
            }
        }
    }
}
