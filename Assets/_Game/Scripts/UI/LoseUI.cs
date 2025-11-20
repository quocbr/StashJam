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

    private void OnNextButtonClickHandle()
    {
        LevelManager.Ins.SpawnLevel(DataManager.Ins.userData.level);
        Close(0);
    }
}