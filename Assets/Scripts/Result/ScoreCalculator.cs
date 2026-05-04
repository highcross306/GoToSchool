// ============================================================
// ScoreCalculator.cs
// 역할: 클리어 성공 시 최종 점수 계산
// 공식: 점수 = 남은 자금 + (남은 시간 × 50)
// ============================================================

using UnityEngine;

public class ScoreCalculator : MonoBehaviour
{
    public static ScoreCalculator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public int Calculate()
    {
        int remainingBudget = Mathf.Max(0, PlayerBudget.Instance.RemainingBudget);
        int remainingMinutes = Mathf.Max(0,
            PlayerBudget.Instance.TimeLimitSeconds / 60 - PlayerBudget.Instance.ElapsedMinutes);

        int score = remainingBudget + (remainingMinutes * 50);

        Debug.Log($"[Score] 남은 자금: {remainingBudget}원 / 남은 시간: {remainingMinutes}분 " +
                  $"/ 점수: {remainingBudget} + ({remainingMinutes} × 50) = {score}점");

        return score;
    }
}