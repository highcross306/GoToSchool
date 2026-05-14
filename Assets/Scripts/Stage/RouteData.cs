// ============================================================
// RouteData.cs
// 역할: 경로 하나의 순수 데이터
//       단방향 고정 (fromNodeId → toNodeId)
// ============================================================

using System;
using UnityEngine;

[Serializable]
public class RouteData
{
    public string id;                // 경로 고유 ID (예: "route_01")
    public string fromNodeId;        // 출발 노드 ID
    public string toNodeId;          // 도착 노드 ID
    public TransportType[] allowedTransports; // 허용된 이동수단 목록
}