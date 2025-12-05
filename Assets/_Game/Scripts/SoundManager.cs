using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

public class SoundFX
{
    public const string ClickStask = "Interface Click 10-1";
    public const string UI_Click = "UI_3_Clicks_01_Appear_mono";
    public const string Fish_Drop = "UI_Switch_Dirty_stereo";
    public const string Fish_Finsher = "etfx_spawn";
}

public class Music
{
    public const string k_Music_bg_music1 = "Casual Music Loop 07";
    public const string k_Music_Lose = "Negative 09";
    public const string k_Music_Win = "Positive 03-2";
}

public class SoundManager : Singleton<SoundManager>
{
    public List<AudioClip> audioData;
    [Header("Audio Configuration")] public AudioMixer masterMixer;

    public AudioSource musicSource;

    private ObjectPool<AudioSource> audioSourcePool;

    [ShowInInspector] private Dictionary<string, AudioClip> dicAudioClips;

    private void Awake()
    {
        LoadAudioClips();
        InitializeAudioPool();
    }

    public void PlaySFX(string clipName, float volume = 1f)
    {
        if (!dicAudioClips.TryGetValue(clipName, out AudioClip clip))
        {
            Debug.LogWarning($"[AudioModule] Audio clip not found: {clipName}");
            return;
        }

        var source = audioSourcePool.Get();
        if (source == null) return;

        source.clip = clip;
        source.volume = volume;
        source.Play();

        float clipLength = clip.length;
        if (clipLength <= 0) clipLength = 0.1f;

        StartCoroutine(ReturnToPoolAfterPlay(source, clipLength));
    }

    public void PlayMusic(string clipName, bool loop = true)
    {
        if (dicAudioClips.TryGetValue(clipName, out AudioClip clip))
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void SetMasterVolume(float volume)
    {
        float dbVolume = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        masterMixer.SetFloat("MasterVolume", dbVolume);
    }

    public void SetSFXVolume(float volume)
    {
        float dbVolume = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        masterMixer.SetFloat("SFXVolume", dbVolume);
    }

    public void SetMusicVolume(float volume)
    {
        float dbVolume = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        masterMixer.SetFloat("MusicVolume", dbVolume);
    }

    public void SetMusicSourceVolume(float volume)
    {
        musicSource.volume = volume;
    }

    private void InitializeAudioPool()
    {
        audioSourcePool = new ObjectPool<AudioSource>(
            () =>
            {
                AudioSource source = new GameObject("Pooled Audio Source").AddComponent<AudioSource>();
                source.outputAudioMixerGroup = masterMixer.FindMatchingGroups("SFX")[0];
                source.playOnAwake = false;
                source.transform.SetParent(transform);
                return source;
            },
            (source) => { source.gameObject.SetActive(true); },
            (source) =>
            {
                source.Stop();
                source.gameObject.SetActive(false);
            },
            (source) => { Destroy(source.gameObject); },
            defaultCapacity: 10,
            maxSize: 50
        );
    }

    private void LoadAudioClips()
    {
        dicAudioClips = new Dictionary<string, AudioClip>();
        foreach (AudioClip clip in audioData)
        {
            if (clip != null)
            {
                dicAudioClips[clip.name] = clip;
            }
        }
    }

    private IEnumerator ReturnToPoolAfterPlay(AudioSource source, float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        if (source != null && audioSourcePool != null)
        {
            audioSourcePool.Release(source);
        }
    }
}