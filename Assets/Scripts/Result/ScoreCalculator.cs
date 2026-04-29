// ============================================================
// ScoreCalculator.cs
// 역할: 클리어 성공 시 최종 점수 계산
//       남은 자금 + 남은 시간 + 이동수단 보너스를 합산
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class ScoreCalculator : MonoBehaviour
{
    public static ScoreCalculator Instance { get; private set; }

    [Header("점수 가중치")]
    public int budgetScorePerWon = 1;  // 남은 자금 1원당 점수
    public int timeScorePerMinute = 5;  // 남은 시간 1분당 점수
    public int walkBonus = 30; // 걷기 선택 시 보너스
    public int busBonus = 10; // 버스 선택 시 보너스
    public int taxiBonus = 0;  // 택시 선택 시 보너스 없음

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public int Calculate()
    {
        int budgetScore = CalculateBudgetScore();
        int timeScore = CalculateTimeScore();
        int transportBonus = CalculateTransportBonus();
        int totalScore = budgetScore + timeScore + transportBonus;

        Debug.Log($"[Score] 자금 점수: {budgetScore} / 시간 점수: {timeScore} " +
                  $"/ 이동수단 보너스: {transportBonus} / 합계: {totalScore}");

        return totalScore;
    }

    // 남은 자금 점수
    private int CalculateBudgetScore()
    {
        return Mathf.Max(0, PlayerBudget.Instance.RemainingBudget) * budgetScorePerWon;
    }

    // 남은 시간 점수
    private int CalculateTimeScore()
    {
        int limitMinutes = PlayerBudget.Instance.TimeLimitSeconds / 60;
        int remainMinutes = Mathf.Max(0, limitMinutes - PlayerBudget.Instance.ElapsedMinutes);
        return remainMinutes * timeScorePerMinute;
    }

    // 이동수단별 보너스 점수
    private int CalculateTransportBonus()
    {
        int bonus = 0;
        List<SelectionEntry> selections = PlanningManager.Instance.Selections;

        foreach (SelectionEntry entry in selections)
        {
            bonus += entry.transport switch
            {
                TransportType.Walk => walkBonus,
                TransportType.Bus => busBonus,
                TransportType.Taxi => taxiBonus,
                _ => 0
            };
        }
        return bonus;
    }
}