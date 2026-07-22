// ============================================================
// EventMessagePanel.cs
// 역할: 이벤트 발동 시 메시지 표시
//       패널 클릭 시 메시지 사라지고 이동 시작
// 부착: Canvas 하위 EventMessagePanel 오브젝트에 부착
// ============================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventMessagePanel : MonoBehaviour
{
    public static EventMessagePanel Instance { get; private set; }

    [Header("UI 연결")]
    public GameObject panel;       // 패널 전체
    public TextMeshProUGUI messageText; // 이벤트 메시지 텍스트
    public Button panelButton; // 패널 클릭 버튼 (패널 전체에 Button 컴포넌트)

    private Action onDismiss; // 패널 닫힌 후 실행할 콜백 (이동 시작)

    // 이벤트 메시지 패널이 열려 있는지 (열려 있으면 다른 입력 차단)
    public static bool IsOpen =>
        Instance != null && Instance.panel != null && Instance.panel.activeSelf;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (panelButton != null)
            panelButton.onClick.AddListener(OnPanelClicked);
        else
            Debug.LogError("[EventMessagePanel] panelButton이 연결되지 않아 " +
                           "메시지를 클릭해 닫을 수 없습니다.", this);

        if (panel != null) panel.SetActive(false);
    }

    // 이벤트 발동 시 호출 — 메시지 표시 후 클릭 대기
    public void Show(string message, Action onDismissCallback)
    {
        onDismiss = onDismissCallback;

        // null 체크를 대입보다 먼저 한다.
        // (이전에는 messageText.text에 먼저 접근한 뒤에 null을 검사해서,
        //  슬롯이 비어 있으면 검사에 닿기도 전에 예외가 났다)
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }
        else
        {
            // 패널이 없으면 메시지를 띄울 수 없으므로,
            // 콜백(이동 시작)이 영원히 실행되지 않아 게임이 멈춘다. 즉시 진행시킨다.
            Debug.LogError("[EventMessagePanel] panel이 연결되지 않아 " +
                           "메시지를 건너뛰고 진행합니다.", this);
            Action cb = onDismiss;
            onDismiss = null;
            cb?.Invoke();
        }
    }

    // 패널 클릭 시 → 패널 + 텍스트 모두 숨기고 이동 시작
    private void OnPanelClicked()
    {
        // 일시정지 창이 위에 떠 있으면 그 클릭으로 메시지가 닫히지 않도록 한다.
        if (PauseMenu.IsOpen) return;

        if (panel != null) panel.SetActive(false);
        if (messageText != null) messageText.gameObject.SetActive(false);
        Action callback = onDismiss;
        onDismiss = null;
        callback?.Invoke();
    }
}