using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySounds : MonoBehaviour
{

    public AudioClip[] clips;
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    private void m1()
    {
        AudioClip clip = GetRandomClip();
        audioSource.PlayOneShot(clips[0]);
    }

    private void m2()
    {
        AudioClip clip = GetRandomClip();
        audioSource.PlayOneShot(clips[1]);
    }
    private void m3()
    {
        AudioClip clip = GetRandomClip();
        audioSource.PlayOneShot(clips[2]);
    }
    private void m4()
    {
        AudioClip clip = GetRandomClip();
        audioSource.PlayOneShot(clips[3]);
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