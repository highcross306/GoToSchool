// ============================================================
// PlanningManager.cs
// 역할: 선택 단계 흐름 관리
//       결정 버튼 클릭 시 즉시 해당 경로 이동 트리거
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
    public bool HasPendingRoute => pendingRoute != null;

    private RouteData pendingRoute;
    private NodeData currentNode;
    private HashSet<NodeData> decidedNodes = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Initialize(NodeData startNode)
    {
        if (startNode == null)
        {
            Debug.LogError("[PlanningManager] startNode가 null입니다.");
            return;
        }
        Selections.Clear();
        decidedNodes.Clear();
        pendingRoute = null;
        currentNode = startNode;
        decidedNodes.Add(startNode);
        MessageSystem.L($"선택 단계 시작. 현재 위치: {startNode.name}");
    }

    public void OnNodeClicked(Node node)
    {
        NodeData targetNode = node.Data;

        if (targetNode.nodeType == NodeType.Start) return;

        if (decidedNodes.Contains(targetNode))
        {
            MessageSystem.E("결정을 마친 노드는 선택할 수 없습니다!");
            return;
        }

        RouteData connectedRoute = SelectionValidator.Instance.FindConnectedRoute(
            currentNode, targetNode);

        if (connectedRoute == null)
        {
            MessageSystem.L($"{currentNode.name} → {targetNode.name} 연결 경로 없음.");
            return;
        }

        pendingRoute = connectedRoute;
        PlanningUI.Instance.ShowSelectionCards(connectedRoute);
        MessageSystem.L($"경로 확정: {connectedRoute.name}");
    }

    public void OnTransportSelected(TransportType transport)
    {
        if (pendingRoute == null) return;
        if (!SelectionValidator.Instance.IsTransportValid(pendingRoute, transport)) return;

        // 출발 노드 결정 완료 처리
        decidedNodes.Add(pendingRoute.fromNode);
        StageManager.Instance.GetNode(pendingRoute.fromNode)?.SetDecided();

        SelectionEntry entry = new SelectionEntry(pendingRoute, transport);
        Selections.Add(entry);
        currentNode = pendingRoute.toNode;
        pendingRoute = null;

        MessageSystem.L($"이동수단 확정: {transport} / 목적지: {currentNode.name}");
    }

    // 결정 버튼 클릭 → 즉시 해당 경로 이동 시작
    public void OnDecideButtonClicked()
    {
        if (Selections.Count == 0) return;

        // 가장 최근 확정된 경로를 즉시 실행
        SelectionEntry latest = Selections[Selections.Count - 1];
        GameManager.Instance.StartSingleRouteExecution(latest);
    }
}