using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public AudioReverbPreset reverbPreset;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            other.GetComponent<AudioReverbZone>().reverbPreset = reverbPreset;
        }
    }
}
