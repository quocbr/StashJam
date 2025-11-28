using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    private const string SAVE_KEY = "UserData";
    public UserData userData;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    private void Start()
    {
        LoadData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveData();
        }
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(userData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Data saved");
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            userData = JsonUtility.FromJson<UserData>(json);
        }
        else
        {
            userData = new UserData();
            SaveData();
        }

        SoundManager.Ins.SetActiveSoundBG(!userData.isMusic);
        SoundManager.Ins.SetActiveSoundFx(!userData.isFX);

        LevelManager.Ins.SpawnLevel(userData.level);
    }

    public void ResetData()
    {
        userData = new UserData();
        SaveData();
    }

    public void AddCoin(int coin)
    {
        AddCoin cb = new AddCoin();
        cb.currentCoin = userData.coin;
        cb.coinAdd = coin;
        EventManager.Trigger(cb);
        userData.coin += coin;
    }
}

[Serializable]
public class UserData
{
    public int level;
    public int coin;
    public int indexCurrentFeature;
    public bool isMusic;
    public bool isFX;
    public bool isHaptic;

    public UserData()
    {
        level = 0;
        coin = 0;
        indexCurrentFeature = 0;
        isMusic = true;
        isFX = true;
        isHaptic = true;
    }
}