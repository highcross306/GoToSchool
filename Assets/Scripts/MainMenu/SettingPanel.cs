// ============================================================
// SettingsPanel.cs
// 역할: 메인 메뉴 전용 설정 패널 — ◀▶ 버튼으로 BGM/SFX 볼륨을 10%씩 조절.
//       PlayerPrefs에 즉시 저장. SoundManager/MusicManager는
//       재생 시점마다 이 값을 직접 읽어가므로 별도로 알릴 필요가 없다.
//
//       [변경] ESC 토글 / 메인 메뉴 이동 / DontDestroyOnLoad는
//              전부 PauseMenu.cs로 분리했다.
//              이 패널은 메인 메뉴 씬 안에서만 "설정" 버튼으로 연다.
//
//       GetBGMVolume()/GetSFXVolume()/SetBGMVolume()/SetSFXVolume()은
//       static이라 PauseMenu.cs에서도 그대로 가져다 쓴다 —
//       PlayerPrefs 키 이름을 두 파일에 따로 적어 어긋나는 것을 방지.
//
// 부착: 메인 메뉴 씬의 SettingsPanel 오브젝트에 부착
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

    public static bool IsOpen =>
        Instance != null && Instance.panel != null && Instance.panel.activeSelf;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (bgmDownButton != null) bgmDownButton.onClick.AddListener(() => AdjustBGM(-Step));
        if (bgmUpButton != null) bgmUpButton.onClick.AddListener(() => AdjustBGM(+Step));
        if (sfxDownButton != null) sfxDownButton.onClick.AddListener(() => AdjustSFX(-Step));
        if (sfxUpButton != null) sfxUpButton.onClick.AddListener(() => AdjustSFX(+Step));

        RefreshText();

        if (panel != null) panel.SetActive(false);
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
        SetBGMVolume(GetBGMVolume() + delta);
        RefreshText();
        PlayClick(); // 새로 반영된 볼륨으로 재생되어 바뀐 소리 크기를 바로 확인 가능
    }

    private void AdjustSFX(float delta)
    {
        SetSFXVolume(GetSFXVolume() + delta);
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

    // 외부(MusicManager, SoundManager, PauseMenu)에서 읽고 쓰는 저장값
    public static float GetBGMVolume() => PlayerPrefs.GetFloat(BGM_KEY, 1f);
    public static float GetSFXVolume() => PlayerPrefs.GetFloat(SFX_KEY, 1f);

    public static void SetBGMVolume(float value)
    {
        PlayerPrefs.SetFloat(BGM_KEY, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }

    public static void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat(SFX_KEY, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }
}