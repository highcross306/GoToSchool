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

        panelButton.onClick.AddListener(OnPanelClicked);
        panel.SetActive(false);
    }

    // 이벤트 발동 시 호출 — 메시지 표시 후 클릭 대기
    public void Show(string message, Action onDismissCallback)
    {
        onDismiss = onDismissCallback;
        messageText.text = message;
        if (messageText != null) messageText.gameObject.SetActive(true);
        panel.SetActive(true);
    }

    // 패널 클릭 시 → 패널 + 텍스트 모두 숨기고 이동 시작
    private void OnPanelClicked()
    {
        panel.SetActive(false);
        if (messageText != null) messageText.gameObject.SetActive(false);
        Action callback = onDismiss;
        onDismiss = null;
        callback?.Invoke();
    }
}