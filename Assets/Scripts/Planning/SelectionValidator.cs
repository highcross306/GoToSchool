// ============================================================
// SelectionValidator.cs
// 역할: 플레이어의 선택이 유효한지 검사 전담
//       PlanningManager에서 호출
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

    // 선택한 이동수단이 해당 노드에서 허용되는지 확인
    public bool IsTransportValid(NodeData nodeData, TransportType transport)
    {
        foreach (TransportType available in nodeData.availableTransports)
        {
            if (available == transport) return true;
        }
        Debug.Log($"[Validator] {nodeData.id}에서 {transport}는 허용되지 않습니다.");
        return false;
    }

    // 현재 노드와 클릭한 노드가 직접 연결되어 있는지 확인
    // 연결된 경우 해당 RouteData 반환, 없으면 null
    public RouteData FindConnectedRoute(string currentNodeId, string targetNodeId, StageData stageData)
    {
        foreach (RouteData route in stageData.routes)
        {
            // 단방향: fromNode → toNode
            if (route.fromNodeId == currentNodeId && route.toNodeId == targetNodeId)
                return route;

            // 양방향: toNode → fromNode도 허용
            if (route.isBidirectional &&
                route.fromNodeId == targetNodeId && route.toNodeId == currentNodeId)
                return route;
        }
        return null;
    }

    // 선택 목록이 완료됐는지 확인
    public bool IsSelectionComplete(List<SelectionEntry> selections, StageData stageData)
    {
        return selections.Count == stageData.routes.Length;
    }
}