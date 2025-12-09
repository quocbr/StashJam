using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelManager : Singleton<LevelManager>
{
    public List<Level> allLevel;
    public ItemDatabase itemDatabase;

    public Level currentLevel;

    [Button]
    public void SpawnLevel(int index)
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel.gameObject);
            currentLevel = null;
        }

        Controller.Ins.isPlay = false;

        if (index >= allLevel.Count)
        {
            index = Random.Range(0, allLevel.Count);
        }

        index = Math.Clamp(index, 0, allLevel.Count - 1);

        currentLevel = Instantiate(allLevel[index], transform);
        currentLevel.Init();

        if (!GameManager.Ins.isCheatMode)
        {
            TinySauce.OnGameStarted(index + 1);
        }

        GamePlayUI.Ins.SetupCamera();
    }

    public void Replay()
    {
        SpawnLevel(DataManager.Ins.userData.level);
    }
}