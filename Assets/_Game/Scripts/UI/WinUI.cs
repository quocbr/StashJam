using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinUI : UICanvas
{
    [SerializeField] private Button NextButton;

    private void Awake()
    {
        NextButton.onClick.AddListener(OnNextButtonClickHandle);
    }

    public override void Open()
    {
        base.Open();
        DataManager.Ins.userData.level++;
        DataManager.Ins.SaveData();
    }

    private void OnNextButtonClickHandle()
    {
        LevelManager.Ins.SpawnLevel(DataManager.Ins.userData.level);
        Close(0);
    }
}