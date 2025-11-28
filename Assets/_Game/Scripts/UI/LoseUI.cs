using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoseUI : UICanvas
{
    [SerializeField] private Button ReTryButton;

    private void Awake()
    {
        ReTryButton.onClick.AddListener(OnNextButtonClickHandle);
    }

    public override void Open()
    {
        base.Open();
        SoundManager.Ins.PlaySoundBG(SoundBg.lose);
    }

    private void OnNextButtonClickHandle()
    {
        LevelManager.Ins.SpawnLevel(DataManager.Ins.userData.level);
        Close(0);
    }
}