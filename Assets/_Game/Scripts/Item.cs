using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int ID;
    [SerializeField] private SpriteRenderer m_ItemSprite;

    public void Init(int id, Sprite sprite, int layer = 0)
    {
        if (m_ItemSprite == null)
        {
            m_ItemSprite.gameObject.SetActive(false);
            return;
        }

        m_ItemSprite.gameObject.SetActive(true);
        ID = id;
        m_ItemSprite.sprite = sprite;
        m_ItemSprite.sortingLayerID = SortingLayer.NameToID($"{layer}");
    }

    public void AnimBackToRoot(Transform parent)
    {
        m_ItemSprite.sortingOrder = 20;
        m_ItemSprite.sortingLayerID = SortingLayer.NameToID("Fly");

        transform.DOScale(1.1f, 0.3f).OnComplete(() =>
        {
            transform.SetParent(parent);
            transform.DOScale(1f, 0.3f);
            transform.DOLocalMove(Vector3.zero, 0.3f).OnComplete(() => { m_ItemSprite.sortingOrder = 1; });
        });
    }
}