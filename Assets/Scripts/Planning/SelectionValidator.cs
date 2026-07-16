// ============================================================
// SelectionValidator.cs
// 역할: 선택 유효성 검사 전담
//       직접 참조 방식으로 문자열 비교 없이 처리
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class SelectionValidator : MonoBehaviour
{
    public static SelectionValidator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 현재 노드의 outgoingRoutes에서 목표 노드로 가는 경로 탐색
    public RouteData FindConnectedRoute(NodeData currentNode, NodeData targetNode)
    {
        foreach (RouteData route in currentNode.outgoingRoutes)
        {
            if (route.toNode == targetNode)
                return route;
        }
        return null;
    }

    // 해당 경로 + 출발 노드 기준으로 이동수단 사용 가능 여부 확인
    // fromNode: 현재 캐릭터가 서 있는 노드 (null이면 노드 제한 검사 생략)
    public bool IsTransportValid(RouteData route, TransportType transport, NodeData fromNode = null)
    {
        bool routeAllows = false;
        foreach (TransportType allowed in route.allowedTransports)
        {
            if (allowed == transport) { routeAllows = true; break; }
        }

        if (!routeAllows)
        {
            Debug.Log($"[Validator] {route.name}에서 {transport}는 허용되지 않습니다.");
            return false;
        }

        // 노드에서 해당 이동수단이 금지됐는지 확인 (Stage 4: 공사구간 택시 금지 등)
        if (fromNode != null && fromNode.IsTransportDisabled(transport))
        {
            Debug.Log($"[Validator] {fromNode.name}에서는 {transport}를 선택할 수 없습니다.");
            return false;
        }

        return true;
    }

    // 전체 경로 선택 완료 여부 확인
    public bool IsSelectionComplete(List<SelectionEntry> selections, StageData stageData)
    {
        return selections.Count == stageData.GetTotalRouteCount();
    }
}