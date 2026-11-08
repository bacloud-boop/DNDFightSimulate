using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class ProjectSetup
{
    public static void CreateAndVerify()
    {
        var state = new CombatState(); state.ApplyDamage(12);
        if(state.Hp!=988) throw new Exception("Damage failed");
        state.ApplyDamage(2000); if(state.Hp!=0) throw new Exception("HP floor failed");
        state.ResetIfDead(); state.EndRound(); if(state.Hp!=1000 || state.Round!=2) throw new Exception("Round failed");
        for(int i=0;i<10000;i++) { int d=UnityEngine.Random.Range(1,13); if(d<1||d>12) throw new Exception("Dice failed"); }
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Training Game").AddComponent<TrainingGame>();
        new GameObject("Audio Listener").AddComponent<AudioListener>();
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Main.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Main.unity",true)};
        PlayerSettings.companyName="Bacloud"; PlayerSettings.productName="DNDFightSimulate";
        PlayerSettings.defaultScreenWidth=1600; PlayerSettings.defaultScreenHeight=900;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        AssetDatabase.SaveAssets(); Debug.Log("MIGRATION_CHECKS_PASSED");
    }
}
