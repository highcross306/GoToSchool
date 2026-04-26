// ============================================================
// PlanningManager.cs
// 역할: 선택 단계 전체 흐름 관리
//       노드 클릭 → 연결 경로 자동 탐색 → 이동수단 선택
// ============================================================

using System.Collections.Generic;
using UnityEngine;

// 선택 하나를 나타내는 데이터 (경로 + 이동수단)
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

    // 확정된 선택 목록 (ExecutionManager에 전달)
    public List<SelectionEntry> Selections { get; private set; } = new();

    // 이동수단 선택 대기 중인 경로
    private RouteData pendingRoute;

    // 현재 플레이어 위치 노드 ID
    private string currentNodeId;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // StageManager가 스테이지 빌드 후 호출
    // Start 노드 ID를 받아 초기 위치 설정
    public void Initialize(string startNodeId)
    {
        Selections.Clear();
        pendingRoute = null;
        currentNodeId = startNodeId;
        Debug.Log($"[Planning] 선택 단계 시작. 현재 위치: {currentNodeId}");
    }

    // Node.cs → OnClicked() → 여기로 전달
    public void OnNodeClicked(Node node)
    {
        string targetNodeId = node.Data.id;

        // Start 노드 클릭 시 무반응
        if (node.Data.nodeType == NodeType.Start)
        {
            Debug.Log("[Planning] 시작 노드는 선택할 수 없습니다.");
            return;
        }

        // 현재 위치와 동일한 노드 클릭 시 무반응
        if (targetNodeId == currentNodeId)
        {
            Debug.Log("[Planning] 현재 위치와 동일한 노드입니다.");
            return;
        }

        // 현재 위치에서 클릭한 노드로 가는 경로 탐색
        RouteData connectedRoute = SelectionValidator.Instance.FindConnectedRoute(
            currentNodeId,
            targetNodeId,
            StageManager.Instance.CurrentStageData
        );

        // 연결되지 않은 노드 클릭 시 무반응
        if (connectedRoute == null)
        {
            Debug.Log($"[Planning] {currentNodeId} → {targetNodeId} 는 연결된 경로가 없습니다.");
            return;
        }

        // 연결 확인 → 이동수단 선택 대기
        pendingRoute = connectedRoute;
        Debug.Log($"[Planning] 경로 확정: {connectedRoute.id} / 이동수단을 선택하세요.");

        // 추후 UI 연결 시 SelectionCardUI 표시 코드 추가
    }

    // SelectionCardUI에서 이동수단 선택 시 호출
    public void OnTransportSelected(TransportType transport)
    {
        if (pendingRoute == null)
        {
            Debug.LogWarning("[Planning] 선택된 경로가 없습니다. 노드를 먼저 클릭하세요.");
            return;
        }

        // 현재 노드에서 해당 이동수단이 허용되는지 검증
        NodeData currentNodeData = GetNodeDataById(currentNodeId);
        if (!SelectionValidator.Instance.IsTransportValid(currentNodeData, transport)) return;

        // 선택 확정
        Selections.Add(new SelectionEntry(pendingRoute, transport));
        currentNodeId = pendingRoute.toNodeId;
        pendingRoute = null;

        Debug.Log($"[Planning] 선택 확정 ({Selections.Count}번째): {transport} / 현재 위치: {currentNodeId}");

        // 모든 경로 선택 완료 시 결정 버튼 활성화
        if (SelectionValidator.Instance.IsSelectionComplete(
                Selections, StageManager.Instance.CurrentStageData))
        {
            Debug.Log("[Planning] 선택 완료! 결정 버튼 활성화");
            // 추후 UI 연결 시 PlanningUI.SetDecideButtonActive(true) 호출
        }
    }

    // 결정 버튼 클릭 시 호출
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

    // ID로 NodeData 검색
    private NodeData GetNodeDataById(string id)
    {
        foreach (NodeData node in StageManager.Instance.CurrentStageData.nodes)
        {
            if (node.id == id) return node;
        }
        return null;
    }
}