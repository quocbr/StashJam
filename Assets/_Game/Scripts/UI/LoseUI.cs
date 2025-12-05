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
        SoundManager.Ins.PlayMusic(Music.k_Music_Lose, false);
    }

    private void OnNextButtonClickHandle()
    {
        LevelManager.Ins.SpawnLevel(DataManager.Ins.userData.level);
        Close(0);
    }
}