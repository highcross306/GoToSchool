// ============================================================
// PauseMenu.cs
// 역할: 게임 플레이 중 ESC로 여는 일시정지 창.
//       - ◀▶ 버튼으로 BGM/SFX 볼륨 조절 (SettingsPanel과 같은 저장값 공유)
//       - "메인 메뉴로" 버튼으로 현재 게임을 중단하고 메인 메뉴 씬으로 복귀
//
//       게임 내내(스테이지가 몇 번 바뀌어도) 유지되어야 하므로
//       DontDestroyOnLoad 싱글톤으로 만든다.
//
//       ★ 진단용 Debug.Log가 포함된 버전 ★
//       문제 원인을 찾은 뒤에는 // [진단] 표시된 로그를 지워도 된다.
//
// 부착: Resources/Bootstrap/PauseMenu.prefab 루트 (자체 Canvas 포함)
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [Header("패널 / 닫기 버튼(X)")]
    public GameObject panel;   // 기본 비활성화
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

    private const float Step = 0.1f;
    private const string MainMenuSceneName = "MainMenu";

    public static bool IsOpen =>
        Instance != null && Instance.panel != null && Instance.panel.activeSelf;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (bgmDownButton != null) bgmDownButton.onClick.AddListener(() => AdjustBGM(-Step));
        if (bgmUpButton != null) bgmUpButton.onClick.AddListener(() => AdjustBGM(+Step));
        if (sfxDownButton != null) sfxDownButton.onClick.AddListener(() => AdjustSFX(-Step));
        if (sfxUpButton != null) sfxUpButton.onClick.AddListener(() => AdjustSFX(+Step));
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        RefreshText();

        // [진단] panel 참조가 연결돼 있는지 확인
        if (panel == null)
            Debug.LogError("[PauseMenu] ★panel 필드가 비어 있습니다★ " +
                           "프리팹 Inspector에서 panel 슬롯에 일시정지 패널 오브젝트를 연결하세요.", this);
        else
            Debug.Log($"[PauseMenu] Awake 완료 — panel 연결됨: '{panel.name}', " +
                      $"현재 활성화={panel.activeSelf}", this);

        if (panel != null) panel.SetActive(false);
    }

    private float diagTimer;

    private void Update()
    {
        // [진단] Update가 실제로 매 프레임 도는지 1초마다 한 번씩 확인
        diagTimer += Time.unscaledDeltaTime;
        if (diagTimer >= 1f)
        {
            diagTimer = 0f;
            Debug.Log($"[PauseMenu] Update 살아있음 (Time.timeScale={Time.timeScale})");
        }

        // Escape / P 둘 다 받아본다 (혹시 ESC가 에디터에 먹히는 경우 대비)
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[PauseMenu] ESC(또는 P) 감지됨 → Toggle 호출");
            Toggle();
        }
    }

    public void Toggle()
    {
        // [진단] Toggle 진입 및 panel 상태
        Debug.Log($"[PauseMenu] Toggle — IsOpen={IsOpen}, panel null? {panel == null}");
        if (IsOpen) Close(); else Open();
    }

    public void Open()
    {
        if (panel == null)
        {
            Debug.LogError("[PauseMenu] Open 호출됐지만 panel이 null이라 열 수 없습니다.", this);
            return;
        }
        panel.SetActive(true);
        panel.transform.SetAsLastSibling(); // 다른 UI보다 위에 그려지도록

        // [진단] 실제로 켜졌는지 + Canvas 상태 확인
        Canvas c = GetComponentInChildren<Canvas>(true);
        Debug.Log($"[PauseMenu] Open 완료 — panel.activeSelf={panel.activeSelf}, " +
                  $"activeInHierarchy={panel.activeInHierarchy}, " +
                  $"Canvas={(c != null ? c.name + " enabled=" + c.enabled : "없음★")}", this);

        // [진단] EventSystem 존재 확인 (버튼 클릭에 필요)
        if (EventSystem.current == null)
            Debug.LogWarning("[PauseMenu] 씬에 EventSystem이 없습니다. 버튼 클릭이 동작하지 않을 수 있습니다.");
    }

    public void Close()
    {
        PlayClick();
        if (panel != null) panel.SetActive(false);
    }

    private void OnMainMenuClicked()
    {
        PlayClick();
        if (panel != null) panel.SetActive(false);
        SceneManager.LoadScene(MainMenuSceneName);
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