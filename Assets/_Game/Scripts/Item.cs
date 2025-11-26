using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int ID;

    //[SerializeField] private SpriteRenderer m_ItemSprite;
    public SkeletonAnimation skeletonAnimation;
    [SerializeField] private SpriteRenderer m_Hidden;
    public bool isEat = false;

    private void OnDestroy()
    {
        // 🛠️ FIX 1: Hủy tất cả tweens đang chạy trên Transform này khi đối tượng bị hủy.
        // Ngăn chặn lỗi "Target or field is missing/null" xảy ra khi transform bị hủy.
        transform.DOKill();
    }

    public void Init(int id, Sprite sprite, int layer = 0, bool isHidden = false, int index = 0)
    {
        skeletonAnimation.gameObject.SetActive(!isHidden);
        ID = id;
        SetItem(id + 1);

        if (m_Hidden != null)
        {
            m_Hidden.gameObject.SetActive(isHidden);
        }

        //m_ItemSprite.enabled = !isHidden;

        // Cần đảm bảo m_ItemSprite và m_Hidden không null trước khi thiết lập sorting layer
        if (skeletonAnimation != null)
        {
            skeletonAnimation.GetComponent<MeshRenderer>().sortingLayerID = SortingLayer.NameToID($"{layer}");
            skeletonAnimation.GetComponent<MeshRenderer>().sortingOrder =
                index == 0 ? 1 : index == 1 || index == 2 ? 2 : 3;
        }

        if (m_Hidden != null)
        {
            m_Hidden.sortingLayerID = SortingLayer.NameToID($"{layer}");
            m_Hidden.sortingOrder = index == 0 ? 1 : index == 1 || index == 2 ? 2 : 3;
        }
    }

    private void SetItem(int id)
    {
        var skeleton = skeletonAnimation.Skeleton;
        var newSkin = skeleton.Data.FindSkin("fish_" + id.ToString());
        if (newSkin != null)
        {
            skeleton.SetSkin(newSkin);
            skeleton.SetSlotsToSetupPose();
            skeletonAnimation.AnimationState.Apply(skeleton);
        }
    }


    public void AnimBackToRoot(Transform parent)
    {
        SetVisualHidden(false);

        if (skeletonAnimation != null)
        {
            skeletonAnimation.GetComponent<MeshRenderer>().sortingOrder = 20;
            skeletonAnimation.GetComponent<MeshRenderer>().sortingLayerID = SortingLayer.NameToID("Fly");
        }


        // Bắt đầu chuỗi Tween. SetTarget được thêm để tăng cường độ an toàn
        transform.DOScale(1.1f, 0.3f)
            .SetTarget(transform)
            .OnComplete(() =>
            {
                // Kiểm tra an toàn trước khi thực hiện các hành động tiếp theo
                if (this == null) return;

                transform.SetParent(parent);

                // SetTarget cho Tween 2
                transform.DOScale(1f, 0.3f).SetTarget(transform);

                // SetTarget cho Tween 3
                transform.DOLocalMove(Vector3.zero, 0.3f)
                    .SetTarget(transform)
                    .OnComplete(() =>
                    {
                        // Kiểm tra an toàn trước khi truy cập m_ItemSprite
                        if (skeletonAnimation != null)
                        {
                            skeletonAnimation.GetComponent<MeshRenderer>().sortingOrder = 1;
                        }
                    });
            });
    }

    public void SetLayer(string layerName, int orderLayer)
    {
        // 🛠️ FIX 2: THÊM KIỂM TRA NULL cho m_ItemSprite
        // Ngăn chặn lỗi "The object of type 'SpriteRenderer' has been destroyed but you are still trying to access it"
        if (skeletonAnimation == null)
        {
            return;
        }

        skeletonAnimation.GetComponent<MeshRenderer>().sortingLayerName = layerName;
        skeletonAnimation.GetComponent<MeshRenderer>().sortingOrder = orderLayer;
    }

    public void SetVisualHidden(bool isShow)
    {
        if (skeletonAnimation != null)
        {
            skeletonAnimation.gameObject.SetActive(!isShow);
        }

        if (m_Hidden != null)
        {
            m_Hidden.gameObject.SetActive(isShow);
        }
    }
}