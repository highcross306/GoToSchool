// ============================================================
// RouteData.cs
// 역할: 경로 하나의 순수 데이터
//       position 제거 — 노드 위치 기반으로 자동 계산
// ============================================================

using System;
using UnityEngine;

[Serializable]
public class RouteData
{
    public string id;                // 경로 고유 ID (예: "route_01")
    public string fromNodeId;        // 출발 노드 ID
    public string toNodeId;          // 도착 노드 ID
    public bool isBidirectional;   // true: 양방향 / false: 단방향
    public TransportType[] allowedTransports; // 허용된 이동수단 목록
}