// ============================================================
// PlayerBudget.cs
// 역할: 자금과 시간 추적
//       실시간 시간 감소: 현실 시간 tickInterval초마다 tickMinutes분 차감
// ============================================================

using System.Collections;
using UnityEngine;

public class PlayerBudget : MonoBehaviour
{
    public static PlayerBudget Instance { get; private set; }

    [Header("실시간 시간 감소 설정")]
    public float tickInterval = 10f; // 현실 시간 몇 초마다
    public int tickMinutes = 3;   // 게임 시간 몇 분 차감

    public int RemainingBudget { get; private set; }
    public int ElapsedMinutes { get; private set; }
    public int TimeLimitSeconds { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Initialize(int initialBudget, int timeLimitSeconds)
    {
        RemainingBudget = initialBudget;
        ElapsedMinutes = 0;
        TimeLimitSeconds = timeLimitSeconds;

        // 실시간 시간 감소 비활성화 (활성화하려면 아래 주석 해제)
        // StopAllCoroutines();
        // StartCoroutine(TimeTickCoroutine());

        Debug.Log($"[Budget] 초기화 — 자금: {RemainingBudget}원 / 제한시간: {TimeLimitSeconds / 60}분");
    }

    // 이동수단 확정 시 즉시 차감
    public void Consume(int cost, int minutes)
    {
        RemainingBudget -= cost;
        ElapsedMinutes += minutes;
        Debug.Log($"[Budget] 소비 — -{cost}원 / +{minutes}분 " +
                  $"| 잔액: {RemainingBudget}원 / 경과: {ElapsedMinutes}분");
    }

    // 실시간 시간 감소 코루틴
    // Planning 단계에서만 타이머가 진행됨
    // Execution(이동 중), Result(결과 화면)에서는 타이머 정지
    private IEnumerator TimeTickCoroutine()
    {
        float accumulated = 0f;

        while (true)
        {
            yield return null; // 매 프레임 확인

            // 결과 화면 → 코루틴 종료
            if (GameState.CurrentPhase == Phase.Result) yield break;

            // 이동 중 → 타이머 정지 (accumulated 유지)
            if (GameState.CurrentPhase == Phase.Execution) continue;

            // Planning 단계에서만 시간 누적
            accumulated += Time.deltaTime;

            if (accumulated >= tickInterval)
            {
                accumulated -= tickInterval;
                ElapsedMinutes += tickMinutes;
                Debug.Log($"[Budget] 시간 틱 +{tickMinutes}분 / " +
                          $"경과: {ElapsedMinutes}분 / " +
                          $"남은: {Mathf.Max(0, TimeLimitSeconds / 60 - ElapsedMinutes)}분");

                // 시간 초과 시 즉시 실패 판정
                if (IsTimeOver())
                {
                    Debug.Log("[Budget] 시간 초과 — 실패 판정");
                    GameManager.Instance.ShowResult();
                    yield break;
                }
            }
        }
    }

    public bool IsBudgetOver() => RemainingBudget < 0;
    public bool IsTimeOver() => ElapsedMinutes * 60 > TimeLimitSeconds;
}