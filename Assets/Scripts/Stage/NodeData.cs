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
    // 기본 노드
    Start,
    Checkpoint,
    End,

    // 강화 노드 (여기에 새 타입 추가)
    Enhanced_BusStop,  // 버스정류장
    Enhanced_Slum,     // 슬럼
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

    [Header("강화 노드 이벤트")]
    public GameEvent[] enhancedEvents; // 강화 노드 전용 이벤트

    [Header("스프라이트 오버라이드")]
    [Tooltip("비워두면 Node 프리팹의 기본 스프라이트 사용")]
    public Sprite customSprite;

    [Header("이동수단 제한 (이 노드에 서 있을 때 선택 불가한 이동수단)")]
    public TransportType[] disabledTransports;

    // 해당 이동수단이 이 노드에서 선택 불가능한지 확인
    public bool IsTransportDisabled(TransportType transport)
    {
        if (disabledTransports == null) return false;
        foreach (TransportType t in disabledTransports)
            if (t == transport) return true;
        return false;
    }

    // 강화 노드 여부 — NodeType이 기본 타입이 아니면 강화 노드
    public bool IsEnhancedNode =>
        nodeType != NodeType.Start &&
        nodeType != NodeType.Checkpoint &&
        nodeType != NodeType.End;
}