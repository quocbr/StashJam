using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MaskTransitions;
using UnityEngine;

[Serializable]
public struct UnlockFeature
{
    public int levelUnlock;
    public Sprite spriteLock;
    public Sprite spriteUnlock;
    public string Title;
    public string Description;
}

public class GameManager : Singleton<GameManager>
{
    public List<UnlockFeature> UnlockFeatures;
    public RectTransform MainCanvasRect;

    private void Start()
    {
        UnlockFeatures.Sort((a, b) => a.levelUnlock.CompareTo(b.levelUnlock));
        TransitionManager.Instance.PlayEndHalfTransition(0.8f, 0.4f);
    }
}