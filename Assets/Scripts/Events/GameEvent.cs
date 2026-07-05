// ============================================================
// GameEvent.cs
// 역할: 이벤트 하나의 데이터 ScriptableObject
// 생성: Assets/ScriptableObjects/Events/ 우클릭
//       → Create → Game → Game Event
// ============================================================

using UnityEngine;

public enum EventTrigger
{
    OnStageStart,
    OnRouteComplete, // 경로 이동 완료 시 (이동수단 확정 후 도착)
}

public enum EffectType
{
    Budget,           // 자금 변화 (양수: 증가, 음수: 감소)
    Time,             // 시간 변화 (양수: 단축, 음수: 추가 소요)
    BonusScore,       // 점수 보너스
}

[System.Serializable]
public class GameEventEffect
{
    public EffectType effectType;
    public int value;
}

[CreateAssetMenu(fileName = "GameEvent", menuName = "Game/Game Event")]
public class GameEvent : ScriptableObject
{
    [Header("기본 정보")]
    public string eventName;
    [TextArea]
    public string uiMessage; // 플레이어에게 표시될 메시지

    [Header("발동 조건")]
    [Range(0f, 1f)]
    public float probability = 0.3f;

    [Header("이동수단 조건 (비워두면 모든 이동수단에 적용)")]
    public TransportType[] targetTransports;

    [Header("출발 노드 조건 (비워두면 모든 노드에 적용)")]
    public NodeData[] targetFromNodes;

    [Header("노드별 이벤트 여부")]
    [Tooltip("true면 해당 노드에서 기존 이동수단 이벤트를 대체")]
    public bool isNodeSpecific = false;

    [Header("효과 목록")]
    public GameEventEffect[] effects;
}