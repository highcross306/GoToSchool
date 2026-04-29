// ============================================================
// SelectionCardUI.cs
// 역할: 이동수단 선택지 카드 하나를 표현
//       노드 클릭 시 PlanningManager가 카드를 활성화
//       카드 클릭 시 PlanningManager.OnTransportSelected() 호출
// 부착: SelectionPanel 하위 카드 오브젝트마다 부착
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionCardUI : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI transportNameText; // 이동수단 이름
    public TextMeshProUGUI costText;          // 비용
    public TextMeshProUGUI timeText;          // 소요 시간
    public Button cardButton;        // 카드 버튼

    private TransportType transportType;
    private TransportCost transportCost;

    private void Awake()
    {
        cardButton.onClick.AddListener(OnCardClicked);
    }

    // PlanningUI가 카드 활성화 시 호출
    public void Setup(TransportCost cost)
    {
        transportCost = cost;
        transportType = cost.transportType;

        transportNameText.text = cost.transportType switch
        {
            TransportType.Walk => "걷기",
            TransportType.Bus => "버스",
            TransportType.Taxi => "택시",
            _ => cost.transportType.ToString()
        };

        costText.text = cost.cost == 0 ? "무료" : $"{cost.cost}원";
        timeText.text = $"{cost.timeMinutes}분";

        gameObject.SetActive(true);
    }

    // 카드 비활성화
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // 카드 클릭 시
    private void OnCardClicked()
    {
        PlanningManager.Instance.OnTransportSelected(transportType);
        PlanningUI.Instance.HideSelectionCards();
    }
}