using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<Level> allLevel;

    public Level currentLevel;

    [Button]
    public void SpawnLevel(int index)
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel.gameObject);
            currentLevel = null;
        }

        index = Math.Clamp(index, 0, allLevel.Count - 1);

        currentLevel = Instantiate(allLevel[index], transform);
        currentLevel.Init();
    }
}