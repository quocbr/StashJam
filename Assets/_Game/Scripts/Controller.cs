using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStashPick : IGameEvent
{
    public List<Item> listItem;
    public Stash Stash;
}

public class OnStashDestroy : IGameEvent
{
    public Stash Stash;
}

public class Controller : Singleton<Controller>
{
    public bool isPlay = false;
    [SerializeField] private LayerMask filterLayer;
    private Stash m_StashChoose;
    private Ray ray;

    private void Update()
    {
        //Logic button up
        if (Input.GetMouseButtonUp(0))
        {
            OnButtonUp();
        }
    }

    private void OnButtonUp()
    {
        if (!isPlay)
        {
            return;
        }

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, filterLayer);

        if (hit.collider != null)
        {
            HandleRaycastItemPick(hit);
        }
    }

    private void HandleRaycastItemPick(RaycastHit2D hit)
    {
        m_StashChoose = hit.collider.GetComponent<Stash>();
        if (!m_StashChoose.CanPick || m_StashChoose.isLock || m_StashChoose.isEat)
        {
            return;
        }

        SoundManager.Ins.PlaySoundFx(SoundFx.ClickStask);
        m_StashChoose.OnPick();

        OnStashPick cb = new OnStashPick();
        cb.Stash = m_StashChoose;
        cb.listItem = m_StashChoose.ListItem;
        EventManager.Trigger(cb);
        m_StashChoose = null;
    }

    public void DestroyAllChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }
}