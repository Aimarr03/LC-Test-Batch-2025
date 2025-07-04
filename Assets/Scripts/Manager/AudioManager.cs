using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private List<AudioData> audioDatas;
    private Dictionary<string, AudioClip> audioClipDic;

    public static AudioManager Instance { get; private set; }
    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);

        audioClipDic = audioDatas.ToDictionary(x => x.name, x => x.clip);
    }
    public void PlayAudio(string name, bool randomPitch = false, float pitchValue = 0.2f)
    {
        AudioClip audio = audioClipDic[name];
        if (audio == null)
        {
            Debug.Log("No Audio Found");
            return;
        }
        PlayAudio(audio, randomPitch, pitchValue);
    }
    public void PlayAudio(AudioClip audio, bool randomPitch = false, float pitchValue = 0.2f)
    {
        if (randomPitch)
        {
            float bufferPitch = sfxSource.pitch;
            sfxSource.pitch = UnityEngine.Random.Range(bufferPitch - pitchValue, bufferPitch + pitchValue);
            Debug.Log("SFX PITCH: " + sfxSource.pitch);
            sfxSource.PlayOneShot(audio);
            sfxSource.pitch = bufferPitch;
        }
        else
        {
            sfxSource.PlayOneShot(audio);
        }
    }
    public IEnumerator AssignNewMusic(string musicID)
    {
        if(musicSource.clip != null)
        {
            yield return musicSource.DOFade(0, 1.2f);
        }
        musicSource.Stop();
        musicSource.clip = audioClipDic[musicID];
        musicSource.Play();
        yield return musicSource.DOFade(0.66f, 1.2f);
    }
    public IEnumerator StopMusicWithFade(float duration = 1.2f)
    {
        yield return musicSource.DOFade(0, duration);
        musicSource.Stop();
    }
    public void StopMusic()=> musicSource.Stop();
    public void StartMusic()=> musicSource.Play();
}
[Serializable]
public class AudioData
{
    public string name;
    public AudioClip clip;
}
