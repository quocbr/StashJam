using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayUI : Singleton<GamePlayUI>
{
    [SerializeField] private Button settingBtn;
    public ConveyorController conveyorPrefab;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI coinText;
    public RectTransform top;
    public RectTransform down;
    public RectTransform left;
    public RectTransform right;

    public RectTransform posConveyor;
    private ConveyorController conveyor;

    // TUTORIAL
    // 1 reference
    private bool showTutorial = false;

    private void Awake()
    {
        settingBtn.onClick.AddListener(OnSettingBtnClickHandler);
    }

    private void Start()
    {
        SetTextCoin(DataManager.Ins.userData.coin);
    }

    private void OnEnable()
    {
        EventManager.AddListener<AddCoin>(OnAddCoinCallBack);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<AddCoin>(OnAddCoinCallBack);
    }

    private void OnAddCoinCallBack(AddCoin obj)
    {
        DOVirtual.Int(obj.currentCoin, obj.currentCoin + obj.coinAdd, 0.3f, (i) => { SetTextCoin(i); });
    }

    private void SetTextCoin(int coin)
    {
        coinText.text = $"{coin}";
    }

    private void OnSettingBtnClickHandler()
    {
        UIManager.Ins.OpenUI<SettingUI>();
    }

    [Button]
    public void SetupCamera()
    {
        SoundManager.Ins.PlaySoundBG(SoundBg.bg);
        Level currentLevel = LevelManager.Ins.currentLevel;
        levelText.text = $"Lv.{DataManager.Ins.userData.level + 1}";
        if (currentLevel == null || currentLevel.min == null || currentLevel.max == null)
        {
            return;
        }

        float paddingTop = 0.7f;
        float paddingLeft = 0.7f;
        float paddingRight = 0.7f;
        float paddingBottom = 0f;

        // Xóa conveyor cũ
        if (conveyor != null)
        {
            Destroy(conveyor.gameObject);
            conveyor = null;
        }

        // Convert UI → World
        Vector3 uiTopPos = Utils_Custom.ConvertUIToWorldPosition(top);
        Vector3 uiBottomPos = Utils_Custom.ConvertUIToWorldPosition(down);
        Vector3 uiLeftPos = Utils_Custom.ConvertUIToWorldPosition(left);
        Vector3 uiRightPos = Utils_Custom.ConvertUIToWorldPosition(right);

        float fullUiWidth = Mathf.Abs(uiRightPos.x - uiLeftPos.x);
        float availableWidth = fullUiWidth - (paddingLeft + paddingRight);

        float fullUiHeight = Mathf.Abs(uiTopPos.y - uiBottomPos.y);
        float availableHeight = fullUiHeight - (paddingTop + paddingBottom);

        if (availableWidth <= 0) availableWidth = 1f;
        if (availableHeight <= 0) availableHeight = 1f;

        Vector3 lvlMin = currentLevel.min.localPosition;
        Vector3 lvlMax = currentLevel.max.localPosition;

        float lvlWidth = Mathf.Abs(lvlMax.x - lvlMin.x);
        float lvlHeight = Mathf.Abs(lvlMax.y - lvlMin.y);
        float scaleX = availableWidth / lvlWidth;
        float scaleY = availableHeight / lvlHeight;
        float finalScale = Mathf.Min(scaleX, scaleY);
        finalScale = Mathf.Clamp(finalScale, 0.1f, 1f);

        currentLevel.levelRoot.transform.localScale = Vector3.one * finalScale;
        float availableAreaLeftX = uiLeftPos.x + paddingLeft;
        float availableAreaRightX = uiRightPos.x - paddingRight;
        float targetCenterX = (availableAreaLeftX + availableAreaRightX) / 2f;

        float lvlCenterX = (lvlMin.x + lvlMax.x) / 2f;

        float targetX = targetCenterX - lvlCenterX * finalScale;

        float targetBottomY = uiBottomPos.y + paddingBottom;
        float targetY = targetBottomY - lvlMin.y * finalScale;

        currentLevel.levelRoot.transform.position = new Vector3(targetX, targetY, currentLevel.transform.position.z);

        conveyor = Instantiate(
            conveyorPrefab,
            Utils_Custom.ConvertUIToWorldPosition(posConveyor, true),
            Quaternion.identity
        );
        conveyor.transform.localScale = Vector3.one * 0.9f;
        conveyor.transform.SetParent(LevelManager.Ins.transform);

        float uiLeftX = uiLeftPos.x + 0.2f;
        float uiRightX = uiRightPos.x - 0.2f;
        float maxAllowedWidth = uiRightX - uiLeftX;
        float conveyorWidth = conveyor.Visual.bounds.size.x;
        if (conveyorWidth > maxAllowedWidth)
        {
            float scaleFactor = maxAllowedWidth / conveyorWidth;
            conveyor.transform.localScale *= scaleFactor;
        }

        DOVirtual.DelayedCall(0.3f, () =>
        {
            if (DataManager.Ins.userData.level == 0)
            {
                StartTutorial();
            }

            Controller.Ins.isPlay = true;
        });
    }

    public void StartTutorial()
    {
        showTutorial = true;
        for (int i = 0; i < LevelManager.Ins.currentLevel.Stash.Count; i++)
        {
            if (i == 0)
            {
                LevelManager.Ins.currentLevel.Stash[i].CanPick = true;
            }
            else
            {
                LevelManager.Ins.currentLevel.Stash[i].CanPick = false;
            }
        }

        Transform unitTransform = LevelManager.Ins.currentLevel.Stash[0].transform;

        Tutorial.Ins.ButtonAction(unitTransform.position + Vector3.up * 0.2f, () => DoneStartTutirual());
        Tutorial.Ins.WorldClick(
            unitTransform.position + Vector3.right * 0.1f,
            Vector3.forward * 45f,
            15f
        );
    }

    // 1 reference
    public void DoneStartTutirual()
    {
        Tutorial.Ins.Off();
        if (!showTutorial)
        {
            for (int i = 0; i < LevelManager.Ins.currentLevel.Stash.Count; i++)
            {
                LevelManager.Ins.currentLevel.Stash[i].CanPick = true;
            }

            return;
        }


        showTutorial = false;
        DOVirtual.DelayedCall(1.4f, () =>
        {
            if (LevelManager.Ins.currentLevel.Stash.Count > 0)
            {
                for (int i = 0; i < LevelManager.Ins.currentLevel.Stash.Count; i++)
                {
                    if (i == 0)
                    {
                        LevelManager.Ins.currentLevel.Stash[i].CanPick = true;
                    }
                    else
                    {
                        LevelManager.Ins.currentLevel.Stash[i].CanPick = false;
                    }
                }

                Transform unitTransform = LevelManager.Ins.currentLevel.Stash[0].transform;
                Tutorial.Ins.ButtonAction(unitTransform.position + Vector3.up * 0.2f, () => DoneStartTutirual());
                Tutorial.Ins.WorldClick(
                    unitTransform.position + Vector3.right * 0.1f,
                    Vector3.forward * 45f,
                    15f
                );
            }
            else
            {
                Tutorial.Ins.Off();
            }
        });
    }
}