// ============================================================
// EventManager.cs
// 역할: 이벤트 발동 여부 판정 + 효과 적용
//       각 시점에서 TryTriggerEvents()를 호출해 사용
// 부착: 씬에 [EventManager] 오브젝트에 부착
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    // 현재 스테이지에서 비활성화된 이동수단 목록
    public HashSet<TransportType> DisabledTransports { get; private set; } = new();

    // 이번 스테이지에서 추가된 점수 보너스
    public int BonusScore { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 스테이지 시작 시 초기화
    public void Initialize()
    {
        DisabledTransports.Clear();
        BonusScore = 0;
    }

    // 특정 시점에 해당하는 이벤트들을 순서대로 확률 판정 후 발동
    // events: 현재 스테이지의 이벤트 목록 (StageData에서 전달)
    // trigger: 현재 발동 시점
    // 반환값: 실제로 발동된 이벤트 목록
    public List<GameEvent> TryTriggerEvents(GameEvent[] events, EventTrigger trigger)
    {
        List<GameEvent> triggered = new();
        if (events == null) return triggered;

        foreach (GameEvent ev in events)
        {
            if (ev == null) continue;
            if (ev.trigger != trigger) continue;
            if (!Roll(ev.probability)) continue;

            ApplyEvent(ev);
            triggered.Add(ev);
            Debug.Log($"[EventManager] 이벤트 발동: {ev.eventName} (확률: {ev.probability * 100}%)");
        }

        return triggered;
    }

    // 확률 판정 — true면 이벤트 발동
    public bool Roll(float probability)
    {
        return Random.value <= probability;
    }

    // 이벤트 효과 적용
    private void ApplyEvent(GameEvent ev)
    {
        foreach (GameEventEffect effect in ev.effects)
        {
            switch (effect.effectType)
            {
                case EffectType.Budget:
                    PlayerBudget.Instance.Consume(-effect.value, 0);
                    // 양수 value → 자금 증가 (Consume에 음수 전달)
                    // 음수 value → 자금 감소 (Consume에 양수 전달)
                    Debug.Log($"[EventManager] 자금 변화: {effect.value}원");
                    break;

                case EffectType.Time:
                    PlayerBudget.Instance.Consume(0, -effect.value);
                    // 양수 value → 시간 단축 (Consume에 음수 전달)
                    // 음수 value → 시간 추가 소요
                    Debug.Log($"[EventManager] 시간 변화: {effect.value}분");
                    break;

                case EffectType.DisableTransport:
                    DisabledTransports.Add(effect.disableTarget);
                    Debug.Log($"[EventManager] 이동수단 비활성화: {effect.disableTarget}");
                    break;

                case EffectType.BonusScore:
                    BonusScore += effect.value;
                    Debug.Log($"[EventManager] 보너스 점수: {effect.value}점");
                    break;
            }
        }
    }

    // 해당 이동수단이 현재 사용 가능한지 확인
    public bool IsTransportAvailable(TransportType transport)
    {
        return !DisabledTransports.Contains(transport);
    }
}