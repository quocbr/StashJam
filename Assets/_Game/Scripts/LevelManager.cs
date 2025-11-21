using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelManager : Singleton<LevelManager>
{
    [SerializeField] private List<Level> allLevel;
    public ItemDatabase itemDatabase;
    public ConveyorController conveyorController;

    public Level currentLevel;

    [Button]
    public void SpawnLevel(int index)
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel.gameObject);
            currentLevel = null;
        }

        conveyorController.ResetConveyor();

        if (index >= allLevel.Count)
        {
            index = Random.Range(0, allLevel.Count);
        }

        index = Math.Clamp(index, 0, allLevel.Count - 1);

        currentLevel = Instantiate(allLevel[index], transform);
        currentLevel.Init();
        GamePlayUI.Ins.SetupCamera();
    }
}