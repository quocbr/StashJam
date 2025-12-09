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
        if (!GameManager.Ins.isCheatMode)
        {
            TinySauce.OnGameFinished(true, 0, DataManager.Ins.userData.level + 1);
        }
        isBlock = false;
        SoundManager.Ins.PlayMusic(Music.k_Music_Win, false);
        content1.SetActive(true);
        content2.SetActive(false);

        textCoin.text = $"{DataManager.Ins.userData.coin}";
        DataManager.Ins.userData.level++;
        DataManager.Ins.SaveData();
        var features = GameManager.Ins.UnlockFeatures;

        int maxUnlockLevel = 0;
        if (features.Count > 0)
        {
            features.Sort((a, b) => a.levelUnlock.CompareTo(b.levelUnlock));
            maxUnlockLevel = features[features.Count - 1].levelUnlock;
        }

        if (DataManager.Ins.userData.level <= maxUnlockLevel)
        {
            Content.anchoredPosition = new Vector2(0, -100);
            container1.SetActive(true);
            SetProcessUnlockFeature1();
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
            Close(0);
            LevelManager.Ins.SpawnLevel(DataManager.Ins.userData.level);
            TransitionManager.Instance.PlayEndHalfTransition(0.6f, 0.35f);
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

    private void SetProcessUnlockFeature1()
    {
        int currentLevel = DataManager.Ins.userData.level;
        var features = GameManager.Ins.UnlockFeatures;
        int targetIndex = -1;

        for (int i = 0; i < features.Count; i++)
        {
            if (features[i].levelUnlock >= currentLevel)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex == -1)
        {
            targetIndex = features.Count - 1;
        }

        UnlockFeature feature = features[targetIndex];
        iconFeature.sprite = feature.spriteLock;

        int temp = 0;
        if (targetIndex > 0)
        {
            temp = features[targetIndex - 1].levelUnlock;
        }

        int targetLevel = feature.levelUnlock;
        float totalRange = (targetLevel - temp) * 1.0f;

        if (totalRange <= 0) totalRange = 1;

        float endValue = (currentLevel - temp) / totalRange;

        float startValue = (currentLevel - 1 - temp) / totalRange;
        if (startValue < 0) startValue = 0;

        slider.value = startValue;
        processText.text = $"{currentLevel - temp} / {targetLevel - temp}";
        if (endValue >= 1)
        {
            isBlock = true;
            slider.DOValue(1, 1f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                content1.SetActive(false);
                content2.SetActive(true);
                content2.transform.DOScale(1f, 0.3f).From(0f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    SoundManager.Ins.PlaySFX(SoundFX.UnLockFeature);
                });

                iconFeatureUnlock.sprite = feature.spriteUnlock;
                title.text = feature.Title;
                description.text = feature.Description;

                isBlock = false;
            }).SetDelay(0.2f);
        }
        else
        {
            slider.DOValue(endValue, 1f).SetEase(Ease.OutQuad).SetDelay(0.2f);
        }
    }

    public void PlaySoundReward()
    {
        SoundManager.Ins.PlaySFX(SoundFX.CoinSFX, 0.5f);
    }
}