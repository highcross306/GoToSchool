// ============================================================
// AutoBootstrapper.cs
// 역할: 어떤 씬에서 Play를 눌러도 GameManager/SoundManager 등
//       필수 매니저가 자동으로 존재하도록 보장.
//       Bootstrap 씬을 거치지 않고 Stage1/Stage2를 직접 실행해도
//       개발 중 테스트가 정상 동작하게 하기 위함.
// 배치: 아무 폴더나 상관없음 (MonoBehaviour 아님, 씬에 붙일 필요 없음)
// 준비물: Assets/Resources/Bootstrap/Managers.prefab
//         (GameManager, SoundManager, TapSpeedController를 담은 프리팹)
// ============================================================

using UnityEngine;

public static class AutoBootstrapper
{
    private const string ManagersPrefabPath = "Bootstrap/Managers";

    // 씬이 로드되기 전, 가장 먼저 실행됨
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureManagers()
    {
        // 이미 매니저가 존재하면 (Bootstrap을 거쳐왔다면) 아무것도 안 함
        if (GameManager.Instance != null) return;

        GameObject prefab = Resources.Load<GameObject>(ManagersPrefabPath);
        if (prefab == null)
        {
            Debug.LogError(
                $"[AutoBootstrapper] 매니저 프리팹을 찾을 수 없습니다. " +
                $"Assets/Resources/{ManagersPrefabPath}.prefab 경로를 확인하세요.");
            return;
        }

        Object.Instantiate(prefab);
        Debug.Log("[AutoBootstrapper] 필수 매니저 자동 생성 완료 (Bootstrap 씬 없이 실행됨)");
    }
}