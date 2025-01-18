using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    private AudioSource audioS;
    public List<SoundGroup> bgm_List = new List<SoundGroup>();
    public List<SoundGroup> sound_List = new List<SoundGroup>();
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        audioS = GetComponent<AudioSource>();
    }
    public void AudioPlay(AudioClip clip)
    {
        audioS.PlayOneShot(clip);
    }
}