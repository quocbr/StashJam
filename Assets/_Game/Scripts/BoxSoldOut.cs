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
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && pos.Length > i && pos[i] != null)
                {
                    items[i].SetLayer("Fly", 10);
                    MoveToPos(items[i], pos[i]);
                }
            }

            // 🚨 THÊM KIỂM TRA NULL CHO Cap 🚨
            // Ngăn chặn lỗi khi BoxSoldOut bị Destroyed trước khi hàm này được gọi từ callback bên ngoài.
            if (Cap == null)
            {
                onComplete?.Invoke();
                return;
            }

            // Tạo tween cho Cap
            tween1 = Cap.transform.DOLocalMove(Vector3.zero, 0.5f)
                .SetDelay(0.4f)
                .SetTarget(gameObject) // Gắn target vào BoxSoldOut để OnDestroy() quản lý
                .OnComplete(() =>
                {
                    // Kiểm tra an toàn cho BoxSoldOut
                    if (this == null) return;

                    isRemove = true;
                    onComplete?.Invoke();
                })
                .OnStart(() =>
                {
                    if (Cap != null) Cap.gameObject.SetActive(true);
                });
        }
    }

    private void MoveToPos(Item source, Transform target)
    {
        if (target != null && source != null)
        {
            source.transform.SetParent(target);

            // Tạo tween cho Item. Dùng SetTarget trên Item để Item tự quản lý tween này
            tween2 = source.transform.DOLocalMove(Vector3.zero, 0.4f)
                .SetTarget(source.transform) // Gắn target vào Item.transform
                .OnComplete(() =>
                {
                    // Kiểm tra an toàn cho Item
                    if (source == null) return;

                    source.transform.localPosition = Vector3.zero;
                    source.SetLayer("Default", 10);

                    // Nếu cần hủy Item sau khi MoveToPos hoàn thành (logic SoldOut), bạn sẽ làm ở đây
                    // Ví dụ: Destroy(source.gameObject); 
                });
        }
    }
}