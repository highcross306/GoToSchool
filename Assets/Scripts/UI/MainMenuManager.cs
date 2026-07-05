// ============================================================
// MainMenuManager.cs
// 역할: 메인 화면 UI 관리
//       게임 시작 / 설정 / 종료 버튼 처리
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

    [Header("설정 패널")]
    public GameObject settingsPanel; // 설정 패널 (기본 비활성화)

    private void Awake()
    {
        if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);

        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // 게임 시작 → 스테이지 1 씬으로 전환
    private void OnStartClicked()
    {
        Debug.Log("[MainMenu] 게임 시작");
        GameManager.Instance.LoadStage(1);
    }

    // 설정 패널 열기/닫기
    private void OnSettingsClicked()
    {
        if (settingsPanel == null) return;
        bool isOpen = settingsPanel.activeSelf;
        settingsPanel.SetActive(!isOpen);
        Debug.Log($"[MainMenu] 설정 패널 {(isOpen ? "닫힘" : "열림")}");
    }

    // 설정 패널 닫기 (설정 패널 내 닫기 버튼에서 호출)
    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // 게임 종료
    private void OnExitClicked()
    {
        Debug.Log("[MainMenu] 게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}