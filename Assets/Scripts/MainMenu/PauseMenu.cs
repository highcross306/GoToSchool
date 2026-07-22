// ============================================================
// PauseMenu.cs
// 역할: 게임 플레이 중 ESC로 여는 일시정지 창.
//       - ◀▶ 버튼으로 BGM/SFX 볼륨 조절 (SettingsPanel과 같은 저장값 공유)
//       - "메인 메뉴로" 버튼으로 현재 게임을 중단하고 메인 메뉴 씬으로 복귀
//
//       ★ 열려 있는 동안:
//         1) 자체 Canvas의 sortingOrder를 매우 높게 올려 모든 UI 위에 표시
//         2) 전체 화면을 덮는 투명 블로커로 뒤쪽 클릭을 전부 차단
//            (노드/카드/결정 버튼뿐 아니라 다른 UI 버튼까지 모두 막힘)
//
//       게임 내내 유지되어야 하므로 DontDestroyOnLoad 싱글톤.
//
// 부착: Resources/Bootstrap/PauseMenu.prefab 루트
//       (루트에 Canvas + CanvasScaler + GraphicRaycaster + 이 스크립트)
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [Header("패널 / 닫기 버튼(X)")]
    public GameObject panel;   // 껐다 켤 대상 (Canvas 자신이 아니라 그 안의 Panel)
    public Button closeButton;

    [Header("볼륨 조절 버튼 (◀ ▶)")]
    public Button bgmDownButton;
    public Button bgmUpButton;
    public Button sfxDownButton;
    public Button sfxUpButton;

    [Header("볼륨 표시 텍스트")]
    public TextMeshProUGUI bgmValueText;
    public TextMeshProUGUI sfxValueText;

    [Header("메인 메뉴로 돌아가기")]
    public Button mainMenuButton;

    [Header("최상단 표시 설정")]
    [Tooltip("이 패널 Canvas의 sortingOrder. 다른 모든 UI보다 크게 둔다.")]
    public int sortingOrder = 999;

    private const float Step = 0.1f;
    private const string MainMenuSceneName = "MainMenu";

    private Canvas selfCanvas;
    private GameObject blocker; // 전체 화면 클릭 차단용 (런타임 자동 생성)

    public static bool IsOpen =>
        Instance != null && Instance.panel != null && Instance.panel.activeSelf;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        // 자체 Canvas를 최상단으로 — 다른 어떤 Canvas보다 위에 그려지게 한다.
        selfCanvas = GetComponent<Canvas>();
        if (selfCanvas != null)
        {
            selfCanvas.overrideSorting = true;
            selfCanvas.sortingOrder = sortingOrder;
        }
        else
        {
            Debug.LogError("[PauseMenu] 루트에 Canvas가 없습니다. " +
                           "PauseMenu 오브젝트에 Canvas 컴포넌트가 있어야 최상단 표시가 됩니다.", this);
        }

        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (bgmDownButton != null) bgmDownButton.onClick.AddListener(() => AdjustBGM(-Step));
        if (bgmUpButton != null) bgmUpButton.onClick.AddListener(() => AdjustBGM(+Step));
        if (sfxDownButton != null) sfxDownButton.onClick.AddListener(() => AdjustSFX(-Step));
        if (sfxUpButton != null) sfxUpButton.onClick.AddListener(() => AdjustSFX(+Step));
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        CreateBlocker();
        RefreshText();

        if (panel != null) panel.SetActive(false);
        if (blocker != null) blocker.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Toggle();
    }

    public void Toggle()
    {
        if (IsOpen) Close(); else Open();
    }

    public void Open()
    {
        if (panel == null) return;

        // 블로커를 패널보다 먼저(아래에) 두고, 패널을 그 위에 둔다.
        // → 블로커는 패널 바깥의 모든 클릭을 흡수하고, 패널 안 버튼은 정상 작동.
        if (blocker != null)
        {
            blocker.SetActive(true);
            blocker.transform.SetAsLastSibling();
        }
        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
    }

    public void Close()
    {
        PlayClick();
        if (panel != null) panel.SetActive(false);
        if (blocker != null) blocker.SetActive(false);
    }

    private void OnMainMenuClicked()
    {
        PlayClick();
        if (panel != null) panel.SetActive(false);
        if (blocker != null) blocker.SetActive(false);

        // GameManager를 거쳐야 GameState.CurrentPhase가 Planning으로 되돌아간다.
        // 직접 LoadScene을 부르면 Phase가 Execution/Result인 채로 메인 메뉴에
        // 진입해, 남아 있는 HUD 등이 잘못된 단계로 동작할 수 있다.
        if (GameManager.Instance != null)
            GameManager.Instance.LoadMainMenu();
        else
            SceneManager.LoadScene(MainMenuSceneName);
    }

    // 전체 화면을 덮는 투명 이미지. raycastTarget=true라 뒤쪽 UI로 가는
    // 클릭을 전부 가로챈다. 색 알파를 0으로 둬 보이지는 않는다.
    private void CreateBlocker()
    {
        blocker = new GameObject("ClickBlocker");
        blocker.transform.SetParent(transform, false);

        RectTransform rt = blocker.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = blocker.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f); // 완전 투명
        img.raycastTarget = true;              // 클릭은 흡수
    }

    private void AdjustBGM(float delta)
    {
        SettingsPanel.SetBGMVolume(SettingsPanel.GetBGMVolume() + delta);
        RefreshText();
        PlayClick();
    }

    private void AdjustSFX(float delta)
    {
        SettingsPanel.SetSFXVolume(SettingsPanel.GetSFXVolume() + delta);
        RefreshText();
        PlayClick();
    }

    private void RefreshText()
    {
        if (bgmValueText != null)
            bgmValueText.text = $"BGM: {Mathf.RoundToInt(SettingsPanel.GetBGMVolume() * 100)}%";
        if (sfxValueText != null)
            sfxValueText.text = $"효과음: {Mathf.RoundToInt(SettingsPanel.GetSFXVolume() * 100)}%";
    }

    private void PlayClick()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundManager.Sfx.Click);
    }
}