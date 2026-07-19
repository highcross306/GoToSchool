// ============================================================
// AutoBootstrapper.cs
// 역할: 어떤 씬에서 Play를 눌러도 GameManager/SoundManager 등
//       필수 매니저와 PauseMenu(ESC 일시정지 창)가 자동으로 존재하도록 보장.
//       Bootstrap 씬을 거치지 않고 Stage1/Stage2를 직접 실행해도
//       개발 중 테스트가 정상 동작하게 하기 위함.
// 배치: 아무 폴더나 상관없음 (MonoBehaviour 아님, 씬에 붙일 필요 없음)
// 준비물: Assets/Resources/Bootstrap/Managers.prefab
//         (GameManager, SoundManager, MusicManager, TapSpeedController를 담은 프리팹)
//         Assets/Resources/Bootstrap/PauseMenu.prefab
//         (PauseMenu 컴포넌트 + 자체 Canvas를 담은 UI 프리팹)
// ============================================================

using UnityEngine;

public static class AutoBootstrapper
{
    private const string ManagersPrefabPath = "Bootstrap/Managers";
    private const string PauseMenuPrefabPath = "Bootstrap/PauseMenu";

    // 씬이 로드되기 전, 가장 먼저 실행됨
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureManagers()
    {
        // 이미 매니저가 존재하면 (Bootstrap을 거쳐왔다면) 아무것도 안 함
        if (GameManager.Instance == null)
        {
            GameObject managersPrefab = Resources.Load<GameObject>(ManagersPrefabPath);
            if (managersPrefab == null)
            {
                Debug.LogError(
                    $"[AutoBootstrapper] 매니저 프리팹을 찾을 수 없습니다. " +
                    $"Assets/Resources/{ManagersPrefabPath}.prefab 경로를 확인하세요.");
            }
            else
            {
                Object.Instantiate(managersPrefab);
                Debug.Log("[AutoBootstrapper] 필수 매니저 자동 생성 완료 (Bootstrap 씬 없이 실행됨)");
            }
        }

        // PauseMenu도 마찬가지로, 어느 씬에서 시작하든 존재해야
        // Stage 씬을 직접 실행했을 때도 ESC 일시정지가 동작한다.
        if (PauseMenu.Instance == null)
        {
            GameObject pauseMenuPrefab = Resources.Load<GameObject>(PauseMenuPrefabPath);
            if (pauseMenuPrefab == null)
            {
                Debug.LogError(
                    $"[AutoBootstrapper] PauseMenu 프리팹을 찾을 수 없습니다. " +
                    $"Assets/Resources/{PauseMenuPrefabPath}.prefab 경로를 확인하세요.");
            }
            else
            {
                Object.Instantiate(pauseMenuPrefab);
                Debug.Log("[AutoBootstrapper] PauseMenu 자동 생성 완료 (Bootstrap 씬 없이 실행됨)");
            }
        }
    }
}