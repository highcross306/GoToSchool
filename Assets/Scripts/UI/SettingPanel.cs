// ============================================================
// SettingsPanel.cs
// 역할: 설정 패널 UI
//       BGM / SFX 볼륨 조절, PlayerPrefs로 저장
// 부착: SettingsPanel 오브젝트에 부착
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    [Header("볼륨 슬라이더")]
    public Slider bgmSlider;  // BGM 볼륨
    public Slider sfxSlider;  // SFX 볼륨

    [Header("볼륨 텍스트 (선택)")]
    public TextMeshProUGUI bgmValueText;
    public TextMeshProUGUI sfxValueText;

    [Header("닫기 버튼")]
    public Button closeButton;

    private const string BGM_KEY = "BGMVolume";
    private const string SFX_KEY = "SFXVolume";

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);

        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0f;
            bgmSlider.maxValue = 1f;
            bgmSlider.value = PlayerPrefs.GetFloat(BGM_KEY, 1f); // 저장값 불러오기
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.value = PlayerPrefs.GetFloat(SFX_KEY, 1f);
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }

        RefreshText();
    }

    private void OnBGMChanged(float value)
    {
        PlayerPrefs.SetFloat(BGM_KEY, value);
        PlayerPrefs.Save();

        // 실제 오디오 매니저 연결 시 여기에 AudioManager.Instance.SetBGMVolume(value) 추가
        Debug.Log($"[Settings] BGM 볼륨: {Mathf.RoundToInt(value * 100)}%");
        RefreshText();
    }

    private void OnSFXChanged(float value)
    {
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();

        // 실제 오디오 매니저 연결 시 여기에 AudioManager.Instance.SetSFXVolume(value) 추가
        Debug.Log($"[Settings] SFX 볼륨: {Mathf.RoundToInt(value * 100)}%");
        RefreshText();
    }

    private void RefreshText()
    {
        if (bgmValueText != null && bgmSlider != null)
            bgmValueText.text = $"{Mathf.RoundToInt(bgmSlider.value * 100)}%";
        if (sfxValueText != null && sfxSlider != null)
            sfxValueText.text = $"{Mathf.RoundToInt(sfxSlider.value * 100)}%";
    }

    private void OnCloseClicked()
    {
        GetComponentInParent<MainMenuManager>()?.CloseSettings();
    }

    // 볼륨 저장값 읽기 (AudioManager 등 외부에서 사용)
    public static float GetBGMVolume() => PlayerPrefs.GetFloat(BGM_KEY, 1f);
    public static float GetSFXVolume() => PlayerPrefs.GetFloat(SFX_KEY, 1f);
}