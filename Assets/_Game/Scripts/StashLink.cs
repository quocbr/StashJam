using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class StashLink : MonoBehaviour
{
    [LabelText("Link")] [SerializeField] private SpriteRenderer link;

    [SerializeField] private SpriteRenderer currentLink;

    public void ApplyConfig(BoxConfig config)
    {
        if (link != null)
        {
            link.sortingLayerID = SortingLayer.NameToID($"{7 - config.gridPos.y}");
        }

        if (currentLink != null)
        {
            currentLink.sortingLayerID = SortingLayer.NameToID($"{7 - config.gridPos.y}");
        }
    }
}