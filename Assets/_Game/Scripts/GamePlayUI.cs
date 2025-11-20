using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GamePlayUI : Singleton<GamePlayUI>
{
    public RectTransform top;
    public RectTransform down;
    public RectTransform left;
    public RectTransform right;

    [Button]
    public void SetupCamera()
    {
        Level currentLevel = LevelManager.Ins.currentLevel;

        if (currentLevel == null || currentLevel.min == null || currentLevel.max == null)
        {
            return;
        }

        float paddingTop = 0.7f;
        float paddingLeft = 0.7f;
        float paddingRight = 0.7f;
        float paddingBottom = 0f;

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

        currentLevel.transform.localScale = Vector3.one * finalScale;

        float availableAreaLeftX = uiLeftPos.x + paddingLeft;
        float availableAreaRightX = uiRightPos.x - paddingRight;
        float targetCenterX = (availableAreaLeftX + availableAreaRightX) / 2f;

        float lvlCenterX = (lvlMin.x + lvlMax.x) / 2f;

        float targetX = targetCenterX - lvlCenterX * finalScale;

        float targetBottomY = uiBottomPos.y + paddingBottom;

        float targetY = targetBottomY - lvlMin.y * finalScale;
        currentLevel.transform.position = new Vector3(targetX, targetY, currentLevel.transform.position.z);
    }
}