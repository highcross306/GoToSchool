// ============================================================
// GameManager.cs
// 역할: 게임 전체 흐름 총괄, 씬 전환 관리
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("스테이지 씬 이름")]
    public string[] stageSceneNames;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 씬 전환으로 스테이지 로드
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

    // 단일 경로 실행 (결정 버튼 클릭 시 즉시 이동)
    public void StartSingleRouteExecution(SelectionEntry entry)
    {
        GameState.CurrentPhase = Phase.Execution;
        ExecutionManager.Instance.ExecuteSingle(entry);
    }

    // 결과 판정 호출
    public void ShowResult()
    {
        GameState.CurrentPhase = Phase.Result;
        ResultEvaluator.Instance.Evaluate();
    }
}