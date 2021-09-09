using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsectSounds : MonoBehaviour
{
    public AudioClip[] clips;
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Idle()
    {
        audioSource.PlayOneShot(clips[0]);
    }
    void Encounter()
    {
        audioSource.PlayOneShot(clips[1]);
    }
    void Kick()
    {
        audioSource.PlayOneShot(clips[Random.Range(2, 4)]);
    }
   
    AudioClip GetRandomClip()
    {
        return clips[Random.Range(0, clips.Length)];
    }
}