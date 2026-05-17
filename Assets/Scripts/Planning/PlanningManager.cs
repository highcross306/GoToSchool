// ============================================================
// PlanningManager.cs
// 역할: 선택 단계 전체 흐름 관리
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

        // 이미 결정된 노드 클릭
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

        decidedNodes.Add(pendingRoute.fromNode);
        StageManager.Instance.GetNode(pendingRoute.fromNode)?.SetDecided();

        Selections.Add(new SelectionEntry(pendingRoute, transport));
        currentNode = pendingRoute.toNode;
        pendingRoute = null;

        MessageSystem.L($"선택 확정 ({Selections.Count}번째): {transport} / 현재 위치: {currentNode.name}");

        if (SelectionValidator.Instance.IsSelectionComplete(
                Selections, StageManager.Instance.CurrentStageData))
        {
            MessageSystem.L("전체 경로 선택 완료! 결정 버튼 활성화");
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