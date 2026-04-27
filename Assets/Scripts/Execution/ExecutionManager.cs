// ============================================================
// ExecutionManager.cs
// 역할: 실행 단계 전체 관리
//       PlanningManager의 선택 목록을 순서대로 소비
//       PlayerMover에게 이동 명령 → PlayerBudget 차감
//       모든 경로 완료 시 GameManager에 결과 신호
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class ExecutionManager : MonoBehaviour
{
    public static ExecutionManager Instance { get; private set; }

    private List<SelectionEntry> selections;
    private int currentIndex = 0;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // GameManager.StartExecution() → 여기서 시작
    public void StartExecution()
    {
        selections = PlanningManager.Instance.Selections;
        currentIndex = 0;

        Debug.Log($"[Execution] 실행 단계 시작 — 총 {selections.Count}개 경로");
        ExecuteNext();
    }

    // 다음 경로 실행
    private void ExecuteNext()
    {
        if (currentIndex >= selections.Count)
        {
            // 모든 경로 완료
            Debug.Log("[Execution] 모든 경로 이동 완료");
            GameManager.Instance.ShowResult();
            return;
        }

        SelectionEntry entry = selections[currentIndex];
        Debug.Log($"[Execution] {currentIndex + 1}번째 이동: " +
                  $"{entry.route.fromNodeId} → {entry.route.toNodeId} ({entry.transport})");

        // 목표 노드 위치 탐색
        GameObject targetObj = GameObject.Find(entry.route.toNodeId);
        if (targetObj == null)
        {
            Debug.LogError($"[Execution] 노드를 찾을 수 없습니다: {entry.route.toNodeId}");
            return;
        }

        // PlayerMover에게 이동 명령
        PlayerMover.Instance.MoveTo(targetObj.transform.position, OnMoveComplete);
    }

    // PlayerMover 이동 완료 콜백
    private void OnMoveComplete()
    {
        SelectionEntry entry = selections[currentIndex];

        // 해당 이동수단의 비용/시간 찾기
        TransportCost cost = FindTransportCost(entry.route, entry.transport);
        if (cost != null)
            PlayerBudget.Instance.Consume(cost.cost, cost.timeMinutes);

        currentIndex++;
        ExecuteNext();
    }

    // RouteData에서 선택한 이동수단의 비용 데이터 반환
    private TransportCost FindTransportCost(RouteData route, TransportType transport)
    {
        foreach (TransportCost cost in route.transportCosts)
        {
            if (cost.transportType == transport) return cost;
        }
        Debug.LogWarning($"[Execution] {route.id}에서 {transport} 비용 데이터를 찾을 수 없습니다.");
        return null;
    }
}