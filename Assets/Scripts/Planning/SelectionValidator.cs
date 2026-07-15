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

    // 경로에서 실제로 허용되는 이동수단 목록 계산
    // fromNode에 출발 제한이 걸려있으면 RouteData.allowedTransports는 무시하고
    // fromNode.allowedTransportsOverride를 최우선으로 사용
    public TransportType[] GetAllowedTransports(RouteData route)
    {
        if (route.fromNode != null && route.fromNode.restrictTransportsOnDeparture)
            return route.fromNode.allowedTransportsOverride ?? new TransportType[0];

        return route.allowedTransports;
    }

    // 해당 경로에서 이동수단 허용 여부 확인
    public bool IsTransportValid(RouteData route, TransportType transport)
    {
        foreach (TransportType allowed in GetAllowedTransports(route))
        {
            if (allowed == transport) return true;
        }
        Debug.Log($"[Validator] {route.name}에서 {transport}는 허용되지 않습니다.");
        return false;
    }

    // 전체 경로 선택 완료 여부 확인
    public bool IsSelectionComplete(List<SelectionEntry> selections, StageData stageData)
    {
        return selections.Count == stageData.GetTotalRouteCount();
    }
}