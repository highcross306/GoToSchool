// ============================================================
// SelectionValidator.cs
// 역할: 플레이어의 선택이 유효한지 검사 전담
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

    // 선택한 이동수단이 해당 경로에서 허용되는지 확인
    public bool IsTransportValid(RouteData routeData, TransportType transport)
    {
        foreach (TransportType allowed in routeData.allowedTransports)
        {
            if (allowed == transport) return true;
        }
        Debug.Log($"[Validator] {routeData.id}에서 {transport}는 허용되지 않습니다.");
        return false;
    }

    // 현재 노드에서 클릭한 노드로 가는 연결 경로 탐색
    public RouteData FindConnectedRoute(string currentNodeId, string targetNodeId, StageData stageData)
    {
        foreach (RouteData route in stageData.routes)
        {
            if (route.fromNodeId == currentNodeId && route.toNodeId == targetNodeId)
                return route;
        }
        return null;
    }

    // 선택 목록 완료 여부 확인
    public bool IsSelectionComplete(List<SelectionEntry> selections, StageData stageData)
    {
        return selections.Count == stageData.routes.Length;
    }
}