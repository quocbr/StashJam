using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Stash : MonoBehaviour
{
    [SerializeField] private List<Item> m_ListItem;
    public Vector2Int index;
    public bool CanPick = true;

    public List<Item> ListItem => m_ListItem;

    public void Init()
    {
        for (int i = 0; i < m_ListItem.Count; i++)
        {
        }

        CanPick = true;
    }

    public void OnPick()
    {
        CanPick = false;
        transform.DOScale(0, 0.5f).SetDelay(0.4f).SetEase(Ease.InBack);
    }
}