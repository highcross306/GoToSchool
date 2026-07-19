// ============================================================
// MainMenuManager.cs
// 역할: 메인 화면 UI 관리 — 게임 시작 / 설정 / 종료 버튼 처리
//       설정 패널은 전역 싱글톤 SettingsPanel이 담당하므로
//       여기서는 열기 요청만 전달한다.
//       메인 메뉴 진입 시 배경음악(단일 트랙 반복)을 재생한다.
//       PDF 지침: "버튼 효과음은 모두 click 효과음으로 통일"
//                 → 모든 버튼에서 SoundManager.Sfx.Click 재생.
// 부착: MainMenu 씬의 [MainMenuManager] 오브젝트에 부착
// ============================================================

using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("버튼 연결")]
    public Button startButton;    // 게임 시작
    public Button settingsButton; // 설정
    public Button exitButton;     // 종료

    private void Awake()
    {
        if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);

        // 메인 메뉴 배경음악 재생 (단일 트랙 반복)
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayMenuMusic();
        else
            Debug.LogWarning("[MainMenu] MusicManager.Instance가 null이라 배경음악을 재생하지 못했습니다.");
    }

    // 게임 시작 → 스테이지 1 씬으로 전환
    private void OnStartClicked()
    {
        PlayClick();
        Debug.Log("[MainMenu] 게임 시작");
        GameManager.Instance.LoadStage(1);
    }

    // 설정 패널 열기 (전역 SettingsPanel 사용)
    private void OnSettingsClicked()
    {
        PlayClick();

        if (SettingsPanel.Instance == null)
        {
            Debug.LogError("[MainMenu] SettingsPanel.Instance가 null입니다. " +
                           "씬에 SettingsPanel 오브젝트가 배치되어 있는지 확인하세요.");
            return;
        }
        SettingsPanel.Instance.Open();
    }

    // 게임 종료
    private void OnExitClicked()
    {
        PlayClick();
        Debug.Log("[MainMenu] 게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayClick()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundManager.Sfx.Click);
    }
}