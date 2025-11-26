using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class StackStash : Stash
{
    [SerializeField] private Sprite[] l_PileSprite;
    [SerializeField] private SpriteRenderer pile;
    [SerializeField] private TextMeshPro text;

    public override void Init()
    {
        pile.sortingLayerID = SortingLayer.NameToID($"{7 - index.x}");
        text.sortingLayerID = SortingLayer.NameToID($"{7 - index.x}");
    }

    protected override void UpdateStack(int stack)
    {
        base.UpdateStack(stack);
        text.text = stack.ToString();

        if (stack == 0)
        {
            //MainAsset.transform.DOScale(0, 0.4f).SetEase(Ease.InBack);
            LevelManager.Ins.currentLevel.Stash.Remove(this);
            pile.transform.DOScale(1.1f, 0.3f).SetEase(Ease.InOutBack).SetLoops(2, LoopType.Yoyo);
        }
        else
        {
            pile.transform.DOScale(1.1f, 0.3f).SetEase(Ease.InOutBack).SetLoops(2, LoopType.Yoyo);
        }
    }

    public override void SetBoxDirection(BoxDirection direction)
    {
        base.SetBoxDirection(direction);
        switch (direction)
        {
            case BoxDirection.Up:
                pile.sprite = l_PileSprite[0];
                text.rectTransform.anchoredPosition = new Vector2(0, 0.3f);
                break;
            case BoxDirection.Down:
                pile.sprite = l_PileSprite[1];
                text.rectTransform.anchoredPosition = new Vector2(0, 0.15f);
                break;
            case BoxDirection.Left:
                pile.sprite = l_PileSprite[2];
                text.rectTransform.anchoredPosition = new Vector2(0, 0.05f);
                break;
            case BoxDirection.Right:
                pile.sprite = l_PileSprite[3];
                text.rectTransform.anchoredPosition = new Vector2(0, 0.05f);
                break;
        }
    }
}