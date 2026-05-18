// ============================================================
// SelectionCardUI.cs
// 역할: 이동수단 선택 카드
//       3가지 상태 — 기본 / 선택 / 비활성화
//       상태 변화는 스프라이트 교체로 처리
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
    public Image cardImage; // 카드 배경 이미지

    [Header("카드 상태 스프라이트")]
    public Sprite defaultSprite;  // 기본 상태
    public Sprite selectedSprite; // 선택 상태
    public Sprite disabledSprite; // 비활성화 상태

    [Header("이동수단 설정 연결")]
    public TransportSettings transportSettings;

    public TransportType TransportType { get; private set; }

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

        SetDefault();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // 기본 상태
    public void SetDefault()
    {
        cardImage.sprite = defaultSprite;
        cardButton.interactable = true;
    }

    // 선택 상태
    public void SetSelected()
    {
        cardImage.sprite = selectedSprite;
        cardButton.interactable = true;
    }

    // 비활성화 상태 (다른 카드가 선택됐을 때)
    public void SetDisabled()
    {
        cardImage.sprite = disabledSprite;
        cardButton.interactable = false;
    }

    private void OnCardClicked()
    {
        if (GameState.CurrentPhase == Phase.Execution)
        {
            MessageSystem.E("이동 중에는 조작할 수 없습니다!");
            return;
        }
        if (GameState.CurrentPhase == Phase.Result)
        {
            MessageSystem.E("결과 확인 후 진행해주세요!");
            return;
        }
        if (!PlanningManager.Instance.HasPendingRoute)
        {
            MessageSystem.E("먼저 이동할 노드를 선택해주세요!");
            return;
        }

        PlanningUI.Instance.OnCardSelected(this);
    }
}