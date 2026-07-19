// ============================================================
// EndingUI.cs
// 역할: 스테이지 1~4를 모두 클리어한 후 표시되는 엔딩 화면.
//       ResultUI.OnNextStageClicked()에서 마지막 스테이지 클리어를
//       감지하면 호출된다.
//
//       누적 점수(GameState.TotalScore)를 기준으로 별 3개(노랑/회색)와
//       코멘트를 표시한다. "스테이지_4_UI_흐름.pdf" 하단 표 기준:
//         5500점 이상        → 완벽한 등교
//         4400 ~ 5499점      → 무난한 등교
//         4399점 이하        → 간신히 등교
//       (별 개수는 PDF에 명시가 없어 3단계에 맞춰 3/2/1개로 채움 — 가정)
//
// 부착: Stage4 씬의 EndingUI 오브젝트에 부착 (기본 비활성화)
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndingUI : MonoBehaviour
{
    public static EndingUI Instance { get; private set; }

    [Header("패널")]
    public GameObject panel; // 기본 비활성화

    [Header("텍스트")]
    public TextMeshProUGUI commentText;    // 예: "완벽한 등교"
    public TextMeshProUGUI totalScoreText; // 예: "총점: 5500점"

    [Header("별 이미지 (왼쪽부터 순서대로 3개)")]
    public Image[] starImages;   // Inspector에서 크기 3으로 설정
    public Sprite starYellowSprite;
    public Sprite starGraySprite;

    [Header("나가기 버튼")]
    public Button exitButton; // 클릭 시 메인 메뉴로 복귀

    // 점수 구간 기준값 (스테이지_4_UI_흐름.pdf 참고)
    private const int PerfectThreshold = 5500; // 이상 → 완벽한 등교, 별 3개
    private const int DecentThreshold = 4400;  // 이상 → 무난한 등교, 별 2개
    // 그 미만 → 간신히 등교, 별 1개

    private const string MainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);

        if (panel != null) panel.SetActive(false);
    }

    // ResultUI.OnNextStageClicked()에서 마지막 스테이지 클리어 시 호출
    public void Show(int totalScore)
    {
        if (panel == null) return;

        int filledStars;
        string comment;

        if (totalScore >= PerfectThreshold)
        {
            filledStars = 3;
            comment = "완벽한 등교";
        }
        else if (totalScore >= DecentThreshold)
        {
            filledStars = 2;
            comment = "무난한 등교";
        }
        else
        {
            filledStars = 1;
            comment = "간신히 등교";
        }

        if (commentText != null) commentText.text = comment;
        if (totalScoreText != null) totalScoreText.text = $"총점: {totalScore}점";

        ApplyStars(filledStars);

        // 결과창과 동일한 관례 — 엔딩 화면에서는 배경음악을 정지
        if (MusicManager.Instance != null)
            MusicManager.Instance.StopMusic();

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
    }

    private void ApplyStars(int filledCount)
    {
        if (starImages == null) return;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            starImages[i].sprite = (i < filledCount) ? starYellowSprite : starGraySprite;
        }
    }

    // 나가기 → 메인 메뉴로 (클릭 시 메인메뉴로 이동, PDF 지침 그대로)
    private void OnExitClicked()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundManager.Sfx.Click);

        // 다음 플레이를 위해 누적 점수/스테이지 상태를 초기화
        GameState.ResetForNewGame();

        SceneManager.LoadScene(MainMenuSceneName);
    }
}