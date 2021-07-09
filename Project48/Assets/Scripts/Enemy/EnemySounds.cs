using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySounds : MonoBehaviour
{
    public AudioClip[] clips;
    public AudioSource audioSource;
    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Idle()
    {
        audioSource.PlayOneShot(clips[0]);
    }
    private void Encounter()
    {
        audioSource.PlayOneShot(clips[1]);
    }
    private void Kick()
    {
        audioSource.PlayOneShot(clips[Random.Range(2, 4)]);
    }
   

    AudioClip GetRandomClip()
    {
        return clips[Random.Range(0, clips.Length)];
    }
}