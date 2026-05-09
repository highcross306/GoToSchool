// ============================================================
// PlanningUI.cs
// 역할: 선택 단계 전체 UI 컨트롤러
//       카드 선택/해제 관리
//       결정 버튼 클릭 시 선택된 이동수단을 PlanningManager에 전달
// ============================================================

using UnityEngine;
using UnityEngine.UI;

public class PlanningUI : MonoBehaviour
{
    public static PlanningUI Instance { get; private set; }

    [Header("UI 연결")]
    public SelectionCardUI[] selectionCards;
    public Button decideButton;
    public GameObject selectionPanel;

    private SelectionCardUI selectedCard = null;  // 현재 선택된 카드
    private bool isDeciding = false; // 결정 버튼 연타 방지

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        decideButton.onClick.AddListener(OnDecideButtonClicked);
        SetDecideButtonActive(false);
        HideSelectionCards();
    }

    // PlanningManager → 노드 클릭 후 경로 확정 시 호출
    public void ShowSelectionCards(RouteData route)
    {
        HideSelectionCards();
        selectionPanel.SetActive(true);
        isDeciding = false;

        foreach (TransportType type in route.allowedTransports)
        {
            SelectionCardUI card = FindCardByType(type);
            if (card != null) card.Setup(type);
        }
    }

    // 카드 패널 전체 숨기기
    public void HideSelectionCards()
    {
        selectionPanel.SetActive(false);
        selectedCard = null;
        foreach (SelectionCardUI card in selectionCards)
            card.Hide();
        SetDecideButtonActive(false);
    }

    // SelectionCardUI 클릭 시 호출
    public void OnCardSelected(SelectionCardUI clickedCard)
    {
        // 이전 선택 해제
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        // 같은 카드 다시 클릭 시 선택 해제
        if (selectedCard == clickedCard)
        {
            selectedCard = null;
            SetDecideButtonActive(false);
            return;
        }

        // 새 카드 선택
        selectedCard = clickedCard;
        selectedCard.SetSelected(true);
        SetDecideButtonActive(true);
    }

    // 결정 버튼 활성/비활성
    public void SetDecideButtonActive(bool active)
    {
        decideButton.interactable = active;
    }

    // 결정 버튼 클릭 시
    // → 선택된 이동수단을 PlanningManager에 전달 후 실행 단계로 전환
    private void OnDecideButtonClicked()
    {
        // 연타 방지
        if (isDeciding) return;
        if (selectedCard == null) return;

        isDeciding = true;
        decideButton.interactable = false;

        // 이동수단 확정 → PlanningManager
        PlanningManager.Instance.OnTransportSelected(selectedCard.TransportType);

        // 선택 완료 시 실행 단계로 전환
        PlanningManager.Instance.OnDecideButtonClicked();
    }

    private SelectionCardUI FindCardByType(TransportType type)
    {
        foreach (SelectionCardUI card in selectionCards)
        {
            if (card.name.Contains(type.ToString())) return card;
        }
        return null;
    }
}