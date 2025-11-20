using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BoxSoldOut : MonoBehaviour
{
    public GameObject Cap;
    [SerializeField] private Transform[] pos;

    public void FlyToBox(List<Item> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].transform.SetParent(transform);
            MoveToPos(items[i].transform, pos[i]);
        }

        DOVirtual.DelayedCall(0.5f, () =>
        {
            Cap.gameObject.SetActive(true);
            Cap.transform.DOLocalMove(Vector3.zero, 0.3f).OnComplete(() =>
            {
                transform.DOMoveX(transform.position.x - 5f, 1f).OnComplete(() => { Destroy(gameObject); });
            });
        });
    }

    private void MoveToPos(Transform source, Transform target)
    {
        source.DOMove(target.position, 0.4f).OnComplete(() => { });
    }
}