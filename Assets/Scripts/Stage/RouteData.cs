// ============================================================
// RouteData.cs
// 역할: 경로 하나의 순수 데이터
//       두 노드를 잇고, 이동수단별 비용/시간 페어를 보관
// ============================================================

using System;

[Serializable]
public class TransportCost
{
    public TransportType transportType; // 이동수단 종류
    public int cost;          // 비용 (원)
    public int timeMinutes;   // 소요 시간 (분)
}

[Serializable]
public class RouteData
{
    public string id;              // 경로 고유 ID (예: "route_01")
    public string fromNodeId;      // 출발 노드 ID
    public string toNodeId;        // 도착 노드 ID
    public bool isBidirectional; // true: 양방향 / false: 단방향
    public TransportCost[] transportCosts;  // 이동수단별 비용/시간 목록
}