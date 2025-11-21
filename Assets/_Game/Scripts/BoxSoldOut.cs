using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BoxSoldOut : MonoBehaviour
{
    public GameObject Cap;
    public GameObject Visual;
    [SerializeField] private Transform[] pos;

    public void FlyToBox(List<Item> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].transform.SetParent(transform);
            items[i].SetLayer("Fly", 10);
            MoveToPos(items[i], pos[i]);
        }

        Visual.transform.DOLocalMove(Vector3.zero, 0.15f).From(Vector3.down).OnComplete(() =>
        {
            Cap.transform.DOLocalMove(Vector3.zero, 0.3f).SetDelay(0.1f).OnComplete(() =>
            {
                transform.DOMoveX(transform.position.x - 5f, 0.3f).OnComplete(() => { Destroy(gameObject); });
            }).OnStart(() => Cap.gameObject.SetActive(true));
        });

        DOVirtual.DelayedCall(0.2f, () => { });
    }

    private void MoveToPos(Item source, Transform target)
    {
        source.transform.DOMove(target.position, 0.3f).OnComplete(() => { source.SetLayer("Default", 1); });
    }
}