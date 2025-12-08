#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ChangeScene : Editor
{
    [MenuItem("Open Scene/1. Loading #1")]
    public static void OpenLoading()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/VoodooPackages/TinySauce/Preload/Scene/TSPreloadScene.unity");
        }
    }

    [MenuItem("Open Scene/2. Main #2")]
    public static void OpenHome()
    {
        OpenScene("Main");
    }

    [MenuItem("Open Scene/3. Monster #3")]
    public static void OpenBarn()
    {
        OpenScene("Monster");
    }

    [MenuItem("Open Scene/4. Campaign #4")]
    public static void OpenCampaign()
    {
        OpenScene("Campaign");
    }

    [MenuItem("Open Scene/5. Battle #5")]
    public static void OpenBattle()
    {
        OpenScene("Battle");
    }

    [MenuItem("Open Scene/PvP Home #6")]
    public static void OpenPvPHome()
    {
        OpenScene("PVPHome");
    }

    [MenuItem("Open Scene/Pvp Battle #7")]
    public static void OpenPvPBattle()
    {
        OpenScene("PvP Battle");
    }

    private static void OpenScene(string sceneName)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/" + sceneName + ".unity");
        }
    }

    [MenuItem("Piti Menu/Clear All Data")]
    public static void ClearAllData()
    {
        string fileName;
#if UNITY_EDITOR
        fileName = "/data.piti";
#elif !UNITY_STANDALONE
        fileName = "/data.piti";
#else
        fileName = "/data1.piti";
#endif
        File.Delete(Application.persistentDataPath + fileName);
        PlayerPrefs.DeleteAll();
    }
}

#endif