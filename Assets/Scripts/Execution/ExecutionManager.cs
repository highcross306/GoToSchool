// ============================================================
// ExecutionManager.cs
// 역할: 단일 경로 이동 실행 및 매 노드마다 결과 판정
//       이동 전 이벤트 판정 → 메시지 표시 → 클릭 후 이동
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

    // 결정 버튼 클릭 후 호출 — 이벤트 판정 후 이동
    public void ExecuteSingle(SelectionEntry entry)
    {
        // 이동 전 이벤트 판정
        GameEvent triggeredEvent = null;
        if (EventManager.Instance != null)
        {
            triggeredEvent = EventManager.Instance.TryTriggerEventForRoute(
                entry.transport,
                entry.route.fromNode,
                StageManager.Instance.CurrentStageData.useTransportEvents
            );
        }

        if (triggeredEvent != null && !string.IsNullOrEmpty(triggeredEvent.uiMessage))
        {
            // 이벤트 발동 → 메시지 패널 표시, 클릭 후 이동
            MessageSystem.L($"이벤트 발동: {triggeredEvent.eventName}");

            if (EventMessagePanel.Instance != null)
                EventMessagePanel.Instance.Show(triggeredEvent.uiMessage, () => StartMovement(entry));
            else
                StartMovement(entry); // 패널 없으면 바로 이동
        }
        else
        {
            // 이벤트 없음 → 바로 이동
            StartMovement(entry);
        }
    }

    // 실제 이동 시작
    private void StartMovement(SelectionEntry entry)
    {
        Node targetNode = StageManager.Instance.GetNode(entry.route.toNode);
        if (targetNode == null)
        {
            Debug.LogError($"[Execution] 노드를 찾을 수 없습니다: {entry.route.toNode.name}");
            return;
        }

        MessageSystem.L($"이동 시작: {entry.route.fromNode.name} → {entry.route.toNode.name} ({entry.transport})");

        if (SoundManager.Instance != null)
        {
            if (entry.transport == TransportType.Walk)
                SoundManager.Instance.PlayWithPitchVariance(SoundManager.Sfx.Walk);
            else
                SoundManager.Instance.Play(SoundManager.Sfx.CarMove);
        }

        PlayerMover.Instance.MoveTo(targetNode.transform.position, () => OnSingleMoveComplete(entry), entry.transport);
    }

    // 이동 완료 후 처리
    private void OnSingleMoveComplete(SelectionEntry entry)
    {
        // 기본 비용/시간 차감
        TransportSetting setting = transportSettings.Get(entry.transport);
        if (setting != null)
        {
            PlayerBudget.Instance.Consume(setting.cost, setting.timeMinutes);
            if (SoundManager.Instance != null)
                SoundManager.Instance.Play(SoundManager.Sfx.Cash);
        }

        MessageSystem.L($"이동 완료 → {entry.route.toNode.name}");

        // 결과 판정
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
            PlanningManager.Instance.HighlightReachableNodes();
            MessageSystem.L("중간 노드 도착. 다음 경로를 선택하세요.");
        }
    }

    private IEnumerator ShowResultAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.ShowResult();
    }
}