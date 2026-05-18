// ============================================================
// ResultUI.cs
// 역할: 결과 팝업 UI
//       성공: 남은 시간/자금/점수 + 다시하기/다음 스테이지 버튼
//       실패: 실패 사유 + 다시하기 버튼만
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultUI : MonoBehaviour
{
    public static ResultUI Instance { get; private set; }

    [Header("팝업 패널")]
    public GameObject resultPanel;  // 팝업 전체
    public GameObject dimOverlay;   // 뒷배경 어두운 오버레이

    [Header("성공 UI")]
    public GameObject successPanel;         // 성공 내용 패널
    public TextMeshProUGUI remainingTimeText; // 남은 시간
    public TextMeshProUGUI remainingBudgetText; // 남은 자금
    public TextMeshProUGUI scoreText;        // 점수

    [Header("실패 UI")]
    public GameObject failPanel;            // 실패 내용 패널
    public TextMeshProUGUI failReasonText;  // 실패 사유

    [Header("버튼")]
    public Button retryButton;              // 다시하기 (성공/실패 공통)
    public Button nextStageButton;          // 다음 스테이지 (성공만)

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        retryButton.onClick.AddListener(OnRetryClicked);
        nextStageButton.onClick.AddListener(OnNextStageClicked);

        resultPanel.SetActive(false);
        if (dimOverlay != null) dimOverlay.SetActive(false);
    }

    // ResultEvaluator → 성공 시 호출
    public void ShowSuccess(int score)
    {
        // 남은 시간 계산
        int limitMinutes = PlayerBudget.Instance.TimeLimitSeconds / 60;
        int remainingMinutes = Mathf.Max(0, limitMinutes - PlayerBudget.Instance.ElapsedMinutes);

        remainingTimeText.text = $"남은 시간  {remainingMinutes}분";
        remainingBudgetText.text = $"남은 자금  {PlayerBudget.Instance.RemainingBudget}원";
        scoreText.text = $"점수  {score}점";

        successPanel.SetActive(true);
        failPanel.SetActive(false);
        nextStageButton.gameObject.SetActive(true); // 다음 스테이지 버튼 표시

        ShowPopup();
    }

    // ResultEvaluator → 실패 시 호출
    public void ShowFail(string reason)
    {
        failReasonText.text = reason;

        successPanel.SetActive(false);
        failPanel.SetActive(true);
        nextStageButton.gameObject.SetActive(false); // 다음 스테이지 버튼 숨김

        ShowPopup();
    }

    private void ShowPopup()
    {
        if (dimOverlay != null) dimOverlay.SetActive(true);
        resultPanel.SetActive(true);
    }

    private void HidePopup()
    {
        resultPanel.SetActive(false);
        if (dimOverlay != null) dimOverlay.SetActive(false);
    }

    // 다시하기 버튼
    private void OnRetryClicked()
    {
        HidePopup();
        GameManager.Instance.LoadStage(GameState.CurrentStage);
    }

    // 다음 스테이지 버튼
    private void OnNextStageClicked()
    {
        int next = GameState.CurrentStage + 1;
        if (next > 4)
        {
            Debug.Log("[ResultUI] 모든 스테이지 클리어!");
            return;
        }
        HidePopup();
        GameManager.Instance.LoadStage(next);
    }
}