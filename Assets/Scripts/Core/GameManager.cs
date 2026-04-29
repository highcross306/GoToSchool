// ============================================================
// GameManager.cs
// 역할: 게임 전체의 진입점, 단계 전환 총괄 싱글톤
//       씬에 [GameManager] 오브젝트에 부착
// ============================================================

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("스테이지 데이터")]
    public StageData[] stageDatas; // 인스펙터에서 StageData_1~4 연결

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadStage(GameState.CurrentStage);
    }

    // 스테이지 로드
    public void LoadStage(int stageIndex)
    {
        GameState.CurrentStage = stageIndex;
        GameState.CurrentPhase = Phase.Planning;

        StageData data = stageDatas[stageIndex - 1];
        StageManager.Instance.BuildStage(data);
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