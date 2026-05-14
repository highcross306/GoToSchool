// ============================================================
// PlanningManager.cs
// 역할: 선택 단계 전체 흐름 관리
//       노드 클릭 → 연결 경로 자동 탐색 → 이동수단 선택
//       결정된 노드는 재클릭 불가, 이전 노드로 돌아갈 수 없음
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

    // 결정이 완료된 노드 ID 목록 (재클릭 차단용)
    private HashSet<string> decidedNodeIds = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // StageManager가 스테이지 빌드 후 호출
    public void Initialize(string startNodeId)
    {
        Selections.Clear();
        decidedNodeIds.Clear();
        pendingRoute = null;
        currentNodeId = startNodeId;

        // 시작 노드는 처음부터 결정된 상태로 처리 (재클릭 차단)
        decidedNodeIds.Add(startNodeId);

        Debug.Log($"[Planning] 선택 단계 시작. 현재 위치: {currentNodeId}");
    }

    // Node.cs → OnClicked() → 여기로 전달
    public void OnNodeClicked(Node node)
    {
        string targetNodeId = node.Data.id;

        // 시작 노드 클릭 시 무반응
        if (node.Data.nodeType == NodeType.Start)
        {
            Debug.Log("[Planning] 시작 노드는 선택할 수 없습니다.");
            return;
        }

        // 이미 결정이 완료된 노드 클릭 시 무반응
        if (decidedNodeIds.Contains(targetNodeId))
        {
            Debug.Log($"[Planning] {targetNodeId}는 이미 결정된 노드입니다.");
            return;
        }

        // 현재 위치에서 직접 연결된 노드인지 확인
        RouteData connectedRoute = SelectionValidator.Instance.FindConnectedRoute(
            currentNodeId, targetNodeId, StageManager.Instance.CurrentStageData);

        if (connectedRoute == null)
        {
            Debug.Log($"[Planning] {currentNodeId} → {targetNodeId} 연결 경로 없음.");
            return;
        }

        // 경로 확정 → 이동수단 카드 표시
        pendingRoute = connectedRoute;
        PlanningUI.Instance.ShowSelectionCards(connectedRoute);
        Debug.Log($"[Planning] 경로 확정: {connectedRoute.id} / 이동수단을 선택하세요.");
    }

    // SelectionCardUI → 이동수단 선택 후 결정 버튼 클릭 시 호출
    public void OnTransportSelected(TransportType transport)
    {
        if (pendingRoute == null)
        {
            Debug.LogWarning("[Planning] 선택된 경로가 없습니다. 노드를 먼저 클릭하세요.");
            return;
        }

        if (!SelectionValidator.Instance.IsTransportValid(pendingRoute, transport)) return;

        // 출발 노드를 결정 완료 목록에 추가 (되돌아오기 차단)
        decidedNodeIds.Add(pendingRoute.fromNodeId);

        // 출발 노드 시각적 비활성화
        GameObject fromObj = GameObject.Find(pendingRoute.fromNodeId);
        if (fromObj != null)
            fromObj.GetComponent<Node>()?.SetDecided();

        // 선택 확정 및 현재 위치 이동
        Selections.Add(new SelectionEntry(pendingRoute, transport));
        currentNodeId = pendingRoute.toNodeId;
        pendingRoute = null;

        Debug.Log($"[Planning] 선택 확정 ({Selections.Count}번째): {transport} / 현재 위치: {currentNodeId}");

        // 출발지 → 도착지까지 모든 경로 선택 완료 시 결정 버튼 활성화
        if (SelectionValidator.Instance.IsSelectionComplete(
                Selections, StageManager.Instance.CurrentStageData))
        {
            Debug.Log("[Planning] 전체 경로 선택 완료! 결정 버튼 활성화");
            PlanningUI.Instance.SetDecideButtonActive(true);
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
}