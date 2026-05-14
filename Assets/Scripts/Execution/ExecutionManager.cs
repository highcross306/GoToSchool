// ============================================================
// ExecutionManager.cs
// 역할: 실행 단계 전체 관리
//       StageManager.nodeMap으로 Node 컴포넌트 직접 조회
// ============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecutionManager : MonoBehaviour
{
    public static ExecutionManager Instance { get; private set; }

    [Header("이동수단 설정 연결")]
    public TransportSettings transportSettings;

    private List<SelectionEntry> selections;
    private int currentIndex = 0;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartExecution()
    {
        selections = PlanningManager.Instance.Selections;
        currentIndex = 0;
        Debug.Log($"[Execution] 실행 단계 시작 — 총 {selections.Count}개 경로");
        ExecuteNext();
    }

    private void ExecuteNext()
    {
        if (currentIndex >= selections.Count)
        {
            Debug.Log("[Execution] 모든 경로 이동 완료");
            StartCoroutine(ShowResultAfterDelay(0.5f));
            return;
        }

        SelectionEntry entry = selections[currentIndex];
        Debug.Log($"[Execution] {currentIndex + 1}번째 이동: " +
                  $"{entry.route.fromNode.name} → {entry.route.toNode.name} ({entry.transport})");

        // nodeMap에서 목표 노드 컴포넌트 직접 조회
        Node targetNode = StageManager.Instance.GetNode(entry.route.toNode);
        if (targetNode == null)
        {
            Debug.LogError($"[Execution] 노드를 찾을 수 없습니다: {entry.route.toNode.name}");
            return;
        }

        PlayerMover.Instance.MoveTo(targetNode.transform.position, OnMoveComplete);
    }

    private void OnMoveComplete()
    {
        SelectionEntry entry = selections[currentIndex];
        TransportSetting setting = transportSettings.Get(entry.transport);
        if (setting != null)
            PlayerBudget.Instance.Consume(setting.cost, setting.timeMinutes);

        currentIndex++;
        ExecuteNext();
    }

    private IEnumerator ShowResultAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.ShowResult();
    }
}