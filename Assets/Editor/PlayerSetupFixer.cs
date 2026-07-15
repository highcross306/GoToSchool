// ============================================================
// PlayerSetupFixer.cs
// 역할: 씬 / 프리팹에 있는 Player 오브젝트를 코드로 직접 교정한다.
//       - 레거시 Animation 컴포넌트 제거
//       - Animator 추가 + Player.controller 할당 + Root Motion 해제
//       - PlayerAnimationController 추가
//       - SpriteRenderer 스프라이트 비어있으면 채움
//       손으로 Inspector를 만지다 생기는 실수를 원천 차단한다.
//
// 위치: Assets/Editor/PlayerSetupFixer.cs   (반드시 Editor 폴더!)
// 사용: Tools → GoToSchool → [1] 씬의 Player 고치기
//       Tools → GoToSchool → [2] Player 프리팹 고치기
//       Tools → GoToSchool → [3] 씬 상태 진단만 하기
// ============================================================

using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PlayerSetupFixer
{
    private const string ControllerPath = "Assets/Animations/Player.controller";
    private const string IdleSpritePath = "Assets/Sprites/character.png";

    // ============================================================
    // [1] 현재 열려 있는 씬의 Player 고치기
    // ============================================================
    [MenuItem("Tools/GoToSchool/[1] 씬의 Player 고치기")]
    public static void FixScenePlayer()
    {
        // 비활성 오브젝트까지 포함해서 찾는다
        PlayerMover[] movers = Object.FindObjectsOfType<PlayerMover>(true);

        if (movers.Length == 0)
        {
            Debug.LogError("[Fixer] 씬에 PlayerMover를 가진 오브젝트가 없습니다. " +
                           "Player 프리팹을 Hierarchy에 넣었는지 확인하세요.");
            return;
        }

        if (movers.Length > 1)
            Debug.LogWarning($"[Fixer] PlayerMover가 {movers.Length}개입니다. 전부 고칩니다.");

        foreach (PlayerMover mover in movers)
        {
            GameObject go = mover.gameObject;
            Undo.RegisterFullObjectHierarchyUndo(go, "Fix Player Setup");
            FixGameObject(go, "씬");
            EditorUtility.SetDirty(go);
        }

        EditorSceneManager.MarkSceneDirty(movers[0].gameObject.scene);
        Debug.Log("[Fixer] 씬 수정 완료. ★ Ctrl+S 로 씬을 저장하세요 ★");
        Selection.activeGameObject = movers[0].gameObject;
    }

    // ============================================================
    // [2] Player 프리팹 에셋 고치기 (앞으로 만들 인스턴스를 위해)
    // ============================================================
    [MenuItem("Tools/GoToSchool/[2] Player 프리팹 고치기")]
    public static void FixPlayerPrefab()
    {
        // 이름이 'Player .prefab'처럼 공백이 섞여 있어도 찾도록,
        // PlayerMover를 가진 프리팹을 전부 뒤진다.
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null || asset.GetComponent<PlayerMover>() == null) continue;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            FixGameObject(root, $"프리팹({path})");
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            fixedCount++;
        }

        if (fixedCount == 0)
            Debug.LogError("[Fixer] PlayerMover를 가진 프리팹을 찾지 못했습니다.");
        else
            Debug.Log($"[Fixer] 프리팹 {fixedCount}개 수정 완료.");

        AssetDatabase.SaveAssets();
    }

    // ============================================================
    // [3] 고치지 않고 현재 상태만 출력
    // ============================================================
    [MenuItem("Tools/GoToSchool/[3] 씬 상태 진단만 하기")]
    public static void Diagnose()
    {
        PlayerMover[] movers = Object.FindObjectsOfType<PlayerMover>(true);
        Debug.Log($"[진단] 씬의 PlayerMover 개수: {movers.Length}");

        foreach (PlayerMover m in movers)
        {
            GameObject go = m.gameObject;
            string kind = PrefabUtility.IsPartOfPrefabInstance(go)
                ? "프리팹 인스턴스 O"
                : "★프리팹 인스턴스 아님(그냥 만든 오브젝트)★";

            Debug.Log(
                $"[진단] '{go.name}'  ({kind})\n" +
                $"  활성화             : {go.activeInHierarchy}\n" +
                $"  Animator           : {(go.GetComponent<Animator>() != null ? "O" : "✗ 없음")}\n" +
                $"  Controller 할당    : {(go.GetComponent<Animator>() != null && go.GetComponent<Animator>().runtimeAnimatorController != null ? go.GetComponent<Animator>().runtimeAnimatorController.name : "✗ 없음")}\n" +
                $"  레거시 Animation   : {(go.GetComponent<Animation>() != null ? "★있음(제거 필요)★" : "없음 O")}\n" +
                $"  PlayerAnimationCtrl: {(go.GetComponent<PlayerAnimationController>() != null ? "O" : "✗ 없음")}\n" +
                $"  SpriteRenderer     : {(go.GetComponent<SpriteRenderer>() != null ? "O" : "✗ 없음")}\n" +
                $"  Sprite 할당        : {(go.GetComponent<SpriteRenderer>() != null && go.GetComponent<SpriteRenderer>().sprite != null ? go.GetComponent<SpriteRenderer>().sprite.name : "✗ 비어있음")}",
                go);
        }

        AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        Debug.Log($"[진단] {ControllerPath} : {(ctrl != null ? "존재 O" : "★없음 — 먼저 애니메이션 생성 메뉴를 실행하세요★")}");
    }

    // ============================================================
    // 실제 교정 로직 (씬/프리팹 공용)
    // ============================================================
    private static void FixGameObject(GameObject go, string where)
    {
        Debug.Log($"[Fixer] {where}: '{go.name}' 교정 시작", go);

        // ---- 1. 레거시 Animation 컴포넌트 제거 ----
        Animation legacy = go.GetComponent<Animation>();
        if (legacy != null)
        {
            Object.DestroyImmediate(legacy, true);
            Debug.Log("   - 레거시 Animation 컴포넌트 제거함");
        }

        // ---- 2. Animator 확보 + Controller 할당 ----
        Animator animator = go.GetComponent<Animator>();
        if (animator == null)
        {
            animator = go.AddComponent<Animator>();
            Debug.Log("   - Animator 추가함");
        }

        AnimatorController controller =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        if (controller == null)
        {
            Debug.LogError($"   - ★{ControllerPath} 가 없습니다★ " +
                           "Tools → GoToSchool → Player 애니메이션 생성 을 먼저 실행하세요.");
        }
        else if (animator.runtimeAnimatorController != controller)
        {
            animator.runtimeAnimatorController = controller;
            Debug.Log("   - Animator에 Player.controller 할당함");
        }

        if (animator.applyRootMotion)
        {
            animator.applyRootMotion = false;
            Debug.Log("   - Apply Root Motion 해제함 (PlayerMover와 충돌 방지)");
        }

        // ---- 3. PlayerAnimationController 확보 ----
        if (go.GetComponent<PlayerAnimationController>() == null)
        {
            go.AddComponent<PlayerAnimationController>();
            Debug.Log("   - PlayerAnimationController 추가함  ← 이게 빠져 있었습니다");
        }

        // ---- 4. SpriteRenderer 스프라이트 ----
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = go.AddComponent<SpriteRenderer>();
            Debug.Log("   - SpriteRenderer 추가함");
        }
        if (sr.sprite == null)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(IdleSpritePath);
            if (s != null)
            {
                sr.sprite = s;
                Debug.Log($"   - Sprite 할당함 ({s.name})");
            }
            else
            {
                Debug.LogWarning($"   - {IdleSpritePath} 를 찾지 못해 Sprite를 비워둡니다.");
            }
        }

        Debug.Log($"[Fixer] {where}: '{go.name}' 교정 완료", go);
    }
}