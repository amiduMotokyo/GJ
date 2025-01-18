using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SoundGroup
{
    public AudioClip audioClip;
    public string soundName;
}

public class SoundManager : MonoBehaviour
{
    public List<SoundGroup> bgm_List = new List<SoundGroup>();
    public List<SoundGroup> sound_List = new List<SoundGroup>();

    public static SoundManager Instance;
    public AudioSource Music { set { music = value; } get { return music; } }
    public AudioSource Voice { set { voice = value; } get { return voice; } }

    private AudioSource music;
    private AudioSource voice;

    void Awake()
    {
        Instance = this;
        if (music == null)
            music = this.gameObject.AddComponent<AudioSource>();
        if (voice == null)
            voice = this.gameObject.AddComponent<AudioSource>();
    }

    public void PlayBgm(string _bgmName)
    {
        music.clip = bgm_List[FindBgm(_bgmName)].audioClip;
        music.loop = true;
        music.Play();
    }
    public void PlayBgm(int index = 0)
    {
        music.clip = bgm_List[index].audioClip;
        music.loop = true;
        music.Play();
    }

    public void StopBgm()
    {
        StartCoroutine("BGMFade");
    }

    public int FindBgm(string _bgmName)
    {
        int i = 0;
        while (i < bgm_List.Count)
        {
            if (bgm_List[i].soundName == _bgmName)
            {
                return i;
            }
            i++;
        }
        return i;
    }

    public void PlayVoice(string _voiceName)
    {
        voice.Stop();
        voice.clip = bgm_List[FindBgm(_voiceName)].audioClip;
        voice.loop = true;
        voice.Play();
    }
    public void StopVoice(string _voiceName)
    {
        voice.clip = bgm_List[FindBgm(_voiceName)].audioClip;
        voice.Stop();
    }

    public void PlaySound(string _soundName)
    {
        if(!GameObject.Find(_soundName))
        {
            GameObject Camerea = GameObject.Find("Main Camera");
            transform.position = new Vector3(Camerea.transform.position.x, Camerea.transform.position.y, transform.position.z);
            AudioSource.PlayClipAtPoint(sound_List[FindSound(_soundName)].audioClip, transform.position, voice.volume);
            GameObject.Find("One shot audio").name = _soundName;
        }
        
    }

    public void PlaySound(string _soundName, Vector3 _pos)
    {
        AudioSource.PlayClipAtPoint(sound_List[FindSound(_soundName)].audioClip, _pos, voice.volume);
    }



    public int FindSound(string _soundName)
    {
        int i = 0;
        while (i < sound_List.Count)
        {
            if (sound_List[i].soundName == _soundName)
            {
                return i;
            }
            i++;
        }
        return i;
    }
    IEnumerator BGMFade()
    {
        float nowV = music.volume;
        while (nowV > 0)
        {
            nowV -= 0.05f;
            music.volume = nowV;
            yield return new WaitForSeconds(0.03f);
        }
        StopCoroutine("BGMFade");
        music.Stop();
        music.volume = 1;
        
    }
}