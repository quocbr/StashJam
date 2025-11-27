using System;
using System.Collections;
using System.Collections.Generic;
using AssetKits.ParticleImage;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class WinUI : UICanvas
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI processText;
    [SerializeField] private TextMeshProUGUI textCoin;
    [SerializeField] private ParticleImage particleImage;
    [SerializeField] private Button NextButton;
    [SerializeField] private Button NextButton2;
    [SerializeField] private GameObject content1;
    [SerializeField] private GameObject container1;
    [SerializeField] private GameObject content2;
    [SerializeField] private Transform rewardIcon;
    [SerializeField] private RectTransform Content;

    [SerializeField] private Image iconFeature;
    [SerializeField] private Image iconFeatureUnlock;

    private void Awake()
    {
        NextButton.onClick.AddListener(OnNextButtonClickHandle);
        NextButton2.onClick.AddListener(OnNextButtonClickHandle);
    }

    public override void Open()
    {
        base.Open();
        content1.SetActive(true);
        content2.SetActive(false);

        textCoin.text = $"{DataManager.Ins.userData.coin}";

        DataManager.Ins.userData.level++;
        DataManager.Ins.SaveData();

        DOVirtual.DelayedCall(0.2f, () => { particleImage.Play(); });
        if (DataManager.Ins.userData.indexCurrentFeature < GameManager.Ins.UnlockFeatures.Count)
        {
            Content.anchoredPosition = new Vector2(0, -100);
            container1.SetActive(true);
            SetProcessUnlockFeature(DataManager.Ins.userData.indexCurrentFeature);
        }
        else
        {
            Content.anchoredPosition = new Vector2(0, -250);
            container1.SetActive(false);
        }
    }

    [Button]
    public void FlyCoin()
    {
        particleImage.transform.position = rewardIcon.position;
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
        UnlockFeature feature = GameManager.Ins.UnlockFeatures[index];
        iconFeature.sprite = feature.spriteLock;
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
            slider.DOValue(1, 1f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                content1.SetActive(false);
                content2.SetActive(true);
                iconFeatureUnlock.sprite = feature.spriteUnlock;
            }).SetDelay(0.2f);
        }
        else
        {
            slider.DOValue(value, 1f).SetEase(Ease.OutQuad).SetDelay(0.2f);
            processText.text =
                $"{DataManager.Ins.userData.level - temp}  / {GameManager.Ins.UnlockFeatures[index].levelUnlock - temp}";
        }


    }
}