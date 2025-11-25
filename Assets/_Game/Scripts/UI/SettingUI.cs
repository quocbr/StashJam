using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : UICanvas
{
    [SerializeField] private Button replayButton;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        replayButton.onClick.AddListener(OnReplayBtnClickHandler);
        closeButton.onClick.AddListener(OnCloseBtnClickHandler);
    }

    public override void Open()
    {
        base.Open();
        Controller.Ins.isPlay = false;
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