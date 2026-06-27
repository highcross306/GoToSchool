// ============================================================
// StageData.cs
// 역할: 스테이지 설정 ScriptableObject
//       routes 배열 제거 — NodeData.outgoingRoutes로 관리
// 생성: Assets/ScriptableObjects/Stages 폴더 우클릭
//       → Create → Game → Stage Data
// ============================================================

using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("스테이지 기본 설정")]
    public int stageIndex;
    public int timeLimitSeconds;
    public int initialBudget;

    [Header("노드 목록 (씬 생성용)")]
    public NodeData[] allNodes;

    [Header("이벤트 목록")]
    public GameEvent[] events; // 이 스테이지에서 발생 가능한 이벤트들

    // 전체 경로 수 계산 (IsSelectionComplete에서 사용)
    public int GetTotalRouteCount()
    {
        int count = 0;
        foreach (NodeData node in allNodes)
            count += node.outgoingRoutes.Length;
        return count;
    }
}