// ============================================================
// SettingsPanel.cs
// 역할: 설정 패널 UI — ◀▶ 버튼으로 BGM/SFX 볼륨을 10%씩 조절.
//       PlayerPrefs에 즉시 저장. SoundManager/MusicManager는
//       재생 시점마다 이 값을 직접 읽어가므로 별도로 알릴 필요가 없다.
//
//       메인 메뉴에서 열든 스테이지 플레이 중 ESC로 열든
//       항상 같은 인스턴스 하나만 존재 — DontDestroyOnLoad 싱글톤.
//       (MusicManager와 동일한 이유로 루트 분리 후 유지:
//        프리팹의 자식인 채로는 DontDestroyOnLoad가 무시됨)
//
// 부착: 메인 메뉴 씬의 최상위 SettingsPanel 오브젝트에 한 번만 배치.
//       이후 씬이 바뀌어도 파괴되지 않고 계속 유지된다.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    public static SettingsPanel Instance { get; private set; }

    [Header("패널 / 닫기 버튼(X)")]
    public GameObject panel;   // 기본 비활성화
    public Button closeButton;

    [Header("볼륨 조절 버튼 (◀ ▶)")]
    public Button bgmDownButton;
    public Button bgmUpButton;
    public Button sfxDownButton;
    public Button sfxUpButton;

    [Header("볼륨 표시 텍스트")]
    public TextMeshProUGUI bgmValueText; // 예: "BGM: 50%"
    public TextMeshProUGUI sfxValueText; // 예: "효과음: 50%"

    private const string BGM_KEY = "BGMVolume";
    private const string SFX_KEY = "SFXVolume";
    private const float Step = 0.1f;

    // 다른 스크립트(노드/카드 클릭 차단 등)에서 참조
    public static bool IsOpen =>
        Instance != null && Instance.panel != null && Instance.panel.activeSelf;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // MusicManager와 동일한 이유로 루트로 분리한 뒤 유지시킴
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (bgmDownButton != null) bgmDownButton.onClick.AddListener(() => AdjustBGM(-Step));
        if (bgmUpButton != null) bgmUpButton.onClick.AddListener(() => AdjustBGM(+Step));
        if (sfxDownButton != null) sfxDownButton.onClick.AddListener(() => AdjustSFX(-Step));
        if (sfxUpButton != null) sfxUpButton.onClick.AddListener(() => AdjustSFX(+Step));

        RefreshText();

        if (panel != null) panel.SetActive(false);
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
        if (panel != null) panel.SetActive(true);
    }

    public void Close()
    {
        PlayClick();
        if (panel != null) panel.SetActive(false);
    }

    private void AdjustBGM(float delta)
    {
        float value = Mathf.Clamp01(PlayerPrefs.GetFloat(BGM_KEY, 1f) + delta);
        PlayerPrefs.SetFloat(BGM_KEY, value);
        PlayerPrefs.Save();
        RefreshText();
        PlayClick(); // 새로 반영된 볼륨으로 재생되어 바뀐 소리 크기를 바로 확인 가능
    }

    private void AdjustSFX(float delta)
    {
        float value = Mathf.Clamp01(PlayerPrefs.GetFloat(SFX_KEY, 1f) + delta);
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();
        RefreshText();
        PlayClick();
    }

    private void RefreshText()
    {
        if (bgmValueText != null)
            bgmValueText.text = $"BGM: {Mathf.RoundToInt(GetBGMVolume() * 100)}%";
        if (sfxValueText != null)
            sfxValueText.text = $"효과음: {Mathf.RoundToInt(GetSFXVolume() * 100)}%";
    }

    private void PlayClick()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundManager.Sfx.Click);
    }

    // 외부(MusicManager, SoundManager)에서 읽어가는 저장값
    public static float GetBGMVolume() => PlayerPrefs.GetFloat(BGM_KEY, 1f);
    public static float GetSFXVolume() => PlayerPrefs.GetFloat(SFX_KEY, 1f);
}