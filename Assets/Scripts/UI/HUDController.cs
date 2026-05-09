// ============================================================
// HUDController.cs
// 역할: 선택/실행 단계에서 남은 시간과 현재 자금을 화면에 표시
//       좌측 상단: 남은 시간 / 우측 상단: 현재 자금
// 부착: Canvas 하위 HUD 오브젝트에 부착
// ============================================================

using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [Header("UI 연결")]
    public TextMeshProUGUI remainingTimeText; // 좌측 상단: 남은 시간
    public TextMeshProUGUI currentBudgetText; // 우측 상단: 현재 자금

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (GameState.CurrentPhase == Phase.Planning ||
            GameState.CurrentPhase == Phase.Execution)
        {
            UpdateRemainingTime();
            UpdateCurrentBudget();
        }
    }

    // 남은 시간 = 제한 시간 - 경과 시간
    private void UpdateRemainingTime()
    {
        int limitMinutes = PlayerBudget.Instance.TimeLimitSeconds / 60;
        int remaining = Mathf.Max(0, limitMinutes - PlayerBudget.Instance.ElapsedMinutes);

        remainingTimeText.text = $"남은 시간: {remaining}분";

        // 남은 시간 20% 미만 시 빨간색 경고
        remainingTimeText.color = remaining <= limitMinutes * 0.2f ? Color.red : Color.white;
    }

    // 현재 자금
    private void UpdateCurrentBudget()
    {
        int budget = PlayerBudget.Instance.RemainingBudget;

        currentBudgetText.text = $"현재 자금: {budget}원";

        // 자금 부족 시 빨간색
        currentBudgetText.color = budget < 0 ? Color.red : Color.white;
    }

    // 외부에서 HUD 즉시 갱신 시 호출 (Consume 직후 등)
    public void RefreshHUD()
    {
        UpdateRemainingTime();
        UpdateCurrentBudget();
    }
}