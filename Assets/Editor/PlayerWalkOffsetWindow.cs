// ============================================================
// PlayerWalkOffsetWindow.cs
// 역할: 걷기 애니메이션 스프라이트(character_walk_*)의 세로 위치를
//       ▲/▼ 버튼으로 실시간 조정하는 에디터 창.
//       Transform.position은 건드리지 않고 Pivot(Y)만 조정 —
//       PlayerMover 이동 판정이나 Animator 로직에는 영향 없음.
//
// 위치: Assets/Editor/PlayerWalkOffsetWindow.cs   (반드시 Editor 폴더!)
// 사용: Tools → GoToSchool → 걷기 애니메이션 위치 조정창 열기
// ============================================================

using UnityEditor;
using UnityEngine;

public class PlayerWalkOffsetWindow : EditorWindow
{
    private const string SpriteFolder = "Assets/Sprites/Player";

    private static readonly string[] WalkSpriteNames =
    {
        "character_walk_1,3",
        "character_walk_2",
        "character_walk_4"
    };

    private float stepPixels = 4f;
    private float currentOffsetPixels = 0f; // 화면 표시용 누적값 (0 = 원위치)

    [MenuItem("Tools/GoToSchool/걷기 애니메이션 위치 조정창 열기")]
    private static void Open()
    {
        PlayerWalkOffsetWindow win = GetWindow<PlayerWalkOffsetWindow>("걷기 위치 조정");
        win.minSize = new Vector2(260, 150);
        win.RefreshCurrentOffset();
    }

    private void OnEnable() => RefreshCurrentOffset();

    private void OnGUI()
    {
        GUILayout.Space(8);
        EditorGUILayout.LabelField("걷기 애니메이션 세로 위치", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"현재 오프셋: {currentOffsetPixels:F1}px " +
                                    (currentOffsetPixels > 0 ? "(아래로 이동됨)" :
                                     currentOffsetPixels < 0 ? "(위로 이동됨)" : "(원위치)"));

        GUILayout.Space(6);
        stepPixels = EditorGUILayout.Slider("클릭당 이동량 (px)", stepPixels, 1f, 20f);

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("▲ 위로", GUILayout.Height(32)))
            Nudge(-stepPixels);

        if (GUILayout.Button("▼ 아래로", GUILayout.Height(32)))
            Nudge(stepPixels);

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(6);
        if (GUILayout.Button("원위치로 리셋"))
            ResetOffset();

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "▲/▼ 버튼을 누른 뒤 Scene 뷰나 Play 모드에서 바로 확인하세요.\n" +
            "Bus/Taxi/Idle 스프라이트는 건드리지 않습니다.",
            MessageType.Info);
    }

    private void Nudge(float pixels)
    {
        foreach (string name in WalkSpriteNames)
            ApplyPivotDelta(name, pixels);

        currentOffsetPixels += pixels;
        AssetDatabase.Refresh();
        Repaint();
    }

    private void ResetOffset()
    {
        foreach (string name in WalkSpriteNames)
            SetPivotYAbsolute(name, 0.5f);

        currentOffsetPixels = 0f;
        AssetDatabase.Refresh();
        Repaint();
        Debug.Log("[WalkOffset] 걷기 스프라이트를 원위치(Pivot 0.5)로 리셋했습니다.");
    }

    // 창을 새로 열었을 때 실제 에셋의 현재 pivot 값을 읽어와 표시값과 동기화
    private void RefreshCurrentOffset()
    {
        string path = SpriteFolder + "/" + WalkSpriteNames[0] + ".png";
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (ti == null || tex == null) { currentOffsetPixels = 0f; return; }

        TextureImporterSettings settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);

        float pivotY = settings.spriteAlignment == (int)SpriteAlignment.Custom
            ? settings.spritePivot.y
            : 0.5f;

        currentOffsetPixels = (pivotY - 0.5f) * tex.height;
    }

    private void ApplyPivotDelta(string spriteName, float deltaPixels)
    {
        string path = SpriteFolder + "/" + spriteName + ".png";
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (ti == null || tex == null)
        {
            Debug.LogError($"[WalkOffset] 스프라이트를 찾지 못했습니다: {path}");
            return;
        }

        TextureImporterSettings settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);

        float currentY = settings.spriteAlignment == (int)SpriteAlignment.Custom
            ? settings.spritePivot.y
            : 0.5f;

        float newY = Mathf.Clamp01(currentY + deltaPixels / tex.height);

        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = new Vector2(0.5f, newY);
        ti.SetTextureSettings(settings);
        ti.SaveAndReimport();
    }

    private void SetPivotYAbsolute(string spriteName, float pivotY)
    {
        string path = SpriteFolder + "/" + spriteName + ".png";
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null)
        {
            Debug.LogError($"[WalkOffset] 스프라이트를 찾지 못했습니다: {path}");
            return;
        }

        TextureImporterSettings settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = new Vector2(0.5f, pivotY);
        ti.SetTextureSettings(settings);
        ti.SaveAndReimport();
    }
}