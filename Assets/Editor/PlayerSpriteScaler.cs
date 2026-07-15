// ============================================================
// PlayerSpriteScaler.cs
// 역할: 이동 애니메이션 스프라이트(Assets/Sprites/Player)의
//       Pixels Per Unit을 한꺼번에 조정해 크기를 바꾼다.
//
//       ★ Idle 스프라이트(Assets/Sprites/character.png)는 건드리지 않는다.
//
//       Player 폴더 안에서 픽셀 크기가 서로 다른 스프라이트가 있어도
//       최종 월드 크기가 동일해지도록 PPU를 개별 계산한다.
//       → 프레임마다 캐릭터가 커졌다 작아졌다 하는 문제가 없다.
//
// 위치: Assets/Editor/PlayerSpriteScaler.cs   (반드시 Editor 폴더!)
// 사용: Tools → GoToSchool → 크기 → 1.5배  (등)
// 주의: Player 프리팹의 Transform Scale은 1로 두세요. 곱해집니다.
// ============================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PlayerSpriteScaler
{
    private const string SpriteFolder = "Assets/Sprites/Player";
    private const string IdleSpritePath = "Assets/Sprites/character.png";

    // 기준: 192px 스프라이트가 PPU 100일 때의 월드 높이 = 1.92 유닛
    private const float BaseUnits = 1.92f;

    // Idle 스프라이트의 원본 PPU (되돌리기용)
    private const float IdleOriginalPPU = 100f;

    // ---- 배율 메뉴 (Idle 제외, 이동 프레임만) ----
    [MenuItem("Tools/GoToSchool/크기/1배 (원래대로)")] private static void S10() => Apply(BaseUnits * 1.00f);
    [MenuItem("Tools/GoToSchool/크기/1.25배")] private static void S12() => Apply(BaseUnits * 1.25f);
    [MenuItem("Tools/GoToSchool/크기/1.5배")] private static void S15() => Apply(BaseUnits * 1.50f);
    [MenuItem("Tools/GoToSchool/크기/1.75배")] private static void S17() => Apply(BaseUnits * 1.75f);
    [MenuItem("Tools/GoToSchool/크기/2배")] private static void S20() => Apply(BaseUnits * 2.00f);
    [MenuItem("Tools/GoToSchool/크기/2.5배")] private static void S25() => Apply(BaseUnits * 2.50f);

    // ---- Idle과 정확히 같은 크기로 맞추기 (튐 없음) ----
    [MenuItem("Tools/GoToSchool/크기/Idle과 크기 맞추기")]
    private static void MatchIdle()
    {
        float idleUnits = GetIdleWorldUnits();
        if (idleUnits <= 0f) return;

        Debug.Log($"[Scaler] Idle 크기 = {idleUnits:F2} 유닛 → 이동 프레임을 여기에 맞춥니다.");
        Apply(idleUnits);
    }

    // ---- Idle을 원본 PPU 100으로 되돌리기 ----
    // (이전 버전 스크립트로 Idle까지 바꿔버렸다면 이걸 실행하세요)
    [MenuItem("Tools/GoToSchool/크기/Idle 스프라이트 원래대로 (PPU 100)")]
    private static void RestoreIdle()
    {
        TextureImporter ti = AssetImporter.GetAtPath(IdleSpritePath) as TextureImporter;
        if (ti == null)
        {
            Debug.LogError($"[Scaler] {IdleSpritePath} 를 찾지 못했습니다.");
            return;
        }

        ti.spritePixelsPerUnit = IdleOriginalPPU;
        ti.SaveAndReimport();

        Debug.Log($"[Scaler] Idle 스프라이트 복원 — PPU {IdleOriginalPPU} " +
                  $"({GetIdleWorldUnits():F2} 유닛)");
    }

    // ============================================================
    private static float GetIdleWorldUnits()
    {
        TextureImporter ti = AssetImporter.GetAtPath(IdleSpritePath) as TextureImporter;
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(IdleSpritePath);

        if (ti == null || tex == null)
        {
            Debug.LogError($"[Scaler] {IdleSpritePath} 를 찾지 못했습니다.");
            return -1f;
        }

        return tex.height / ti.spritePixelsPerUnit;
    }

    private static void Apply(float targetUnits)
    {
        List<string> paths = new List<string>();

        // Player 폴더의 스프라이트만. Idle(character.png)은 이 폴더 밖이라 제외된다.
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpriteFolder }))
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));

        if (paths.Count == 0)
        {
            Debug.LogError($"[Scaler] {SpriteFolder} 에서 스프라이트를 찾지 못했습니다.");
            return;
        }

        int changed = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string path in paths)
            {
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null) continue;

                if (ti.textureType != TextureImporterType.Sprite)
                {
                    Debug.LogWarning($"[Scaler] 건너뜀 (Sprite 타입 아님): {path}");
                    continue;
                }

                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) continue;

                // 픽셀 높이가 다르면 PPU도 다르게 → 결과 월드 크기는 동일
                float ppu = tex.height / targetUnits;

                ti.spritePixelsPerUnit = ppu;
                ti.SaveAndReimport();
                changed++;

                Debug.Log($"[Scaler] {System.IO.Path.GetFileName(path)}  " +
                          $"{tex.width}x{tex.height}px  →  PPU {ppu:F2}  →  {targetUnits:F2} 유닛");
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        float idleUnits = GetIdleWorldUnits();
        Debug.Log($"[Scaler] 완료 — 이동 프레임 {changed}개 → {targetUnits:F2} 유닛 " +
                  $"(화면 높이 10유닛의 {targetUnits / 10f * 100f:F0}%)\n" +
                  $"         Idle은 건드리지 않음 → {idleUnits:F2} 유닛");

        if (Mathf.Abs(idleUnits - targetUnits) > 0.01f)
        {
            float pct = (targetUnits / idleUnits - 1f) * 100f;
            Debug.LogWarning(
                $"[Scaler] Idle({idleUnits:F2})과 이동({targetUnits:F2}) 크기가 다릅니다. " +
                $"이동을 시작하면 캐릭터가 {Mathf.Abs(pct):F0}% {(pct > 0 ? "커집니다" : "작아집니다")}.\n" +
                $"         튐을 없애려면 '크기 → Idle과 크기 맞추기'를 실행하세요.");
        }
    }
}