// ============================================================
// EventManager.cs
// 역할: 이벤트 발동 판정 및 효과 적용
//       우선순위: 강화노드 이벤트 > 대중교통 이벤트
//       한 경로당 이벤트 1개만 발동
// ============================================================

using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Header("대중교통 이벤트 테이블")]
    public TransportEventTable transportEventTable;

    public int BonusScore { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Initialize()
    {
        BonusScore = 0;
    }

    // 경로 완료 시 호출
    // 우선순위: 강화노드 이벤트 → 대중교통 이벤트 → 없음
    public GameEvent TryTriggerEventForRoute(
        TransportType transport,
        NodeData fromNode,
        bool useTransportEvents)
    {
        // 1순위: 강화노드 이벤트 (최우선)
        // NodeType이 기본 타입(Start/Checkpoint/End)이 아니면 강화 노드
        if (fromNode != null && fromNode.IsEnhancedNode &&
            fromNode.enhancedEvents != null && fromNode.enhancedEvents.Length > 0)
        {
            // 이동수단 조건에 맞는 이벤트만 후보로 필터링
            // (restrictToTransport가 false면 모든 이동수단에 적용)
            System.Collections.Generic.List<GameEvent> candidates = new();
            foreach (GameEvent e in fromNode.enhancedEvents)
            {
                if (e == null) continue;
                if (e.restrictToTransport && e.requiredTransport != transport) continue;
                candidates.Add(e);
            }

            GameEvent ev = RollFirst(candidates.ToArray());
            if (ev != null)
            {
                ApplyEvent(ev);
                Debug.Log($"[EventManager] 강화노드 이벤트 발동: {ev.eventName} (이동수단: {transport})");
                return ev;
            }

            // 이동수단 조건에 맞는 강화노드 이벤트가 없거나 발동 실패 시
            // 대중교통 이벤트로 넘어가지 않고 종료 (강화노드는 항상 최우선)
            return null;
        }

        // 2순위: 대중교통 이벤트 (스테이지 2부터)
        if (useTransportEvents && transportEventTable != null)
        {
            GameEvent[] events = transportEventTable.GetEvents(transport);
            GameEvent ev = RollFirst(events);
            if (ev != null)
            {
                ApplyEvent(ev);
                Debug.Log($"[EventManager] 대중교통 이벤트 발동: {ev.eventName}");
                return ev;
            }
        }

        return null;
    }

    // 이벤트 목록에서 순서대로 확률 판정 — 첫 번째 발동 이벤트 반환
    private GameEvent RollFirst(GameEvent[] events)
    {
        if (events == null) return null;
        foreach (GameEvent ev in events)
        {
            if (ev == null) continue;
            if (Random.value <= ev.probability)
                return ev;
        }
        return null;
    }

    // 이벤트 효과 적용
    private void ApplyEvent(GameEvent ev)
    {
        foreach (GameEventEffect effect in ev.effects)
        {
            switch (effect.effectType)
            {
                case EffectType.Budget:
                    // 양수: 자금 증가 / 음수: 자금 감소
                    PlayerBudget.Instance.Consume(-effect.value, 0);
                    break;
                case EffectType.Time:
                    // 양수: 시간 단축 / 음수: 시간 추가
                    PlayerBudget.Instance.Consume(0, -effect.value);
                    break;
                case EffectType.BonusScore:
                    BonusScore += effect.value;
                    break;
            }
        }
    }
}