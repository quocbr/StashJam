using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class GamePlayUI : Singleton<GamePlayUI>
{
    public ConveyorController conveyorPrefab;
    public TextMeshProUGUI levelText;
    public RectTransform top;
    public RectTransform down;
    public RectTransform left;
    public RectTransform right;

    public RectTransform posConveyor;
    private ConveyorController conveyor;

    [Button]
    public void SetupCamera()
    {
        Level currentLevel = LevelManager.Ins.currentLevel;
        levelText.text = $"Level {DataManager.Ins.userData.level + 1}";
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

        // Lấy min/max của level (local)
        Vector3 lvlMin = currentLevel.min.localPosition;
        Vector3 lvlMax = currentLevel.max.localPosition;

        float lvlWidth = Mathf.Abs(lvlMax.x - lvlMin.x);
        float lvlHeight = Mathf.Abs(lvlMax.y - lvlMin.y);

        // Scale level vừa UI
        float scaleX = availableWidth / lvlWidth;
        float scaleY = availableHeight / lvlHeight;
        float finalScale = Mathf.Min(scaleX, scaleY);
        finalScale = Mathf.Clamp(finalScale, 0.1f, 1f);

        currentLevel.transform.localScale = Vector3.one * finalScale;

        // Tính vị trí đặt Level cho vừa UI
        float availableAreaLeftX = uiLeftPos.x + paddingLeft;
        float availableAreaRightX = uiRightPos.x - paddingRight;
        float targetCenterX = (availableAreaLeftX + availableAreaRightX) / 2f;

        float lvlCenterX = (lvlMin.x + lvlMax.x) / 2f;

        float targetX = targetCenterX - lvlCenterX * finalScale;

        float targetBottomY = uiBottomPos.y + paddingBottom;
        float targetY = targetBottomY - lvlMin.y * finalScale;

        currentLevel.transform.position = new Vector3(targetX, targetY, currentLevel.transform.position.z);


        // ============================================================================================
        //                                  >>> CONVEYOR <<<
        // ============================================================================================

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
        //float conveyorHalfWidth = conveyorWidth * 0.5f;

        if (conveyorWidth > maxAllowedWidth)
        {
            float scaleFactor = maxAllowedWidth / conveyorWidth;

            conveyor.transform.localScale *= scaleFactor;

            //conveyorWidth = conveyor.Visual.bounds.size.x;
            //conveyorHalfWidth = conveyorWidth * 0.5f;
        }

        // Vector3 cPos = conveyor.transform.position;
        //
        // float minX = uiLeftX + conveyorHalfWidth;
        // float maxX = uiRightX - conveyorHalfWidth;
        //
        // if (minX > maxX)
        // {
        //     cPos.x = (minX + maxX) / 2f;
        // }
        // else
        // {
        //     cPos.x = Mathf.Clamp(cPos.x, minX, maxX);
        // }
        //
        // conveyor.transform.position = cPos;
    }
}