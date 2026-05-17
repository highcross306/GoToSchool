// ============================================================
// MessageSystem.cs
// 역할: E(에러) 메시지 화면 중앙 표시 + L(로그) 콘솔 출력
//       E: 빨간 볼드체, 3초 후 자동 사라짐
//       L: Debug.Log로만 출력
// 부착: Canvas 하위 MessagePanel 오브젝트에 부착
// ============================================================

using System.Collections;
using UnityEngine;
using TMPro;

public class MessageSystem : MonoBehaviour
{
    public static MessageSystem Instance { get; private set; }

    [Header("UI 연결")]
    public TextMeshProUGUI messageText; // 화면 중앙 텍스트
    public GameObject messagePanel; // 메시지 패널 (비활성 상태로 시작)

    [Header("설정")]
    public float displayDuration = 3f; // 표시 시간 (초)

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        messagePanel.SetActive(false);
    }

    // E: 플레이어에게 노출되는 에러 메시지
    public static void E(string message)
    {
        if (Instance == null) return;
        Instance.ShowError(message);
    }

    // L: 로그에만 노출되는 상태 메시지
    public static void L(string message)
    {
        Debug.Log($"[상태] {message}");
    }

    private void ShowError(string message)
    {
        // 이전 코루틴 취소 후 새로 시작
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        messageText.text = message;
        messagePanel.SetActive(true);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        messagePanel.SetActive(false);
    }
}