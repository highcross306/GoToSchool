// ============================================================
// CreditPanel.cs
// 역할: 크레딧 패널 — CREDIT 버튼 클릭 시 열림, X 클릭 시 닫힘.
//       메인 메뉴 씬에만 존재 (게임 플레이 중엔 필요 없어 전역화하지 않음).
// 부착: CreditPanel 오브젝트에 부착
// ============================================================

using UnityEngine;
using UnityEngine.UI;

public class CreditPanel : MonoBehaviour
{
    [Header("버튼")]
    public Button openButton;  // 메인 메뉴의 CREDIT 버튼
    public Button closeButton; // 패널 안의 X 버튼

    [Header("패널")]
    public GameObject panel;   // 기본 비활성화

    private void Awake()
    {
        if (openButton != null) openButton.onClick.AddListener(Open);
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        if (panel != null) panel.SetActive(false);
    }

    public void Open()
    {
        PlayClick();
        if (panel != null) panel.SetActive(true);
    }

    public void Close()
    {
        PlayClick();
        if (panel != null) panel.SetActive(false);
    }

    private void PlayClick()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundManager.Sfx.Click);
    }
}