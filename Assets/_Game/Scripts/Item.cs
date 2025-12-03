using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Item : MonoBehaviour
{
    public int ID;

    //[SerializeField] private SpriteRenderer m_ItemSprite;
    public SkeletonAnimation skeletonAnimation;
    [SerializeField] private SpriteRenderer m_Hidden;
    public bool isEat = false;
    public Sprite Sprite;
    public TrailRenderer trail;
    public MMF_Player squashAndStretch;

    public UnityEvent flash;

    private void OnDestroy()
    {
        transform.DOKill();
    }

    [Button]
    public void SquashAndStretch()
    {
        squashAndStretch.PlayFeedbacks();
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

        trail.gameObject.SetActive(false);
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

    public void AnimBackToRoot(Transform parent, Action onComplete = null)
    {
        SetVisualHidden(false);
        trail.gameObject.SetActive(true);

        if (skeletonAnimation != null)
        {
            var meshRenderer = skeletonAnimation.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = 20;
                meshRenderer.sortingLayerID = SortingLayer.NameToID("Fly");
                Utils_Custom.PlayAnimation(skeletonAnimation, "Idle");
            }
        }

        transform.DOKill();

        transform.SetParent(parent);

        Sequence seq = DOTween.Sequence();

        float duration = Random.Range(0.35f, 0.4f);
        float jumpPower = Random.Range(1f, 1.5f);

        seq.Append(transform.DOLocalJump(Vector3.zero, jumpPower, 1, duration)
            .SetEase(Ease.OutQuad));
        seq.Join(transform.DOLocalRotate(Vector3.zero, duration).SetEase(Ease.OutSine));
        seq.Join(transform.DOScale(Vector3.one, duration).SetEase(Ease.OutSine));
        seq.OnComplete(() =>
        {
            if (this == null) return;
            if (skeletonAnimation != null)
            {
                var meshRenderer = skeletonAnimation.GetComponent<MeshRenderer>();
                if (meshRenderer != null) meshRenderer.sortingOrder = 1;
            }

            var x = PoolManager.Ins.Spawn<ParticleSystem>(PoolName.SplashFX, transform);
            x.transform.localPosition = Vector3.zero;
            SquashAndStretch();
            trail.gameObject.SetActive(false);
            flash?.Invoke();
            onComplete?.Invoke();
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