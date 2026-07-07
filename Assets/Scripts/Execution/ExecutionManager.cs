// ============================================================
// ExecutionManager.cs
// 역할: 단일 경로 이동 실행 및 매 노드마다 결과 판정
//       이동 완료 후 이벤트 발동
// ============================================================

using System.Collections;
using UnityEngine;

public class ExecutionManager : MonoBehaviour
{
    public static ExecutionManager Instance { get; private set; }

    [Header("이동수단 설정 연결")]
    public TransportSettings transportSettings;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ExecuteSingle(SelectionEntry entry)
    {
        Node targetNode = StageManager.Instance.GetNode(entry.route.toNode);
        if (targetNode == null)
        {
            Debug.LogError($"[Execution] 노드를 찾을 수 없습니다: {entry.route.toNode.name}");
            return;
        }

        MessageSystem.L($"이동 시작: {entry.route.fromNode.name} → {entry.route.toNode.name} ({entry.transport})");
        PlayerMover.Instance.MoveTo(targetNode.transform.position, () => OnSingleMoveComplete(entry));
    }

    private void OnSingleMoveComplete(SelectionEntry entry)
    {
        // 1. 기본 비용/시간 차감
        TransportSetting setting = transportSettings.Get(entry.transport);
        if (setting != null)
            PlayerBudget.Instance.Consume(setting.cost, setting.timeMinutes);

        // 2. 이벤트 판정 (강화노드 이벤트 > 대중교통 이벤트)
        GameEvent triggeredEvent = null;
        if (EventManager.Instance != null)
        {
            triggeredEvent = EventManager.Instance.TryTriggerEventForRoute(
                entry.transport,
                entry.route.fromNode,
                StageManager.Instance.CurrentStageData.useTransportEvents
            );
        }

        // 3. 이벤트 발동 시 메시지 표시
        if (triggeredEvent != null && !string.IsNullOrEmpty(triggeredEvent.uiMessage))
            MessageSystem.E(triggeredEvent.uiMessage);

        MessageSystem.L($"이동 완료 → {entry.route.toNode.name}");

        // 4. 결과 판정
        bool isFail = PlayerBudget.Instance.IsBudgetOver() ||
                      PlayerBudget.Instance.IsTimeOver();
        bool isEnd = entry.route.toNode.nodeType == NodeType.End;

        if (isFail)
        {
            StartCoroutine(ShowResultAfterDelay(0.5f));
        }
        else if (isEnd)
        {
            StartCoroutine(ShowResultAfterDelay(0.5f));
        }
        else
        {
            GameState.CurrentPhase = Phase.Planning;
            PlanningUI.Instance.ResetForNextSelection();
            PlanningManager.Instance.HighlightReachableNodes(); // 다음 이동 가능한 노드 강조
            MessageSystem.L("중간 노드 도착. 다음 경로를 선택하세요.");
        }
    }

    private IEnumerator ShowResultAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.ShowResult();
    }
}