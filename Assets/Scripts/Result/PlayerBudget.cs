// ============================================================
// PlayerBudget.cs
// 역할: 현재 자금과 경과 시간 실시간 추적
//       ExecutionManager가 이동 완료 시 Consume() 호출
//       HUDController가 이 값을 구독해 화면에 표시
// ============================================================

using UnityEngine;

public class PlayerBudget : MonoBehaviour
{
    public static PlayerBudget Instance { get; private set; }

    public int RemainingBudget { get; private set; }
    public int ElapsedMinutes { get; private set; }
    public int TimeLimitSeconds { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // StageManager가 스테이지 시작 시 호출
    public void Initialize(int initialBudget, int timeLimitSeconds)
    {
        RemainingBudget = initialBudget;
        ElapsedMinutes = 0;
        TimeLimitSeconds = timeLimitSeconds;
        Debug.Log($"[Budget] 초기화 — 자금: {RemainingBudget}원 / 제한시간: {TimeLimitSeconds / 60}분");
    }

    // ExecutionManager가 경로 하나 완료 시 호출
    public void Consume(int cost, int minutes)
    {
        RemainingBudget -= cost;
        ElapsedMinutes += minutes;
        Debug.Log($"[Budget] 소비 — -{cost}원 / +{minutes}분 " +
                  $"| 잔액: {RemainingBudget}원 / 경과: {ElapsedMinutes}분");
    }

    // 자금 초과 여부
    public bool IsBudgetOver() => RemainingBudget < 0;

    // 시간 초과 여부
    public bool IsTimeOver() => ElapsedMinutes * 60 > TimeLimitSeconds;
}