using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : UICanvas
{
    public GameObject musicOn;
    public GameObject musicOff;

    public GameObject soundOn;
    public GameObject soundOff;

    public GameObject hapticOn;
    public GameObject hapticOff;

    [SerializeField] private Button replayButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private Button music;
    [SerializeField] private Button sound;
    [SerializeField] private Button haptic;

    private void Awake()
    {
        replayButton.onClick.AddListener(OnReplayBtnClickHandler);
        closeButton.onClick.AddListener(OnCloseBtnClickHandler);
        music.onClick.AddListener(OnMusicClickHandler);
        sound.onClick.AddListener(OnSoundClickHandler);
        haptic.onClick.AddListener(OnHapticClickHandler);
    }

    private void OnHapticClickHandler()
    {
        DataManager.Ins.userData.isHaptic = !DataManager.Ins.userData.isHaptic;
        if (DataManager.Ins.userData.isHaptic)
        {
            hapticOn.SetActive(true);
            hapticOff.SetActive(false);
        }
        else
        {
            hapticOn.SetActive(false);
            hapticOff.SetActive(true);
        }
    }

    private void OnMusicClickHandler()
    {
        DataManager.Ins.userData.isMusic = !DataManager.Ins.userData.isMusic;
        SoundManager.Ins.SetActiveSoundBG(!DataManager.Ins.userData.isMusic);

        if (DataManager.Ins.userData.isMusic)
        {
            musicOn.SetActive(true);
            musicOff.SetActive(false);
        }
        else
        {
            musicOn.SetActive(false);
            musicOff.SetActive(true);
        }
    }

    private void OnSoundClickHandler()
    {
        DataManager.Ins.userData.isFX = !DataManager.Ins.userData.isFX;
        SoundManager.Ins.SetActiveSoundFx(!DataManager.Ins.userData.isFX);
        if (DataManager.Ins.userData.isFX)
        {
            soundOn.SetActive(true);
            soundOff.SetActive(false);
        }
        else
        {
            soundOn.SetActive(false);
            soundOff.SetActive(true);
        }
    }

    public override void Open()
    {
        base.Open();
        Controller.Ins.isPlay = false;
        if (DataManager.Ins.userData.isHaptic)
        {
            hapticOn.SetActive(true);
            hapticOff.SetActive(false);
        }
        else
        {
            hapticOn.SetActive(false);
            hapticOff.SetActive(true);
        }

        if (DataManager.Ins.userData.isMusic)
        {
            musicOn.SetActive(true);
            musicOff.SetActive(false);
        }
        else
        {
            musicOn.SetActive(false);
            musicOff.SetActive(true);
        }

        if (DataManager.Ins.userData.isFX)
        {
            soundOn.SetActive(true);
            soundOff.SetActive(false);
        }
        else
        {
            soundOn.SetActive(false);
            soundOff.SetActive(true);
        }
    }

    private void OnReplayBtnClickHandler()
    {
        LevelManager.Ins.Replay();
        Close(0);
    }

    private void OnCloseBtnClickHandler()
    {
        Controller.Ins.isPlay = true;
        Close(0);
    }
}