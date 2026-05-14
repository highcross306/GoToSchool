// ============================================================
// PlanningManager.cs
// 역할: 선택 단계 전체 흐름 관리
//       문자열 ID 대신 NodeData 객체로 직접 관리
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
    private NodeData currentNode;
    private HashSet<NodeData> decidedNodes = new(); // 객체 직접 비교

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // StageManager가 스테이지 빌드 후 호출
    public void Initialize(NodeData startNode)
    {
        Selections.Clear();
        decidedNodes.Clear();
        pendingRoute = null;
        currentNode = startNode;

        // 시작 노드는 처음부터 결정 완료 처리
        decidedNodes.Add(startNode);
        Debug.Log($"[Planning] 선택 단계 시작. 현재 위치: {startNode.name}");
    }

    public void OnNodeClicked(Node node)
    {
        NodeData targetNode = node.Data;

        // 시작 노드 클릭 무반응
        if (targetNode.nodeType == NodeType.Start) return;

        // 이미 결정된 노드 클릭 무반응
        if (decidedNodes.Contains(targetNode))
        {
            Debug.Log($"[Planning] {targetNode.name}는 이미 결정된 노드입니다.");
            return;
        }

        // 현재 노드의 outgoingRoutes에서 직접 탐색
        RouteData connectedRoute = SelectionValidator.Instance.FindConnectedRoute(
            currentNode, targetNode);

        if (connectedRoute == null)
        {
            Debug.Log($"[Planning] {currentNode.name} → {targetNode.name} 연결 경로 없음.");
            return;
        }

        pendingRoute = connectedRoute;
        PlanningUI.Instance.ShowSelectionCards(connectedRoute);
        Debug.Log($"[Planning] 경로 확정: {connectedRoute.name}");
    }

    public void OnTransportSelected(TransportType transport)
    {
        if (pendingRoute == null) return;
        if (!SelectionValidator.Instance.IsTransportValid(pendingRoute, transport)) return;

        // 출발 노드 결정 완료 처리
        decidedNodes.Add(pendingRoute.fromNode);

        // StageManager의 nodeMap으로 Node 컴포넌트 조회 후 비활성화
        StageManager.Instance.GetNode(pendingRoute.fromNode)?.SetDecided();

        Selections.Add(new SelectionEntry(pendingRoute, transport));
        currentNode = pendingRoute.toNode;
        pendingRoute = null;

        Debug.Log($"[Planning] 선택 확정 ({Selections.Count}번째): {transport} / 현재 위치: {currentNode.name}");

        if (SelectionValidator.Instance.IsSelectionComplete(
                Selections, StageManager.Instance.CurrentStageData))
        {
            Debug.Log("[Planning] 전체 경로 선택 완료! 결정 버튼 활성화");
            PlanningUI.Instance.SetDecideButtonActive(true);
        }
    }

    public void OnDecideButtonClicked()
    {
        if (!SelectionValidator.Instance.IsSelectionComplete(
                Selections, StageManager.Instance.CurrentStageData)) return;
        GameManager.Instance.StartExecution();
    }
}