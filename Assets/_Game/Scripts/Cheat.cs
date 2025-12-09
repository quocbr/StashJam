using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cheat : MonoBehaviour
{
    public BaseButton btnCheat;
    public GameObject panelCheat;

    public BaseButton prevLevelBtn;
    public BaseButton nextLevelBtn;

    public BaseButton HideUIBtn;
    public BaseButton SoundBtn;

    public GameObject panelContent;

    private void Awake()
    {
        btnCheat.AddListener(OnOffCheat);
        prevLevelBtn.AddListener(OnPrevLevel);
        nextLevelBtn.AddListener(OnNextLevel);
        HideUIBtn.AddListener(OnHideUI);
        SoundBtn.AddListener(OnSound);
    }

    private void OnSound()
    {
        DataManager.Ins.userData.isMusic = !DataManager.Ins.userData.isMusic;
        SoundManager.Ins.SetMusicVolume(DataManager.Ins.userData.isMusic ? 1f : 0f);
    }

    private void OnHideUI()
    {
        panelContent.SetActive(!panelContent.activeSelf);
    }

    private void OnNextLevel()
    {
        if (LevelManager.Ins.allLevel.Count <= DataManager.Ins.userData.level + 1)
        {
            return;
        }

        DataManager.Ins.userData.level++;
        LevelManager.Ins.SpawnLevel(DataManager.Ins.userData.level);
    }

    private void OnPrevLevel()
    {
        if (DataManager.Ins.userData.level - 1 < 0)
        {
            return;
        }

        DataManager.Ins.userData.level--;
        LevelManager.Ins.SpawnLevel(DataManager.Ins.userData.level);
    }

    private void OnOffCheat()
    {
        panelCheat.SetActive(!panelCheat.activeSelf);
    }
}