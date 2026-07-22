// ============================================================
// ResultUI.cs
// 역할: 결과 팝업 UI
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultUI : MonoBehaviour
{
    public static ResultUI Instance { get; private set; }

    [Header("팝업 패널")]
    public GameObject resultPanel;
    public GameObject dimOverlay;

    [Header("성공 UI")]
    public GameObject successPanel;
    public TextMeshProUGUI remainingTimeText;
    public TextMeshProUGUI remainingBudgetText;
    public TextMeshProUGUI scoreText;

    [Header("실패 UI")]
    public GameObject failPanel;
    public TextMeshProUGUI failReasonText;

    [Header("버튼")]
    public Button retryButton;
    public Button nextStageButton;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // null 체크
        if (retryButton == null)
            Debug.LogError("[ResultUI] retryButton이 연결되지 않았습니다.");
        else
            retryButton.onClick.AddListener(OnRetryClicked);

        if (nextStageButton == null)
            Debug.LogError("[ResultUI] nextStageButton이 연결되지 않았습니다.");
        else
            nextStageButton.onClick.AddListener(OnNextStageClicked);

        if (resultPanel != null) resultPanel.SetActive(false);
        if (dimOverlay != null) dimOverlay.SetActive(false);
    }

    // 성공 팝업
    public void ShowSuccess(int score)
    {
        Debug.Log($"[ResultUI] ShowSuccess 호출 — 점수: {score}");

        int limitMinutes = PlayerBudget.Instance.TimeLimitSeconds / 60;
        int remainingMinutes = Mathf.Max(0, limitMinutes - PlayerBudget.Instance.ElapsedMinutes);

        if (remainingTimeText != null) remainingTimeText.text = $"남은 시간  {remainingMinutes}분";
        if (remainingBudgetText != null) remainingBudgetText.text = $"남은 자금  {PlayerBudget.Instance.RemainingBudget}원";
        if (scoreText != null) scoreText.text = $"점수  {score}점";

        if (successPanel != null) successPanel.SetActive(true);
        if (failPanel != null) failPanel.SetActive(false);
        if (nextStageButton != null) nextStageButton.gameObject.SetActive(true);

        PlaySfxSafe(SoundManager.Sfx.StageClear);
        ShowPopup();
    }

    // 실패 팝업
    public void ShowFail(string reason)
    {
        Debug.Log($"[ResultUI] ShowFail 호출 — 사유: {reason}");

        if (failReasonText != null) failReasonText.text = reason;
        else Debug.LogError("[ResultUI] failReasonText가 연결되지 않았습니다.");

        if (successPanel != null) successPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(true);
        else Debug.LogError("[ResultUI] failPanel이 연결되지 않았습니다.");

        if (nextStageButton != null) nextStageButton.gameObject.SetActive(false);
        if (retryButton != null) retryButton.gameObject.SetActive(true);

        PlaySfxSafe(SoundManager.Sfx.StageFail);
        ShowPopup();
    }

    private void ShowPopup()
    {
        Debug.Log("[ResultUI] ShowPopup 호출");

        if (MusicManager.Instance != null)
            MusicManager.Instance.StopMusic();

        // 강화 노드에서 자동으로 떠 있던 이벤트 설명창이 결과 팝업 뒤에
        // 남지 않도록 정리한다.
        if (EventInfoPopup.Instance != null)
            EventInfoPopup.Instance.Hide();

        if (dimOverlay != null) dimOverlay.SetActive(true);
        if (resultPanel != null) resultPanel.SetActive(true);

        // 같은 Canvas 안의 다른 UI(가속 버튼 등)보다 항상 위에 그려지도록
        // 형제 오브젝트 순서를 맨 뒤(= 가장 위에 렌더링)로 강제한다.
        // Hierarchy 순서를 나중에 바꿔도 이 문제가 재발하지 않는다.
        if (dimOverlay != null) dimOverlay.transform.SetAsLastSibling();
        if (resultPanel != null) resultPanel.transform.SetAsLastSibling();
    }

    private void HidePopup()
    {
        Debug.Log("[ResultUI] HidePopup 호출");
        if (resultPanel != null) resultPanel.SetActive(false);
        if (dimOverlay != null) dimOverlay.SetActive(false);
    }

    // 다시하기 버튼
    private void OnRetryClicked()
    {
        Debug.Log($"[ResultUI] OnRetryClicked 호출 — " +
                  $"CurrentStage: {GameState.CurrentStage} / " +
                  $"GameManager null: {GameManager.Instance == null}");

        // 매니저 상태와 무관하게 클릭 반응 소리는 항상 재생
        PlaySfxSafe(SoundManager.Sfx.Click);

        if (GameManager.Instance == null)
        {
            Debug.LogError("[ResultUI] GameManager.Instance가 null입니다. " +
                            "Bootstrap을 거치지 않고 이 씬을 직접 실행했을 가능성이 높습니다.");
            return;
        }

        HidePopup();
        GameManager.Instance.LoadStage(GameState.CurrentStage);
    }

    // 다음 스테이지 버튼
    private void OnNextStageClicked()
    {
        Debug.Log($"[ResultUI] OnNextStageClicked 호출 — CurrentStage: {GameState.CurrentStage}");

        PlaySfxSafe(SoundManager.Sfx.Click);

        if (GameManager.Instance == null)
        {
            Debug.LogError("[ResultUI] GameManager.Instance가 null입니다. " +
                            "Bootstrap을 거치지 않고 이 씬을 직접 실행했을 가능성이 높습니다.");
            return;
        }

        int next = GameState.CurrentStage + 1;

        // 하드코딩된 4 대신 GameManager에 실제 등록된 스테이지 개수를 참조.
        // 스테이지 수가 바뀌어도 여기를 따로 수정할 필요가 없다.
        // (이전엔 4로 고정돼 있어 스테이지가 3개로 줄었을 때
        //  마지막 스테이지 클리어 후 존재하지 않는 4번 스테이지를 시도 →
        //  HidePopup()만 실행되고 LoadStage(4)는 조용히 실패해
        //  버튼도 다음 씬도 없는 상태로 멈추는 버그가 있었음)
        int totalStages = GameManager.Instance.stageSceneNames.Length;
        if (next > totalStages)
        {
            Debug.Log($"[ResultUI] 모든 스테이지 클리어! 누적 점수: {GameState.TotalScore}점");

            HidePopup();

            if (EndingUI.Instance != null)
                EndingUI.Instance.Show(GameState.TotalScore);
            else
                Debug.LogError("[ResultUI] EndingUI.Instance가 null입니다. " +
                               "씬에 EndingUI 오브젝트가 배치되어 있는지 확인하세요.");
            return;
        }

        HidePopup();
        GameManager.Instance.LoadStage(next);
    }

    // SoundManager.Instance가 null이어도 게임이 죽지 않도록 방어적으로 재생
    private void PlaySfxSafe(SoundManager.Sfx id)
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning($"[ResultUI] SoundManager.Instance가 null이라 {id} 소리를 재생하지 못했습니다.");
            return;
        }
        SoundManager.Instance.Play(id);
    }
}