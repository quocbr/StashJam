using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class StackStash : Stash
{
    [SerializeField] private SpriteRenderer pile;
    [SerializeField] private TextMeshPro text;

    public override void Init()
    {
        pile.sortingLayerID = SortingLayer.NameToID($"{7 - index.y + 1}");
        text.sortingLayerID = SortingLayer.NameToID($"{7 - index.y + 1}");
    }

    protected override void UpdateStack(int stack)
    {
        base.UpdateStack(stack);
        text.text = stack.ToString();
        if (stack == 0)
        {
            MainAsset.transform.DOScale(0, 0.4f).SetEase(Ease.InBack);
            LevelManager.Ins.currentLevel.Stash.Remove(this);
        }
    }
}