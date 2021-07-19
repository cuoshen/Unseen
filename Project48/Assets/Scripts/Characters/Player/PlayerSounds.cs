using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    public AudioClip[] clips;
    AudioSource audioSource;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Step()
    {
        AudioClip clip = GetRandomClip();
        audioSource.PlayOneShot(clip);
    }

    AudioClip GetRandomClip()
    {
        return clips[Random.Range(0, clips.Length)];
    }
}

