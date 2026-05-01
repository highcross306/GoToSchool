// ============================================================
// PlanningManager.cs
// 역할: 선택 단계 전체 흐름 관리
//       노드 클릭 → 연결 경로 자동 탐색 → 이동수단 선택
// ============================================================

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SelectionEntry
{
    public RouteData route;
    public TransportType transport;

    public SelectionEntry(RouteData route, TransportType transport)
    {
        this.route = route;
        this.transport = transport;
    }
}

public class PlanningManager : MonoBehaviour
{
    public static PlanningManager Instance { get; private set; }

    public List<SelectionEntry> Selections { get; private set; } = new();

    private RouteData pendingRoute;
    private string currentNodeId;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Initialize(string startNodeId)
    {
        Selections.Clear();
        pendingRoute = null;
        currentNodeId = startNodeId;
        Debug.Log($"[Planning] 선택 단계 시작. 현재 위치: {currentNodeId}");
    }

    public void OnNodeClicked(Node node)
    {
        string targetNodeId = node.Data.id;

        if (node.Data.nodeType == NodeType.Start)
        {
            Debug.Log("[Planning] 시작 노드는 선택할 수 없습니다.");
            return;
        }

        if (targetNodeId == currentNodeId)
        {
            Debug.Log("[Planning] 현재 위치와 동일한 노드입니다.");
            return;
        }

        RouteData connectedRoute = SelectionValidator.Instance.FindConnectedRoute(
            currentNodeId, targetNodeId, StageManager.Instance.CurrentStageData);

        if (connectedRoute == null)
        {
            Debug.Log($"[Planning] {currentNodeId} → {targetNodeId} 연결 경로 없음.");
            return;
        }

        pendingRoute = connectedRoute;
        PlanningUI.Instance.ShowSelectionCards(connectedRoute);
        Debug.Log($"[Planning] 경로 확정: {connectedRoute.id} / 이동수단을 선택하세요.");
    }

    public void OnTransportSelected(TransportType transport)
    {
        if (pendingRoute == null)
        {
            Debug.LogWarning("[Planning] 선택된 경로가 없습니다. 노드를 먼저 클릭하세요.");
            return;
        }

        if (!SelectionValidator.Instance.IsTransportValid(pendingRoute, transport)) return;

        Selections.Add(new SelectionEntry(pendingRoute, transport));
        currentNodeId = pendingRoute.toNodeId;
        pendingRoute = null;

        Debug.Log($"[Planning] 선택 확정 ({Selections.Count}번째): {transport} / 현재 위치: {currentNodeId}");

        if (SelectionValidator.Instance.IsSelectionComplete(
                Selections, StageManager.Instance.CurrentStageData))
        {
            Debug.Log("[Planning] 선택 완료! 결정 버튼 활성화");
            PlanningUI.Instance.SetDecideButtonActive(true);
        }
    }

    public void OnDecideButtonClicked()
    {
        if (!SelectionValidator.Instance.IsSelectionComplete(
                Selections, StageManager.Instance.CurrentStageData))
        {
            Debug.LogWarning("[Planning] 아직 모든 경로를 선택하지 않았습니다.");
            return;
        }
        GameManager.Instance.StartExecution();
    }
}