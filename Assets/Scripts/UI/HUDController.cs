// ============================================================
// HUDController.cs
// 역할: 실행 단계에서 자금/시간을 실시간으로 화면에 표시
//       PlayerBudget 값을 매 프레임 읽어 갱신
// 부착: Canvas 하위 HUD 오브젝트에 부착
// ============================================================

using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [Header("UI 연결")]
    public TextMeshProUGUI budgetText;  // 현재 자금 표시
    public TextMeshProUGUI timeText;    // 경과 시간 표시

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        // Execution 단계에서만 갱신
        if (GameState.CurrentPhase != Phase.Execution) return;

        UpdateBudget();
        UpdateTime();
    }

    private void UpdateBudget()
    {
        int remaining = PlayerBudget.Instance.RemainingBudget;
        budgetText.text = $"자금  {remaining}원";

        // 자금 부족 시 빨간색으로 표시
        budgetText.color = remaining < 0 ? Color.red : Color.white;
    }

    private void UpdateTime()
    {
        int elapsed = PlayerBudget.Instance.ElapsedMinutes;
        int limit = PlayerBudget.Instance.TimeLimitSeconds / 60;
        timeText.text = $"시간  {elapsed} / {limit}분";

        // 제한 시간 80% 초과 시 주황색 경고
        timeText.color = elapsed >= limit * 0.8f ? new Color(1f, 0.5f, 0f) : Color.white;
    }
}