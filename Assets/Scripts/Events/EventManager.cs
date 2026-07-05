// ============================================================
// EventManager.cs
// 역할: 이벤트 발동 판정 + 효과 적용
//       한 경로당 최대 하나의 이벤트만 발동 (긍정/부정/없음)
//       노드별 이벤트가 있으면 기존 이동수단 이벤트를 대체
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

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

    // 경로 완료 시 호출 — 이동수단 + 출발 노드 기반 이벤트 판정
    // 반환값: 발동된 이벤트 (없으면 null)
    public GameEvent TryTriggerEventForRoute(
        GameEvent[] events,
        TransportType transport,
        NodeData fromNode)
    {
        if (events == null || events.Length == 0) return null;

        // 1. 노드별 이벤트 필터링
        List<GameEvent> nodeSpecific = FilterEvents(events, transport, fromNode, nodeSpecificOnly: true);
        List<GameEvent> general = FilterEvents(events, transport, fromNode, nodeSpecificOnly: false);

        // 노드별 이벤트가 있으면 그것만 사용 (일반 이벤트 대체)
        List<GameEvent> candidates = nodeSpecific.Count > 0 ? nodeSpecific : general;

        // 2. 순서대로 확률 판정 — 첫 번째 발동 이벤트만 적용 (중복 방지)
        foreach (GameEvent ev in candidates)
        {
            if (Roll(ev.probability))
            {
                ApplyEvent(ev);
                Debug.Log($"[EventManager] 이벤트 발동: {ev.eventName} ({ev.probability * 100}%)");
                return ev;
            }
        }

        return null;
    }

    // 이동수단 + 노드 조건으로 이벤트 필터링
    private List<GameEvent> FilterEvents(
        GameEvent[] events,
        TransportType transport,
        NodeData fromNode,
        bool nodeSpecificOnly)
    {
        List<GameEvent> result = new();

        foreach (GameEvent ev in events)
        {
            if (ev == null) continue;
            if (ev.isNodeSpecific != nodeSpecificOnly) continue;

            // 이동수단 조건 확인
            if (ev.targetTransports != null && ev.targetTransports.Length > 0)
            {
                bool transportMatch = false;
                foreach (TransportType t in ev.targetTransports)
                    if (t == transport) { transportMatch = true; break; }
                if (!transportMatch) continue;
            }

            // 출발 노드 조건 확인
            if (ev.targetFromNodes != null && ev.targetFromNodes.Length > 0)
            {
                bool nodeMatch = false;
                foreach (NodeData n in ev.targetFromNodes)
                    if (n == fromNode) { nodeMatch = true; break; }
                if (!nodeMatch) continue;
            }

            result.Add(ev);
        }

        return result;
    }

    private bool Roll(float probability)
    {
        return Random.value <= probability;
    }

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