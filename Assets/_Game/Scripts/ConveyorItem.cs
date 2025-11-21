using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class ConveyorItem : MonoBehaviour
{
    [SerializeField] private Item item;
    public GameObject arrow;
    [ShowInInspector] private int m_Index;
    public int Index => m_Index;

    public bool IsEmpty => item == null;
    public Item CurrentItem => item;

    public void SetItem(Item item)
    {
        this.item = item;
        if (item == null)
        {
            return;
        }

        item.transform.SetParent(Controller.Ins.transform);
        item.AnimBackToRoot(transform);
    }

    public void SetIndex(int index)
    {
        m_Index = index;
    }

    public void Earn()
    {
        Item it = item;
        DOVirtual.DelayedCall(0.2f,
            () =>
            {
                it.transform.SetParent(Controller.Ins.transform);
                it.SetLayer("Fly", 10);
                it.transform.DOScale(1.2f, 0.3f).OnComplete(() =>
                {
                    //it.transform.DOScale(0, 0.3f).OnComplete(() => { it.gameObject.SetActive(false); });
                });
            });
        SetItem(null);
    }
}