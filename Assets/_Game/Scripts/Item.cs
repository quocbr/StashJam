using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int ID;
    [SerializeField] private SpriteRenderer m_ItemSprite;

    public void AnimBackToRoot(Transform parent)
    {
        m_ItemSprite.sortingOrder = 20;

        transform.DOScale(0.7f, 0.3f).OnComplete(() =>
        {
            transform.SetParent(parent);
            transform.DOScale(0.5f, 0.3f);
            transform.DOLocalMove(Vector3.zero, 0.3f).OnComplete(() => { m_ItemSprite.sortingOrder = 1; });
        });
    }
}