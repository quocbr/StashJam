using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct UnlockFeature
{
    public int levelUnlock;
    public Sprite spriteLock;
    public Sprite spriteUnlock;
}

public class GameManager : Singleton<GameManager>
{
    public List<UnlockFeature> UnlockFeatures;
    public RectTransform MainCanvasRect;
}