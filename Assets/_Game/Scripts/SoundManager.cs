using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public enum SoundBg
{
    bg = 0,
    win = 1,
    lose = 2
}

public enum SoundFx
{
    ClickStask = 0
}

public class SoundManager : Singleton<SoundManager>
{
    public List<AudioSource> list_SoundBG;
    public List<AudioSource> list_SoundFx;

    private float dfVolume = 1f;
    private float xmasVolume = 0.25f;

    public void PlaySoundBG(SoundBg soundbg)
    {
        for (int i = 0; i < list_SoundBG.Count; i++)
        {
            if (!list_SoundBG[i].isPlaying) continue;
            if ((int)soundbg == i) return;

            int temp = i;
            list_SoundBG[temp].Stop();
        }

        list_SoundBG[(int)soundbg].Play();
    }

    [Button]
    public void PlaySoundFx(SoundFx fx, float timePlay = 0)
    {
        if (timePlay != 0) list_SoundFx[(int)fx].time = timePlay;
        list_SoundFx[(int)fx].Play();
    }

    public void SetActiveSoundBG(bool isMute)
    {
        for (int i = 0; i < list_SoundBG.Count; i++)
        {
            list_SoundBG[i].mute = isMute;
        }
    }

    public void SetActiveSoundFx(bool isMute)
    {
        for (int i = 0; i < list_SoundFx.Count; i++)
        {
            list_SoundFx[i].mute = isMute;
        }
    }
}