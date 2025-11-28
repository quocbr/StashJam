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
    public Sprite Sprite;

    private void OnDestroy()
    {
        transform.DOKill();
    }

    public void Init(int id, Sprite sprite, int layer = 0, bool isHidden = false, int index = 0)
    {
        skeletonAnimation.gameObject.SetActive(!isHidden);
        ID = id;
        SetItem(id + 1);
        Sprite = sprite;

        if (m_Hidden != null)
        {
            m_Hidden.gameObject.SetActive(isHidden);
        }

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
        var newSkinName = "fish_" + id.ToString();
        var skeleton = skeletonAnimation.Skeleton;
        var newSkin = skeleton.Data.FindSkin(newSkinName);

        if (newSkin != null)
        {
            // 1. Gán giá trị Initial Skin mới
            skeletonAnimation.initialSkinName = newSkinName; // <--- Dòng mới

            // 2. Thiết lập Skin và cập nhật hiển thị ngay lập tức (như trước)
            skeleton.SetSkin(newSkin);
            skeleton.SetSlotsToSetupPose();
            skeletonAnimation.AnimationState.Apply(skeleton);

            // 3. Nếu đang trong Editor, đánh dấu thay đổi để lưu vào scene file
#if UNITY_EDITOR
            // Cần phải có using UnityEditor;
            UnityEditor.EditorUtility.SetDirty(skeletonAnimation);
#endif
        }
    }


    public void AnimBackToRoot(Transform parent)
    {
        SetVisualHidden(false);

        if (skeletonAnimation != null)
        {
            skeletonAnimation.GetComponent<MeshRenderer>().sortingOrder = 20;
            skeletonAnimation.GetComponent<MeshRenderer>().sortingLayerID = SortingLayer.NameToID("Fly");
            Utils_Custom.PlayAnimation(skeletonAnimation, "Idle");
        }


        // // Bắt đầu chuỗi Tween. SetTarget được thêm để tăng cường độ an toàn
        //transform.DOScale(1.1f, 0.2f)
        //.SetTarget(transform)
        //.OnComplete(() =>
        DOVirtual.DelayedCall(0.2f, () =>
        {
            if (this == null) return;

            transform.SetParent(parent);

            // SetTarget cho Tween 2
            transform.DOScale(1f, 0.4f).SetTarget(transform);

            // SetTarget cho Tween 3
            transform.DOLocalMove(Vector3.zero, 0.4f)
                .SetTarget(transform)
                .OnComplete(() =>
                {
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