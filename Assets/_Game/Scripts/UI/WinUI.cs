using System;
using System.Collections;
using System.Collections.Generic;
using AssetKits.ParticleImage;
using DG.Tweening;
using MaskTransitions;
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
    [SerializeField] private BaseButton NextButton;
    [SerializeField] private BaseButton NextButton2;
    [SerializeField] private GameObject content1;
    [SerializeField] private GameObject container1;
    [SerializeField] private GameObject content2;
    [SerializeField] private Transform rewardIcon;
    [SerializeField] private RectTransform Content;

    [SerializeField] private Image iconFeature;
    [SerializeField] private Image iconFeatureUnlock;

    public TextMeshProUGUI title;
    public TextMeshProUGUI description;

    private bool isBlock = false;

    private void Awake()
    {
        NextButton.AddListener(OnNextButtonClickHandle);
        NextButton2.AddListener(OnNextButtonClickHandle);
    }

    public override void Open()
    {
        base.Open();
        TinySauce.OnGameFinished(true, 0, DataManager.Ins.userData.level + 1);
        isBlock = false;
        SoundManager.Ins.PlayMusic(Music.k_Music_Win, false);
        content1.SetActive(true);
        content2.SetActive(false);

        textCoin.text = $"{DataManager.Ins.userData.coin}";
        DataManager.Ins.userData.level++;
        DataManager.Ins.SaveData();

        DOVirtual.DelayedCall(0.2f, () => { });
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
        DOVirtual.Int(DataManager.Ins.userData.coin, DataManager.Ins.userData.coin + 100, 0.5f,
            value => { textCoin.text = $"{value}"; }).OnComplete(() => { });
        DataManager.Ins.AddCoin(100);
    }

    public void Up()
    {
        TransitionManager.Instance.PlayStartHalfTransition(0.6f, 0.3f, () =>
        {
            LevelManager.Ins.SpawnLevel(DataManager.Ins.userData.level);
            TransitionManager.Instance.PlayEndHalfTransition(0.6f, 0.3f);
            Close(0);
        });
    }

    private void OnNextButtonClickHandle()
    {
        if (isBlock) return;
        isBlock = true;
        particleImage.transform.position = rewardIcon.position;
        particleImage.Play();
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

        int currentLevel = DataManager.Ins.userData.level;
        int targetLevel = GameManager.Ins.UnlockFeatures[index].levelUnlock;
        float totalRange = (targetLevel - temp) * 1.0f;

        float endValue = (currentLevel - temp) / totalRange;

        float startValue = (currentLevel - 1 - temp) / totalRange;
        if (startValue < 0) startValue = 0;

        slider.value = startValue;

        processText.text = $"{currentLevel - temp} / {targetLevel - temp}";

        if (endValue >= 1)
        {
            isBlock = true;
            DataManager.Ins.userData.indexCurrentFeature++;

            slider.DOValue(1, 1f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                content1.SetActive(false);
                content2.SetActive(true);
                content2.transform.DOScale(1f, 0.3f).From(0f).SetEase(Ease.OutBack);
                iconFeatureUnlock.sprite = feature.spriteUnlock;
                title.text = feature.Title;
                description.text = feature.Description;
                isBlock = false;
            }).SetDelay(0.2f);
        }
        else
        {
            // Chạy từ startValue lên endValue
            slider.DOValue(endValue, 1f).SetEase(Ease.OutQuad).SetDelay(0.2f);
        }
    }
}