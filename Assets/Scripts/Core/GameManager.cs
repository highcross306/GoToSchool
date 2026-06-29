// ============================================================
// GameManager.cs
// 역할: 게임 전체 흐름 총괄, 씬 전환 관리
//       DontDestroyOnLoad로 씬이 바뀌어도 유지됨
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("스테이지 씬 이름 (Build Settings 순서와 일치)")]
    public string[] stageSceneNames; // 예: ["Stage1", "Stage2", "Stage3", "Stage4"]

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 씬 전환으로 스테이지 로드
    public void LoadStage(int stageIndex)
    {
        if (stageIndex < 1 || stageIndex > stageSceneNames.Length)
        {
            Debug.LogError($"[GameManager] 존재하지 않는 스테이지: {stageIndex}");
            return;
        }

        GameState.CurrentStage = stageIndex;
        GameState.CurrentPhase = Phase.Planning;

        string sceneName = stageSceneNames[stageIndex - 1];
        Debug.Log($"[GameManager] 씬 전환: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    // 선택 완료 → 실행 단계로 전환
    public void StartExecution()
    {
        GameState.CurrentPhase = Phase.Execution;
        ExecutionManager.Instance.StartExecution();
    }

    // 실행 완료 → 결과 단계로 전환
    public void ShowResult()
    {
        GameState.CurrentPhase = Phase.Result;
        ResultEvaluator.Instance.Evaluate();
    }
}