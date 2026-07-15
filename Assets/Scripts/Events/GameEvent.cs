// ============================================================
// GameEvent.cs
// 역할: 이벤트 하나의 데이터 ScriptableObject
// 생성: Assets/ScriptableObjects/Events/ 우클릭
//       → Create → Game → Game Event
// ============================================================

using UnityEngine;

public enum EffectType
{
    Budget,     // 자금 변화 (양수: 증가, 음수: 감소)
    Time,       // 시간 변화 (양수: 단축, 음수: 추가 소요)
    BonusScore, // 점수 보너스
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

    [Header("발동 확률 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float probability = 0.3f;

    [Header("효과 목록")]
    public GameEventEffect[] effects;
}