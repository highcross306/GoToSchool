// ============================================================
// NodeData.cs
// 역할: 노드 하나의 순수 데이터
//       이동수단 허용 목록은 RouteData에서 관리
// ============================================================

using System;
using UnityEngine;

public enum NodeType
{
    Start,       // 출발 노드
    Checkpoint,  // 거점 노드
    End          // 도착 노드
}

public enum TransportType
{
    Walk,  // 걷기
    Bus,   // 버스
    Taxi   // 택시
}

[Serializable]
public class NodeData
{
    public string id;       // 노드 고유 ID (예: "node_01")
    public NodeType nodeType; // 노드 타입 (출발/거점/도착)
    public Vector2 position; // 씬 배치 좌표
}