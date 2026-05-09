// ============================================================
// SelectionCardUI.cs
// 역할: 이동수단 선택지 카드 하나를 표현
//       클릭 시 강조 표시만 (확정은 결정 버튼에서)
//       다른 카드 클릭 시 이전 선택 자동 해제
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionCardUI : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI transportNameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI timeText;
    public Button cardButton;
    public Image cardBackground; // 카드 배경 이미지 (테두리 강조용)

    [Header("이동수단 설정 연결")]
    public TransportSettings transportSettings;

    [Header("선택 상태 색상")]
    public Color normalColor;   // 기본 상태 색상
    public Color selectedColor; // 선택 상태 색상

    public TransportType TransportType { get; private set; }
    public bool IsSelected { get; private set; } = false;

    private void Awake()
    {
        cardButton.onClick.AddListener(OnCardClicked);
    }

    // PlanningUI가 카드 활성화 시 호출
    public void Setup(TransportType type)
    {
        TransportType = type;

        TransportSetting setting = transportSettings.Get(type);

        transportNameText.text = type switch
        {
            TransportType.Walk => "도보",
            TransportType.Bus => "버스",
            TransportType.Taxi => "택시",
            _ => type.ToString()
        };

        if (setting != null)
        {
            costText.text = setting.cost == 0 ? "-0원" : $"-{setting.cost}원";
            timeText.text = $"-{setting.timeMinutes}분";
        }

        SetSelected(false);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        SetSelected(false);
        gameObject.SetActive(false);
    }

    // 선택 상태 설정 (PlanningUI에서 호출)
    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        // 테두리 색상 변경
        if (cardBackground != null)
            cardBackground.color = selected ? selectedColor : normalColor;

        // 1.05배 확대 / 원래 크기
        transform.localScale = selected ? Vector3.one * 1.05f : Vector3.one;
    }

    // 카드 클릭 시 → PlanningUI에 알림 (확정 아님)
    private void OnCardClicked()
    {
        PlanningUI.Instance.OnCardSelected(this);
    }
}