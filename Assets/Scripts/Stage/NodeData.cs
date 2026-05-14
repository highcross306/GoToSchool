// ============================================================
// NodeData.cs
// 역할: 노드 하나의 데이터 ScriptableObject
//       outgoingRoutes로 연결된 경로를 직접 참조
// 생성: Assets/ScriptableObjects/Nodes 폴더 우클릭
//       → Create → Game → Node Data
// ============================================================

using UnityEngine;

public enum NodeType
{
    Start,
    Checkpoint,
    End
}

public enum TransportType
{
    Walk,
    Bus,
    Taxi
}

[CreateAssetMenu(fileName = "NodeData", menuName = "Game/Node Data")]
public class NodeData : ScriptableObject
{
    public NodeType nodeType;
    public Vector2 position;
    public RouteData[] outgoingRoutes; // 이 노드에서 출발하는 경로 직접 참조
}