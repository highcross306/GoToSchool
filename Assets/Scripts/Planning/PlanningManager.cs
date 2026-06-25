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
        if (startNode == null)
        {
            Debug.LogError("[PlanningManager] startNode가 null입니다. StageData에 Start 노드가 있는지 확인하세요.");
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

        decidedNodes.Add(pendingRoute.fromNode);
        StageManager.Instance.GetNode(pendingRoute.fromNode)?.SetDecided();

        Selections.Add(new SelectionEntry(pendingRoute, transport));
        currentNode = pendingRoute.toNode;
        pendingRoute = null;

        MessageSystem.L($"선택 확정 ({Selections.Count}번째): {transport} / 현재 위치: {currentNode.name}");

        if (SelectionValidator.Instance.IsSelectionComplete(
                Selections, StageManager.Instance.CurrentStageData))
        {
            MessageSystem.L("전체 경로 선택 완료!");
            // 주의: 여기서 SetDecideButtonActive(true)를 호출하지 않습니다.
            // 이미 결정 버튼은 OnDecideButtonClicked에서 비활성화된 상태이며,
            // selectedCard도 null이므로 다시 활성화하면 빈 클릭 상태가 됩니다.
        }
    }

    public void OnDecideButtonClicked()
    {
        if (!SelectionValidator.Instance.IsSelectionComplete(
                Selections, StageManager.Instance.CurrentStageData)) return;
        GameManager.Instance.StartExecution();
    }
}