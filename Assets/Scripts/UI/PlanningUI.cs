// ============================================================
// PlanningUI.cs
// 역할: 선택 단계 전체 UI 관리
//       카드 선택 시 → 선택 카드 강조 + 나머지 2개 비활성화
//       카드 해제 시 → 전체 기본 상태 복구
// ============================================================

using UnityEngine;
using UnityEngine.UI;

public class PlanningUI : MonoBehaviour
{
    public static PlanningUI Instance { get; private set; }

    [Header("UI 연결")]
    public SelectionCardUI[] selectionCards; // Card_Walk, Card_Bus, Card_Taxi
    public Button decideButton;
    public GameObject selectionPanel;

    private SelectionCardUI selectedCard = null;
    private bool isDeciding = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        decideButton.onClick.AddListener(OnDecideButtonClicked);
        SetDecideButtonActive(false);
        HideSelectionCards();
    }

    // 경로 확정 후 허용된 이동수단 카드 표시
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

    // 카드 패널 숨기기 + 전체 초기화
    public void HideSelectionCards()
    {
        selectionPanel.SetActive(false);
        selectedCard = null;
        foreach (SelectionCardUI card in selectionCards)
            card.Hide();
        SetDecideButtonActive(false);
    }

    // 카드 클릭 시 호출
    public void OnCardSelected(SelectionCardUI clickedCard)
    {
        // 같은 카드 다시 클릭 → 선택 해제, 전체 기본 상태 복구
        if (selectedCard == clickedCard)
        {
            MessageSystem.L("이동수단 선택 해제.");
            ResetAllCardsToDefault();
            selectedCard = null;
            SetDecideButtonActive(false);
            return;
        }

        // 다른 카드 클릭
        if (selectedCard != null)
            MessageSystem.L("이동수단 변경.");

        // 선택된 카드 → Selected / 나머지 → Disabled
        selectedCard = clickedCard;
        UpdateCardStates();
        SetDecideButtonActive(true);
    }

    public void SetDecideButtonActive(bool active)
    {
        decideButton.interactable = active;
    }

    private void OnDecideButtonClicked()
    {
        if (isDeciding) return;

        if (GameState.CurrentPhase == Phase.Execution)
        {
            MessageSystem.E("이동 중에는 조작할 수 없습니다!");
            return;
        }
        if (!PlanningManager.Instance.HasPendingRoute && selectedCard == null)
        {
            MessageSystem.E("아직 이동할 노드를 선택하지 않았습니다!");
            return;
        }
        if (PlanningManager.Instance.HasPendingRoute && selectedCard == null)
        {
            MessageSystem.E("먼저 이동수단을 선택해주세요!");
            return;
        }

        isDeciding = true;
        decideButton.interactable = false;
        InputLock.Lock();

        PlanningManager.Instance.OnTransportSelected(selectedCard.TransportType);
        PlanningManager.Instance.OnDecideButtonClicked();
    }

    // 선택 카드 → Selected / 나머지 활성 카드 → Disabled
    private void UpdateCardStates()
    {
        foreach (SelectionCardUI card in selectionCards)
        {
            if (!card.gameObject.activeSelf) continue;

            if (card == selectedCard)
                card.SetSelected();
            else
                card.SetDisabled();
        }
    }

    // 전체 활성 카드 → Default 상태로 복구
    private void ResetAllCardsToDefault()
    {
        foreach (SelectionCardUI card in selectionCards)
        {
            if (!card.gameObject.activeSelf) continue;
            card.SetDefault();
        }
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