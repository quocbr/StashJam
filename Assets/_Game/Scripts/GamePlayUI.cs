using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GamePlayUI : MonoBehaviour
{
    public RectTransform t1;
    public RectTransform t2;
    public RectTransform xmin;
    public RectTransform xmax;

    [Button]
    public void SetupCamera()
    {
        // 1. Calculate bounds of the level (O)
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);


        // 2. Calculate UI area in world space (P)
        Vector3 uiTop = Utils_Custom.ConvertUIToWorldPosition(t1);
        Vector3 uiBottom = Utils_Custom.ConvertUIToWorldPosition(t2);
        Vector3 uiLeft = Utils_Custom.ConvertUIToWorldPosition(xmin);
        Vector3 uiRight = Utils_Custom.ConvertUIToWorldPosition(xmax);

        Vector3 uiTopRight = new Vector3(uiRight.x - 0.2f, uiTop.y, 0);
        Vector3 uiBottomLeft = new Vector3(uiLeft.x + 0.2f, uiBottom.y, 0);

        LevelManager.Ins.currentLevel.transform.position = new Vector3(
            0,
            (uiTop.y + uiBottom.y) / 2f,
            0
        );

        max = LevelManager.Ins.currentLevel.max.transform.position;
        min = LevelManager.Ins.currentLevel.min.transform.position;


        Vector2 sizeO = new Vector2(max.x - min.x, max.y - min.y);

        float x1 = Math.Abs(Math.Abs(uiTopRight.x) - Math.Abs(max.x));
        float y1 = Math.Abs(Math.Abs(uiTopRight.y) - Math.Abs(max.y));
        float x2 = Math.Abs(Math.Abs(uiBottomLeft.x) - Math.Abs(min.x));
        float y2 = Math.Abs(Math.Abs(uiBottomLeft.y) - Math.Abs(min.y));

        float minDistance = Mathf.Min(x1, y1, x2, y2);
        float padding = 0.6f;
        float size = 1;
        if (x1 == minDistance)
        {
            size = Math.Abs(uiTopRight.x) / Math.Abs(max.x + padding);
        }
        else if (y1 == minDistance)
        {
            size = Math.Abs(uiTopRight.y) / Math.Abs(max.y + padding);
        }
        else if (x2 == minDistance)
        {
            size = Math.Abs(uiBottomLeft.x) / Math.Abs(min.x - padding);
        }
        else if (y2 == minDistance)
        {
            size = Math.Abs(uiBottomLeft.y) / Math.Abs(min.y - padding);
        }


        Vector2 sizeP = new Vector2(
            Mathf.Abs(uiTopRight.x - uiBottomLeft.x),
            Mathf.Abs(uiTopRight.y - uiBottomLeft.y)
        );

        // 3. Calculate scale factor (fit O inside P)
        float scaleX = sizeP.x / sizeO.x;
        float scaleY = sizeP.y / sizeO.y;
        float finalScale = Mathf.Min(scaleX, scaleY) * 0.65f; // 0.9f for padding

        // 4. Apply scale to level
        //LevelSpawner.Ins.CurrentLevel.transform.localScale = Vector3.one * finalScale;
        LevelManager.Ins.currentLevel.transform.localScale = Vector3.one * size;

        // 5. Center the level vertically between UI top and bottom
        Vector3 pos1 = LevelManager.Ins.currentLevel.transform.position;
        LevelManager.Ins.currentLevel.transform.position = new Vector3(
            pos1.x,
            (uiTop.y + uiBottom.y) / 2f,
            pos1.z
        );
    }
}
