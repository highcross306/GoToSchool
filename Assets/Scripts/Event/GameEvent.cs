// ============================================================
// GameEvent.cs
// 역할: 이벤트 하나의 데이터 ScriptableObject
//       확률, 발동 시점, 효과 목록을 보관
// 생성: Assets/ScriptableObjects/Events 폴더 우클릭
//       → Create → Game → Game Event
// ============================================================

using UnityEngine;

// 이벤트 발동 시점
public enum EventTrigger
{
    OnStageStart,        // 스테이지 시작 시
    OnNodeArrival,       // 노드 도착 시
    OnTransportSelected, // 이동수단 결정 시
    OnRouteComplete,     // 경로 이동 완료 시
}

// 이벤트 효과 타입
public enum EffectType
{
    Budget,           // 자금 변화 (양수: 증가, 음수: 감소)
    Time,             // 시간 변화 (양수: 단축, 음수: 추가 소요)
    DisableTransport, // 특정 이동수단 사용 불가
    BonusScore,       // 점수 보너스
}

// 이벤트 효과 하나
[System.Serializable]
public class GameEventEffect
{
    public EffectType effectType;
    public int value;           // 자금/시간/점수 변화량
    public TransportType disableTarget;   // effectType이 DisableTransport일 때만 사용
}

[CreateAssetMenu(fileName = "GameEvent", menuName = "Game/Game Event")]
public class GameEvent : ScriptableObject
{
    [Header("기본 정보")]
    public string eventName;        // 이벤트 이름 (예: "버스 연착")
    [TextArea]
    public string description;      // 이벤트 설명 (UI 표시용)

    [Header("발동 조건")]
    public EventTrigger trigger;    // 언제 발동되는지
    [Range(0f, 1f)]
    public float probability = 0.3f; // 발동 확률 (0.0 ~ 1.0)

    [Header("효과 목록")]
    public GameEventEffect[] effects; // 발동 시 적용될 효과들
}