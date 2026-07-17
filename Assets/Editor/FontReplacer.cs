using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class FontReplacer : EditorWindow
{
    private TMP_FontAsset newFont;

    [MenuItem("Tools/Replace All TMP Fonts")]
    public static void ShowWindow()
    {
        GetWindow<FontReplacer>("Font Replacer");
    }

    void OnGUI()
    {
        GUILayout.Label("모든 씬 + 프리팹의 TMP 폰트 일괄 교체", EditorStyles.boldLabel);
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("새 폰트", newFont, typeof(TMP_FontAsset), false);

        if (newFont == null)
        {
            EditorGUILayout.HelpBox("교체할 새 폰트를 먼저 지정하세요.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("프리팹 전체 교체"))
        {
            ReplaceInPrefabs();
        }

        if (GUILayout.Button("모든 씬 전체 교체 (열려있는 씬 저장 주의!)"))
        {
            ReplaceInAllScenes();
        }
    }

    void ReplaceInPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            TMP_Text[] texts = prefab.GetComponentsInChildren<TMP_Text>(true);
            bool changed = false;

            foreach (var text in texts)
            {
                if (text.font != newFont)
                {
                    Undo.RecordObject(text, "Replace Font");
                    text.font = newFont;
                    changed = true;
                    count++;
                }
            }

            if (changed)
                EditorUtility.SetDirty(prefab);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[FontReplacer] 프리팹에서 {count}개 텍스트 폰트 교체 완료");
    }

    void ReplaceInAllScenes()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        int totalCount = 0;

        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            TMP_Text[] texts = GameObject.FindObjectsOfType<TMP_Text>(true);
            int sceneCount = 0;

            foreach (var text in texts)
            {
                if (text.font != newFont)
                {
                    Undo.RecordObject(text, "Replace Font");
                    text.font = newFont;
                    sceneCount++;
                }
            }

            if (sceneCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            totalCount += sceneCount;
            Debug.Log($"[FontReplacer] {path} 에서 {sceneCount}개 교체");
        }

        Debug.Log($"[FontReplacer] 전체 씬 합계 {totalCount}개 텍스트 폰트 교체 완료");
    }
}