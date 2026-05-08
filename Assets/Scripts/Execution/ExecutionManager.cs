// ============================================================
// ExecutionManager.cs
// 역할: 실행 단계 전체 관리
//       선택 목록을 순서대로 소비
//       TransportSettings에서 비용/시간 조회 후 PlayerBudget 차감
// ============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecutionManager : MonoBehaviour
{
    public static ExecutionManager Instance { get; private set; }

    [Header("이동수단 설정 연결")]
    public TransportSettings transportSettings; // 인스펙터에서 TransportSettings 에셋 연결

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
                  $"{entry.route.fromNodeId} → {entry.route.toNodeId} ({entry.transport})");

        GameObject targetObj = GameObject.Find(entry.route.toNodeId);
        if (targetObj == null)
        {
            Debug.LogError($"[Execution] 노드를 찾을 수 없습니다: {entry.route.toNodeId}");
            return;
        }

        PlayerMover.Instance.MoveTo(targetObj.transform.position, OnMoveComplete);
    }

    private void OnMoveComplete()
    {
        SelectionEntry entry = selections[currentIndex];

        // TransportSettings에서 고정 비용/시간 조회
        TransportSetting setting = transportSettings.Get(entry.transport);
        if (setting != null)
            PlayerBudget.Instance.Consume(setting.cost, setting.timeMinutes);

        currentIndex++;
        ExecuteNext();
    }
    //딜레이만큼 대기 후 실행
    private IEnumerator ShowResultAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.ShowResult();
    }
}