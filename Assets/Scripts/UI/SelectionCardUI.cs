// ============================================================
// SelectionCardUI.cs
// 역할: 이동수단 선택 카드
//       노드 미선택 상태에서 클릭 시 에러 메시지
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
    public Image cardBackground;

    [Header("이동수단 설정 연결")]
    public TransportSettings transportSettings;

    [Header("선택 상태 색상")]
    public Color normalColor;
    public Color selectedColor;

    public TransportType TransportType { get; private set; }
    public bool IsSelected { get; private set; } = false;

    private void Awake()
    {
        cardButton.onClick.AddListener(OnCardClicked);
    }

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

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (cardBackground != null)
            cardBackground.color = selected ? selectedColor : normalColor;
        transform.localScale = selected ? Vector3.one * 1.05f : Vector3.one;
    }

    private void OnCardClicked()
    {
        // 이동 중
        if (GameState.CurrentPhase == Phase.Execution)
        {
            MessageSystem.E("이동 중에는 조작할 수 없습니다!");
            return;
        }

        // 결과 팝업
        if (GameState.CurrentPhase == Phase.Result)
        {
            MessageSystem.E("결과 확인 후 진행해주세요!");
            return;
        }

        // 노드 미선택 상태
        if (!PlanningManager.Instance.HasPendingRoute)
        {
            MessageSystem.E("먼저 이동할 노드를 선택해주세요!");
            return;
        }

        PlanningUI.Instance.OnCardSelected(this);
    }
}