// ============================================================
// ExecutionManager.cs
// 역할: 단일 경로 이동 실행 및 매 노드마다 결과 판정
// ============================================================

using System.Collections;
using System.Collections.Generic;
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

    // 단일 경로 실행 (결정 버튼 클릭 시 호출)
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

    // 단일 이동 완료 후 처리
    private void OnSingleMoveComplete(SelectionEntry entry)
    {
        // 비용/시간 차감
        TransportSetting setting = transportSettings.Get(entry.transport);
        if (setting != null)
            PlayerBudget.Instance.Consume(setting.cost, setting.timeMinutes);

        MessageSystem.L($"이동 완료 → {entry.route.toNode.name}");

        bool isFail = PlayerBudget.Instance.IsBudgetOver() ||
                      PlayerBudget.Instance.IsTimeOver();
        bool isEnd = entry.route.toNode.nodeType == NodeType.End;

        if (isFail)
        {
            // 실패 — 결과창 표시
            StartCoroutine(ShowResultAfterDelay(0.5f));
        }
        else if (isEnd)
        {
            // 도착 성공 — 결과창 표시
            StartCoroutine(ShowResultAfterDelay(0.5f));
        }
        else
        {
            // 중간 노드 도착 — 다음 경로 선택 계속
            GameState.CurrentPhase = Phase.Planning;
            PlanningUI.Instance.ResetForNextSelection();
            MessageSystem.L("중간 노드 도착. 다음 경로를 선택하세요.");
        }
    }

    private IEnumerator ShowResultAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.ShowResult();
    }
}