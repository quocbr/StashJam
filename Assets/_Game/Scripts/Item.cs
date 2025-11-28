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
    public TrailRenderer trail;

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
            trail.sortingLayerID = SortingLayer.NameToID($"{layer}");
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
            skeletonAnimation.initialSkinName = newSkinName;
            skeleton.SetSkin(newSkin);
            skeleton.SetSlotsToSetupPose();
            skeletonAnimation.AnimationState.Apply(skeleton);
#if UNITY_EDITOR
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

        DOVirtual.DelayedCall(0.2f, () =>
        {
            if (this == null) return;

            transform.SetParent(parent);

            // Scale về 1
            transform.DOScale(1f, 0.4f).SetTarget(transform);

            float jumpPower = 1.5f;
            int numJumps = 1;

            transform.DOLocalJump(Vector3.zero, jumpPower, numJumps, Random.Range(0.2f, 0.4f))
                .SetEase(Ease.Linear)
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