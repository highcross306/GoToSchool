// ============================================================
// PlayerAnimationSetup.cs
// 역할: Assets/Sprites/Player 의 스프라이트로 AnimationClip 4개와
//       Player.controller(Animator Controller)를 자동 생성한다.
// 위치: Assets/Editor/PlayerAnimationSetup.cs  (반드시 Editor 폴더 안!)
// 사용: 상단 메뉴 → Tools → GoToSchool → Player 애니메이션 생성
// 주의: 에디터 전용 스크립트라 빌드에는 포함되지 않는다.
// ============================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class PlayerAnimationSetup
{
    // ---- 경로 설정 ----
    private const string SpriteFolder = "Assets/Sprites/Player";
    private const string IdleSpritePath = "Assets/Sprites/character.png"; // 정지 상태 스프라이트
    private const string OutputFolder = "Assets/Animations";
    private const string ControllerPath = OutputFolder + "/Player.controller";

    // ---- 파라미터 이름 (PlayerAnimationController.cs와 반드시 동일) ----
    private const string ParamIsMoving = "IsMoving";
    private const string ParamTransport = "TransportType";

    [MenuItem("Tools/GoToSchool/Player 애니메이션 생성")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder("Assets", "Animations");

        // ---------- 1. 클립 생성 ----------
        // Walk: 1,3 → 2 → 1,3 → 4 (4프레임, 10fps)
        AnimationClip walk = CreateClip("Walk_Move", 10f, true, new[]
        {
            "character_walk_1,3",
            "character_walk_2",
            "character_walk_1,3",
            "character_walk_4"
        });

        // Bus: 1 → 2 (2프레임, 6fps — 차량 흔들림은 느린 게 자연스럽다)
        AnimationClip bus = CreateClip("Bus_Move", 6f, true, new[]
        {
            "character_bus_1",
            "character_bus_2"
        });

        // Taxi: 1 → 2 (2프레임, 6fps)
        AnimationClip taxi = CreateClip("Taxi_Move", 6f, true, new[]
        {
            "character_taxi_1",
            "character_taxi_2"
        });

        // Idle: 1프레임 (정지 상태)
        AnimationClip idle = CreateIdleClip("Idle");

        if (walk == null || bus == null || taxi == null || idle == null)
        {
            Debug.LogError("[Setup] 클립 생성 실패. 스프라이트 이름/경로를 확인하세요.");
            return;
        }

        // ---------- 2. 컨트롤러 생성 ----------
        AssetDatabase.DeleteAsset(ControllerPath); // 기존 것이 있으면 새로 만든다
        AnimatorController controller =
            AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        controller.AddParameter(ParamIsMoving, AnimatorControllerParameterType.Bool);
        controller.AddParameter(ParamTransport, AnimatorControllerParameterType.Int);

        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        AnimatorState idleState = sm.AddState("Idle", new Vector3(300, 0, 0));
        idleState.motion = idle;
        sm.defaultState = idleState;

        AnimatorState walkState = sm.AddState("Walk_Move", new Vector3(600, -80, 0));
        walkState.motion = walk;

        AnimatorState busState = sm.AddState("Bus_Move", new Vector3(600, 20, 0));
        busState.motion = bus;

        AnimatorState taxiState = sm.AddState("Taxi_Move", new Vector3(600, 120, 0));
        taxiState.motion = taxi;

        // ---------- 3. 트랜지션 ----------
        // Any State → 각 이동 스테이트 (IsMoving == true && TransportType == n)
        // canTransitionToSelf = false 로 두어야 같은 스테이트를 매 프레임 재진입하지 않는다.
        AddAnyTransition(sm, walkState, 0);
        AddAnyTransition(sm, busState, 1);
        AddAnyTransition(sm, taxiState, 2);

        // 각 이동 스테이트 → Idle (IsMoving == false)
        AddToIdle(walkState, idleState);
        AddToIdle(busState, idleState);
        AddToIdle(taxiState, idleState);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Setup] 완료 → " + ControllerPath +
                  "\n다음: Player 프리팹에 Animator 추가 후 이 컨트롤러를 연결하세요.");
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
    }

    // ------------------------------------------------------------
    // 스프라이트 이름 배열로 루프 클립을 만든다.
    // 마지막에 더미 키를 하나 더 찍어야 클립 길이가 프레임수/fps 로 정확히 맞는다.
    // ------------------------------------------------------------
    private static AnimationClip CreateClip(string name, float fps, bool loop, string[] spriteNames)
    {
        List<ObjectReferenceKeyframe> keys = new List<ObjectReferenceKeyframe>();

        for (int i = 0; i < spriteNames.Length; i++)
        {
            Sprite s = LoadSprite(spriteNames[i]);
            if (s == null) return null;

            keys.Add(new ObjectReferenceKeyframe { time = i / fps, value = s });
        }

        // 마지막 프레임 지속시간 확보용 더미 키
        Sprite last = LoadSprite(spriteNames[spriteNames.Length - 1]);
        keys.Add(new ObjectReferenceKeyframe { time = spriteNames.Length / fps, value = last });

        return WriteClip(name, fps, loop, keys.ToArray());
    }

    private static AnimationClip CreateIdleClip(string name)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(IdleSpritePath);
        if (s == null)
        {
            // character.png가 없으면 걷기 1프레임을 정지 포즈로 대체
            s = LoadSprite("character_walk_1,3");
            if (s == null) return null;
            Debug.LogWarning("[Setup] " + IdleSpritePath + " 없음 → walk_1,3 을 Idle로 사용");
        }

        ObjectReferenceKeyframe[] keys =
        {
            new ObjectReferenceKeyframe { time = 0f, value = s }
        };
        return WriteClip(name, 12f, false, keys);
    }

    private static AnimationClip WriteClip(string name, float fps, bool loop,
                                           ObjectReferenceKeyframe[] keys)
    {
        string path = OutputFolder + "/" + name + ".anim";
        AssetDatabase.DeleteAsset(path);

        AnimationClip clip = new AnimationClip { frameRate = fps };

        EditorCurveBinding binding =
            EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, path);
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    }

    private static Sprite LoadSprite(string fileName)
    {
        string path = SpriteFolder + "/" + fileName + ".png";
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null)
            Debug.LogError("[Setup] 스프라이트를 찾을 수 없음: " + path +
                           "\n(Texture Type이 Sprite(2D and UI)인지 확인)");
        return s;
    }

    private static void AddAnyTransition(AnimatorStateMachine sm, AnimatorState target, int transportValue)
    {
        AnimatorStateTransition t = sm.AddAnyStateTransition(target);
        t.hasExitTime = false;
        t.duration = 0f;
        t.canTransitionToSelf = false;
        t.AddCondition(AnimatorConditionMode.If, 0f, ParamIsMoving);
        t.AddCondition(AnimatorConditionMode.Equals, transportValue, ParamTransport);
    }

    private static void AddToIdle(AnimatorState from, AnimatorState idle)
    {
        AnimatorStateTransition t = from.AddTransition(idle);
        t.hasExitTime = false;
        t.duration = 0f;
        t.AddCondition(AnimatorConditionMode.IfNot, 0f, ParamIsMoving);
    }
}