using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BoxSoldOut : MonoBehaviour
{
    public GameObject Cap;
    public SpriteRenderer Visual;
    [SerializeField] private Transform[] pos;
    public bool isRemove = false;

    public void Init(int index)
    {
        Visual.sortingOrder = index;
    }

    public void FlyToBox(List<Item> items, Action onComplete = null)
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetLayer("Fly", 10);
            MoveToPos(items[i], pos[i]);
        }

        Cap.transform.DOLocalMove(Vector3.zero, 0.5f).SetDelay(0.4f).OnComplete(() =>
        {
            isRemove = true;
            onComplete?.Invoke();
        }).OnStart(() => Cap.gameObject.SetActive(true));
    }

    private void MoveToPos(Item source, Transform target)
    {
        source.transform.SetParent(target);
        source.transform.DOLocalMove(Vector3.zero, 0.4f).OnComplete(() =>
        {
            source.transform.localPosition = Vector3.zero;
            source.SetLayer("Default", 10);
        });
    }
}