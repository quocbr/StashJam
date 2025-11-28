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
    [SerializeField] private List<SpriteRenderer> sprite;
    public bool isRemove = false;

    private Tween tween1;
    private Tween tween2;

    private void OnDestroy()
    {
        // Đảm bảo các tweens được tạo trong lớp này được hủy khi đối tượng BoxSoldOut bị hủy
        if (tween1 != null) tween1.Kill();
        if (tween2 != null) tween2.Kill();

        // Hoặc an toàn hơn, hủy tất cả tweens trên GameObject này
        transform.DOKill();
    }

    public void Init(int index)
    {
        if (Visual != null)
        {
            Visual.sortingOrder = index;
        }
    }

    public void FlyToBox(List<Item> items, Action onComplete = null)
    {
        if (items.Count > 0)
        {
            DOVirtual.DelayedCall(0.1f, () =>
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] != null && pos.Length > i && pos[i] != null)
                    {
                        items[i].SetLayer("Fly", 10);
                        MoveToPos(items[i], i);
                    }
                }

                if (Cap == null)
                {
                    onComplete?.Invoke();
                    return;
                }

                DOVirtual.DelayedCall(0.8f, () =>
                {
                    if (this == null) return;
                    isRemove = true;
                    onComplete?.Invoke();
                });
            });
        }
    }

    private void MoveToPos(Item source, int index)
    {
        if (pos[index] != null && source != null)
        {
            Utils_Custom.PlayAnimation(source.skeletonAnimation, "Idle");
            source.transform.SetParent(pos[index]);
            tween2 = source.transform.DOLocalMove(Vector3.zero, 0.4f)
                .SetTarget(source.transform)
                .OnComplete(() =>
                {
                    if (source == null) return;
                    sprite[index].sprite = source.Sprite;
                    sprite[index].enabled = true;
                    Destroy(source.gameObject);
                    //source.transform.localPosition = Vector3.zero;
                    //source.SetLayer("Default", 10);
                });
        }
    }
}