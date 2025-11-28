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

    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    private bool canNext = false;

    private void Awake()
    {
        NextButton.onClick.AddListener(OnNextButtonClickHandle);
        NextButton2.onClick.AddListener(OnNextButtonClickHandle);
    }

    public override void Open()
    {
        base.Open();
        SoundManager.Ins.PlaySoundBG(SoundBg.win);
        content1.SetActive(true);
        content2.SetActive(false);

        textCoin.text = $"{DataManager.Ins.userData.coin}";

        canNext = false;
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
            value => { textCoin.text = $"{value}"; }).OnComplete(() => { canNext = true; });
        DataManager.Ins.AddCoin(100);
    }

    private void OnNextButtonClickHandle()
    {
        if (!canNext) return;
        canNext = false;
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

        // Lấy các thông số cần thiết
        int currentLevel = DataManager.Ins.userData.level;
        int targetLevel = GameManager.Ins.UnlockFeatures[index].levelUnlock;
        float totalRange = (targetLevel - temp) * 1.0f;

        // 1. Tính giá trị ĐÍCH (Level hiện tại)
        float endValue = (currentLevel - temp) / totalRange;

        // 2. Tính giá trị BẮT ĐẦU (Level trước đó = Level hiện tại - 1)
        // Dùng Mathf.Max(0, ...) để đảm bảo không bị âm nếu là level đầu tiên
        float startValue = (currentLevel - 1 - temp) / totalRange;
        if (startValue < 0) startValue = 0;

        // 3. Đặt slider ngay lập tức về vị trí cũ
        slider.value = startValue;

        // Cập nhật Text hiện tại (có thể để nó chạy theo slider nếu muốn, ở đây đang hiển thị số cuối cùng)
        processText.text = $"{currentLevel - temp} / {targetLevel - temp}";

        // Xử lý Logic hoàn thành (Unlock)
        if (endValue >= 1)
        {
            DataManager.Ins.userData.indexCurrentFeature++;

            // Chạy từ startValue lên 1
            slider.DOValue(1, 1f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                content1.SetActive(false);
                content2.SetActive(true);
                iconFeatureUnlock.sprite = feature.spriteUnlock;
                title.text = feature.Title;
                description.text = feature.Description;
            }).SetDelay(0.2f);
        }
        else
        {
            // Chạy từ startValue lên endValue
            slider.DOValue(endValue, 1f).SetEase(Ease.OutQuad).SetDelay(0.2f);
        }
    }
}