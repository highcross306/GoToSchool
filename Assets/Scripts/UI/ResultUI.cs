// ============================================================
// ResultUI.cs
// 역할: 결과 화면 UI
//       성공/실패 표시, 최종 점수, 남은 자금/시간 표시
//       재시도 / 다음 스테이지 버튼 처리
// 부착: Canvas 하위 ResultPanel 오브젝트에 부착
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultUI : MonoBehaviour
{
    public static ResultUI Instance { get; private set; }

    [Header("결과 텍스트")]
    public TextMeshProUGUI resultTitleText;  // 성공 / 실패
    public TextMeshProUGUI scoreText;        // 최종 점수
    public TextMeshProUGUI budgetText;       // 남은 자금
    public TextMeshProUGUI timeText;         // 경과 시간
    public TextMeshProUGUI failReasonText;   // 실패 이유 (실패 시만 표시)

    [Header("버튼")]
    public Button retryButton;        // 재시도
    public Button nextStageButton;    // 다음 스테이지 (성공 시만 활성화)

    [Header("패널")]
    public GameObject resultPanel;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        retryButton.onClick.AddListener(OnRetryClicked);
        nextStageButton.onClick.AddListener(OnNextStageClicked);
        resultPanel.SetActive(false);
    }

    // ResultEvaluator → 성공 시 호출
    public void ShowSuccess(int score)
    {
        resultPanel.SetActive(true);
        failReasonText.gameObject.SetActive(false);
        nextStageButton.interactable = true;

        resultTitleText.text = "성공!";
        resultTitleText.color = Color.green;
        scoreText.text = $"점수  {score}점";
        budgetText.text = $"남은 자금  {PlayerBudget.Instance.RemainingBudget}원";
        timeText.text = $"소요 시간  {PlayerBudget.Instance.ElapsedMinutes}분";
    }

    // ResultEvaluator → 실패 시 호출
    public void ShowFail(string reason)
    {
        resultPanel.SetActive(true);
        failReasonText.gameObject.SetActive(true);
        nextStageButton.interactable = false;

        resultTitleText.text = "실패";
        resultTitleText.color = Color.red;
        scoreText.text = "";
        failReasonText.text = reason;
        budgetText.text = $"남은 자금  {PlayerBudget.Instance.RemainingBudget}원";
        timeText.text = $"소요 시간  {PlayerBudget.Instance.ElapsedMinutes}분";
    }

    // 재시도 버튼
    private void OnRetryClicked()
    {
        resultPanel.SetActive(false);
        GameManager.Instance.LoadStage(GameState.CurrentStage);
    }

    // 다음 스테이지 버튼
    private void OnNextStageClicked()
    {
        int next = GameState.CurrentStage + 1;
        if (next > 4)
        {
            Debug.Log("[ResultUI] 모든 스테이지 클리어!");
            // 추후 엔딩 씬 또는 타이틀로 전환
            return;
        }
        resultPanel.SetActive(false);
        GameManager.Instance.LoadStage(next);
    }
}