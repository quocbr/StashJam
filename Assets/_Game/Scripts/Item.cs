using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int ID;
    [SerializeField] private SpriteRenderer m_ItemSprite;
    [SerializeField] private SpriteRenderer m_Hidden;
    public bool isEat = false;

    public void Init(int id, Sprite sprite, int layer = 0, bool isHidden = false, int index = 0)
    {
        if (m_ItemSprite == null)
        {
            m_ItemSprite.gameObject.SetActive(false);
            return;
        }

        m_ItemSprite.gameObject.SetActive(true);
        ID = id;
        m_ItemSprite.sprite = sprite;
        m_Hidden.gameObject.SetActive(isHidden);
        m_ItemSprite.sortingLayerID = SortingLayer.NameToID($"{layer}");
        m_Hidden.sortingLayerID = SortingLayer.NameToID($"{layer}");
        if (index == 0)
        {
            m_ItemSprite.sortingOrder = 1;
        }
        else if (index == 1 || index == 2)
        {
            m_ItemSprite.sortingOrder = 2;
        }
        else
        {
            m_ItemSprite.sortingOrder = 3;
        }
    }


    public void AnimBackToRoot(Transform parent)
    {
        m_Hidden.gameObject.SetActive(false);
        m_ItemSprite.sortingOrder = 20;
        m_ItemSprite.sortingLayerID = SortingLayer.NameToID("Fly");

        transform.DOScale(1.1f, 0.3f).OnComplete(() =>
        {
            transform.SetParent(parent);
            transform.DOScale(1f, 0.3f);
            transform.DOLocalMove(Vector3.zero, 0.3f).OnComplete(() => { m_ItemSprite.sortingOrder = 1; });
        });
    }

    public void SetLayer(string layerName, int orderLayer)
    {
        m_ItemSprite.sortingLayerName = layerName;
        m_ItemSprite.sortingOrder = orderLayer;
    }
}