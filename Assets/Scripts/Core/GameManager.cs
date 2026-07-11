// ============================================================
// GameManager.cs
// 역할: 게임 전체 흐름 총괄, 씬 전환 관리
//       Managers 프리팹의 자식이어도 DontDestroyOnLoad가 제대로
//       동작하도록 Awake에서 루트로 분리 후 유지
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("씬 이름")]
    public string mainMenuSceneName = "MainMenu";
    public string bootstrapSceneName = "Bootstrap";
    public string[] stageSceneNames;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // Managers 프리팹의 자식으로 있으면 DontDestroyOnLoad가 무시되므로
        // 루트로 분리한 뒤 유지시킴
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        HandleInitialScene();
    }

    private void HandleInitialScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == bootstrapSceneName || string.IsNullOrEmpty(currentScene))
        {
            LoadStage(1);
            return;
        }

        for (int i = 0; i < stageSceneNames.Length; i++)
        {
            if (stageSceneNames[i] == currentScene)
            {
                GameState.CurrentStage = i + 1;
                GameState.CurrentPhase = Phase.Planning;
                Debug.Log($"[GameManager] '{currentScene}' 씬 직접 실행 감지 → " +
                          $"Stage {GameState.CurrentStage}로 인식, 씬 재로드 없이 진행");
                return;
            }
        }

        Debug.Log($"[GameManager] '{currentScene}' 씬에서 시작 — 스테이지 목록에 없어 상태 동기화 생략");
    }

    public void LoadStage(int stageIndex)
    {
        Debug.Log($"[GameManager] LoadStage 호출 — 스테이지: {stageIndex} / 씬 수: {stageSceneNames.Length}");

        if (stageIndex < 1 || stageIndex > stageSceneNames.Length)
        {
            Debug.LogError($"[GameManager] 존재하지 않는 스테이지: {stageIndex}");
            return;
        }

        string sceneName = stageSceneNames[stageIndex - 1];
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"[GameManager] stageSceneNames[{stageIndex - 1}]이 비어있습니다.");
            return;
        }

        GameState.CurrentStage = stageIndex;
        GameState.CurrentPhase = Phase.Planning;
        SceneManager.LoadScene(sceneName);
    }

    public void StartSingleRouteExecution(SelectionEntry entry)
    {
        GameState.CurrentPhase = Phase.Execution;
        ExecutionManager.Instance.ExecuteSingle(entry);
    }

    public void LoadMainMenu()
    {
        GameState.CurrentPhase = Phase.Planning;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ShowResult()
    {
        GameState.CurrentPhase = Phase.Result;
        ResultEvaluator.Instance.Evaluate();
    }
}