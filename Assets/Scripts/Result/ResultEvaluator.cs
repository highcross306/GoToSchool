// ============================================================
// ResultEvaluator.cs
// 역할: 성공/실패 판정 전담
//       GameManager.ShowResult() → 여기서 판정
//       판정 결과를 ScoreCalculator와 GameManager에 전달
// ============================================================

using UnityEngine;

public class ResultEvaluator : MonoBehaviour
{
    public static ResultEvaluator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // GameManager.ShowResult() → 여기서 판정 시작
    public void Evaluate()
    {
        bool isBudgetOver = PlayerBudget.Instance.IsBudgetOver();
        bool isTimeOver = PlayerBudget.Instance.IsTimeOver();

        if (isBudgetOver || isTimeOver)
        {
            // 실패
            string reason = isBudgetOver ? "자금 부족" : "시간 초과";
            if (isBudgetOver && isTimeOver) reason = "자금 부족 + 시간 초과";

            Debug.Log($"[Result] 실패 — {reason}");
            ResultUI.Instance.ShowFail(reason);
        }
        else
        {
            // 성공
            int score = ScoreCalculator.Instance.Calculate();

            // 4개 스테이지 누적 점수에 이번 스테이지 점수를 더한다.
            // (엔딩 화면에서 GameState.TotalScore를 그대로 사용)
            GameState.TotalScore += score;

            Debug.Log($"[Result] 성공 — 최종 점수: {score}점 / 누적 점수: {GameState.TotalScore}점");
            ResultUI.Instance.ShowSuccess(score);
        }
    }
}