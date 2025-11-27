using System;
using System.Collections;
using System.Collections.Generic;
using AssetKits.ParticleImage;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinUI : UICanvas
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI processText;
    [SerializeField] private TextMeshProUGUI textCoin;
    [SerializeField] private ParticleImage particleImage;
    [SerializeField] private Button NextButton;
    [SerializeField] private GameObject content1;

    private void Awake()
    {
        NextButton.onClick.AddListener(OnNextButtonClickHandle);
    }

    public override void Open()
    {
        base.Open();
        textCoin.text = $"{DataManager.Ins.userData.coin}";

        DataManager.Ins.userData.level++;
        DataManager.Ins.SaveData();

        DOVirtual.DelayedCall(0.3f, () => { particleImage.Play(); });
        if (DataManager.Ins.userData.indexCurrentFeature < GameManager.Ins.UnlockFeatures.Count)
        {
            content1.gameObject.SetActive(true);
            SetProcessUnlockFeature(DataManager.Ins.userData.indexCurrentFeature);
        }
        else
        {
            content1.gameObject.SetActive(false);
        }
    }

    public void FlyCoin()
    {
        DOVirtual.Int(DataManager.Ins.userData.coin, DataManager.Ins.userData.coin + 100, 0.5f,
            value => { textCoin.text = $"{value}"; });
        DataManager.Ins.AddCoin(100);
    }

    private void OnNextButtonClickHandle()
    {
        LevelManager.Ins.SpawnLevel(DataManager.Ins.userData.level);
        Close(0);
    }

    private void SetProcessUnlockFeature(int index)
    {
        int temp = 0;
        if (index > 0)
        {
            temp = GameManager.Ins.UnlockFeatures[index - 1].levelUnlock;
        }

        float value = (DataManager.Ins.userData.level - temp) /
                      ((GameManager.Ins.UnlockFeatures[index].levelUnlock - temp) * 1.0f);

        if (value >= 1)
        {
            DataManager.Ins.userData.indexCurrentFeature++;
            value = 1;
        }

        slider.value = value;
        processText.text =
            $"{DataManager.Ins.userData.level - temp}  / {GameManager.Ins.UnlockFeatures[index].levelUnlock - temp}";
    }
}