// ============================================================
// SelectionCardUI.cs
// 역할: 이동수단 선택지 카드 하나를 표현
//       TransportSettings에서 비용/시간을 조회해 표시
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

    [Header("이동수단 설정 연결")]
    public TransportSettings transportSettings; // 인스펙터에서 TransportSettings 에셋 연결

    private TransportType transportType;

    private void Awake()
    {
        cardButton.onClick.AddListener(OnCardClicked);
    }

    // PlanningUI가 카드 활성화 시 호출
    // 허용된 이동수단 타입만 받아 TransportSettings에서 비용/시간 조회
    public void Setup(TransportType type)
    {
        transportType = type;

        TransportSetting setting = transportSettings.Get(type);

        transportNameText.text = type switch
        {
            TransportType.Walk => "걷기",
            TransportType.Bus => "버스",
            TransportType.Taxi => "택시",
            _ => type.ToString()
        };

        if (setting != null)
        {
            costText.text = setting.cost == 0 ? "무료" : $"{setting.cost}원";
            timeText.text = $"{setting.timeMinutes}분";
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnCardClicked()
    {
        PlanningManager.Instance.OnTransportSelected(transportType);
        PlanningUI.Instance.HideSelectionCards();
    }
}